using System;
using System.Collections.Generic;
using System.Globalization;
using Path = System.IO.Path;
using System.Text.RegularExpressions;
using Perrinn424.AutopilotSystem;
using UnityEngine;
using VehiclePhysics;

namespace Perrinn424.LiveLink
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Perrinn424CarController))]
    public sealed class UnityLiveLink : MonoBehaviour
    {
        public int port = 42480;
        public bool showConnectionPanel = true;
        public string[] allowedOrigins = { "http://localhost:4200", "http://127.0.0.1:4200", "http://localhost:5004", "https://perrinn.com", "https://www.perrinn.com" };
        [NonSerialized] public string pairingToken;
        public string Status => server == null ? "Stopped" : authenticated != 0 ? "Connected" : "Waiting for browser";
        public string NetworkError => recording?.Error ?? recordingError ?? server?.Error;
        public string RecordingPath => recording?.DirectoryPath;
        SessionRecording recording;
        string recordingError;
        double startedAt;
        const int Rate = 50;
        static readonly string[] Names = { "Speed", "Throttle", "Brake", "SteeringAngle", "RideHeightFront", "RideHeightRear", "UndersteerAngle", "MotorRpmFront", "MotorRpmRear", "ElectricalPowerTotal" };
        readonly Queue<Request> commands = new Queue<Request>();
        readonly Dictionary<string, string> responses = new Dictionary<string, string>();
        readonly Queue<string> responseOrder = new Queue<string>();
        readonly int[] indices = new int[10];
        readonly float[] multipliers = new float[10];
        readonly ChannelDescription[] descriptions = new ChannelDescription[10];

        Perrinn424CarController car;
        Autopilot autopilot;
        WheelColliderBehaviour left, right;
        KinematicWheel.SuspensionDamper leftOriginalDamper, rightOriginalDamper;
        float leftOriginalRate, rightOriginalRate, originalOffset;
        bool springsAvailable, springChanged, offsetChanged;
        bool previousRunInBackground;
        LocalWebSocketServer server;
        int authenticated, observedClient;
        double connectedAt, lastHeartbeat, nextSample, lastPauseSample, previousSimulationTime;
        string session;
        static readonly Regex NumberValue = new Regex("\"value\"\\s*:\\s*(-?(?:0|[1-9]\\d*)(?:\\.\\d+)?(?:[eE][+-]?\\d+)?)(?=\\s*[,}])", RegexOptions.CultureInvariant);

        [Serializable] class Request { public string type; public int protocol; public string token; public string requestId; public string parameter; public float value; public int after; public int eventAfter; public int limit; }
        [Serializable] public class ChannelDescription { public string id; public string unit; public float sourceHz; public bool available; }
        [Serializable] class Parameter { public string id; public string label; public string unit; public float value; public float min; public float max; public bool available; }
        [Serializable] class Welcome { public string type = "welcome"; public int protocol = 2; public string session; public int rateHz = Rate; public ChannelDescription[] channels; public Parameter[] parameters; public string recordingPath; public string recordingError; public int recordedSamples; public int recordedEvents; public bool complete; }
        [Serializable] class Result { public string type = "result"; public string requestId; public string parameter; public bool ok; public float value; public int effectiveFromFrame; public string error; }
        [Serializable] class Failure { public string type = "error"; public string error; }

        void OnEnable()
        {
            previousRunInBackground = Application.runInBackground;
            Application.runInBackground = true;
            car = GetComponent<Perrinn424CarController>();
            autopilot = car.GetComponentInChildren<Autopilot>(true);
            left = car.frontAxle.leftWheel;
            right = car.frontAxle.rightWheel;
            springsAvailable = Supported(left) && Supported(right);
            if (springsAvailable)
            {
                leftOriginalRate = GetRate(left); rightOriginalRate = GetRate(right);
                // Kinematic dampers are reference objects. Clone before changing either one.
                if (left is VPKinematicWheelCollider kl) leftOriginalDamper = kl.suspensionDamper;
                if (right is VPKinematicWheelCollider kr) rightOriginalDamper = kr.suspensionDamper;
            }
            if (autopilot != null) originalOffset = autopilot.positionOffset;
            pairingToken = Guid.NewGuid().ToString("N");
            session = Guid.NewGuid().ToString("N");
            nextSample = previousSimulationTime = lastPauseSample = 0; startedAt = Time.realtimeSinceStartupAsDouble; recordingError = null;
            car.onPreDynamicsStep += ApplyCommands;
            car.onDynamicsStepCompleted += SampleCompletedStep;
            try { server = new LocalWebSocketServer(port, allowedOrigins); }
            catch (Exception ex) { Debug.LogError("Unity Live Link could not start: " + ex.Message, this); enabled = false; }
        }
        static bool Supported(WheelColliderBehaviour wheel) => wheel is VPKinematicWheelCollider || wheel is VPStandardWheelCollider;
        static float GetRate(WheelColliderBehaviour wheel)
        {
            if (wheel is VPKinematicWheelCollider k) return k.suspensionDamper.springRate;
            return ((VPStandardWheelCollider)wheel).springRate;
        }
        static void SetRate(WheelColliderBehaviour wheel, float value)
        {
            if (wheel is VPKinematicWheelCollider k) k.suspensionDamper.springRate = value;
            else ((VPStandardWheelCollider)wheel).springRate = value;
            wheel.RefreshPhysics();
        }
        void Update()
        {
            if (server == null) return;
            double now = Time.realtimeSinceStartupAsDouble;
            int client = server.ClientId;
            if (client != observedClient)
            {
                observedClient = client; authenticated = 0; connectedAt = now;
                commands.Clear(); responses.Clear(); responseOrder.Clear();
            }
            LocalWebSocketServer.Incoming incoming;
            for (int n = 0; n < 16 && server.TryRead(out incoming); n++)
            {
                if (incoming.client != client) continue;
                try
                {
                    var request = JsonUtility.FromJson<Request>(incoming.json);
                    if (request != null && request.type == "setParameter")
                    {
                        var number = NumberValue.Match(incoming.json);
                        float value;
                        if (!number.Success || !float.TryParse(number.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                            throw new ArgumentException();
                        request.value = value;
                    }
                    Handle(incoming.client, request);
                }
                catch (Exception) { server.Reply(client, JsonUtility.ToJson(new Failure { error = "Invalid message" })); }
            }
            if (client != 0 && (authenticated == 0 ? now - connectedAt > 5 : now - lastHeartbeat > 5)) server.Disconnect(client);
            if (Paused)
            {
                ApplyCommands();
                if (now - lastPauseSample >= 1.0 / Rate) { Capture(); lastPauseSample = now; }
            }
        }
        bool Paused => Time.timeScale == 0 || car.paused;
        void Handle(int client, Request request)
        {
            if (request == null) throw new ArgumentException();
            if (authenticated == 0)
            {
                if (request.type != "hello" || request.protocol != 2 || request.token != pairingToken)
                { server.Reply(client, JsonUtility.ToJson(new Failure { error = "Pairing failed: check the session token and protocol" })); return; }
                authenticated = client; lastHeartbeat = Time.realtimeSinceStartupAsDouble;
                SendWelcome();
                return;
            }
            lastHeartbeat = Time.realtimeSinceStartupAsDouble;
            if (request.type == "ping") { server.Reply(client, "{\"type\":\"pong\"}"); return; }
            if (request.type == "history")
            {
                EnsureRecording();
                if (recording == null || request.after < 0 || request.after > recording.Count || request.eventAfter < 0 || request.eventAfter > recording.EventCount) throw new ArgumentException();
                server.Reply(client, recording.History(request.after, request.eventAfter, request.limit));
                return;
            }
            if (request.type != "setParameter" || string.IsNullOrEmpty(request.requestId) || request.requestId.Length > 64)
                throw new ArgumentException();
            string previous;
            if (responses.TryGetValue(request.requestId, out previous)) { server.Reply(client, previous); return; }
            foreach (var queued in commands) if (queued.requestId == request.requestId) return;
            if (commands.Count >= 16) { server.Disconnect(client); return; }
            commands.Enqueue(request);
        }
        void SampleCompletedStep()
        {
            if (server == null || car == null) return;
            // VPP invokes this immediately after updating its telemetry row.
            double now = car.telemetry.latest.totalTime;
            if (now < previousSimulationTime) nextSample = 0;
            previousSimulationTime = now;
            if (now + 0.000001 < nextSample) return;
            nextSample = now + 1.0 / Rate;
            Capture();
        }
        void ApplyCommands()
        {
            while (commands.Count > 0)
            {
                var request = commands.Dequeue();
                var result = new Result { requestId = request.requestId, parameter = request.parameter, effectiveFromFrame = car.telemetry.latest.frame + 1 };
                try
                {
                    if (float.IsNaN(request.value) || float.IsInfinity(request.value)) throw new ArgumentException("Value must be finite");
                    if (request.parameter == "frontSpringRate")
                    {
                        if (!springsAvailable) throw new ArgumentException("Supported front wheel components are unavailable");
                        if (request.value < 10000 || request.value > 1000000) throw new ArgumentException("Spring rate must be between 10000 and 1000000 N/m");
                        if (!springChanged)
                        {
                            if (left is VPKinematicWheelCollider kl) kl.suspensionDamper = JsonUtility.FromJson<KinematicWheel.SuspensionDamper>(JsonUtility.ToJson(leftOriginalDamper));
                            if (right is VPKinematicWheelCollider kr) kr.suspensionDamper = JsonUtility.FromJson<KinematicWheel.SuspensionDamper>(JsonUtility.ToJson(rightOriginalDamper));
                            springChanged = true;
                        }
                        float previousLeft = GetRate(left), previousRight = GetRate(right);
                        try { SetRate(left, request.value); SetRate(right, request.value); }
                        catch { SetRate(left, previousLeft); SetRate(right, previousRight); throw; }
                        result.value = left.api is KinematicWheelCollider k ? k.suspensionDamper.springRate : ((StandardWheelCollider)left.api).springRate;
                    }
                    else if (request.parameter == "autopilotPositionOffset")
                    {
                        if (autopilot == null) throw new ArgumentException("Autopilot is unavailable");
                        if (request.value < -10 || request.value > 10) throw new ArgumentException("Position offset must be between -10 and 10 m");
                        autopilot.positionOffset = request.value; offsetChanged = true;
                        result.value = autopilot.positionOffset;
                    }
                    else throw new ArgumentException("Unknown parameter");
                    result.ok = true;
                    EnsureRecording();
                    recording?.AddChange("{\"id\":" + (recording.EventCount + 1) + ",\"time\":" + (Time.realtimeSinceStartupAsDouble - startedAt).ToString("R", CultureInfo.InvariantCulture) + ",\"sequence\":" + (recording.Count + 1) + ",\"result\":" + JsonUtility.ToJson(result) + "}");
                }
                catch (Exception ex) { result.error = ex.Message; }
                var reply = JsonUtility.ToJson(result);
                responses[request.requestId] = reply; responseOrder.Enqueue(request.requestId);
                if (responseOrder.Count > 32) responses.Remove(responseOrder.Dequeue());
                server.Reply(authenticated, reply);
            }
        }
        void ResolveChannels()
        {
            for (int n = 0; n < Names.Length; n++)
            {
                int index = car.telemetry.GetChannelIndex(Names[n]);
                indices[n] = index;
                bool active = index >= 0 && car.telemetry.channels[index].group.instance != null;
                var semantic = index >= 0 ? car.telemetry.GetChannelSemmantic(index) : null;
                multipliers[n] = semantic == null ? 1 : semantic.displayMultiplier;
                descriptions[n] = new ChannelDescription { id = Names[n], available = active, unit = semantic == null ? "" : semantic.displayUnits.Trim(), sourceHz = index < 0 ? 0 : car.telemetry.channels[index].group.actualFrequency };
            }
        }
        Welcome DescribeSession()
        {
            ResolveChannels();
            var welcome = new Welcome { session = session, recordingPath = RecordingPath, recordingError = NetworkError, recordedSamples = recording == null ? 0 : recording.Count, recordedEvents = recording == null ? 0 : recording.EventCount, complete = false, channels = descriptions, parameters = new[] {
                new Parameter { id = "frontSpringRate", label = "Front spring rate (both wheels)", unit = "N/m", value = springsAvailable ? GetRate(left) : 0, min = 10000, max = 1000000, available = springsAvailable },
                new Parameter { id = "autopilotPositionOffset", label = "Autopilot position offset", unit = "m", value = autopilot == null ? 0 : autopilot.positionOffset, min = -10, max = 10, available = autopilot != null }
            } };
            return welcome;
        }
        void EnsureRecording()
        {
            if (recording != null || recordingError != null) return;
            try
            {
                string path = Path.Combine(Application.persistentDataPath, "LiveLink", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + session);
                var metadata = DescribeSession(); metadata.recordingPath = path;
                recording = new SessionRecording(path, JsonUtility.ToJson(metadata));
            }
            catch (Exception ex) { recordingError = "Cannot create recording: " + ex.Message; Debug.LogError(recordingError, this); }
        }
        void SendWelcome()
        {
            EnsureRecording();
            server.Reply(authenticated, JsonUtility.ToJson(DescribeSession()));
        }
        void Capture()
        {
            EnsureRecording();
            if (recording == null) return;
            var row = car.telemetry.latest;
            var values = new float[10];
            for (int n = 0; n < Names.Length; n++)
            {
                int index = indices[n];
                values[n] = index < 0 || index >= row.values.Length || car.telemetry.channels[index].group.instance == null ? float.NaN : row.values[index] * multipliers[n];
            }
            var sample = new SessionRecording.Sample { frame = row.frame, time = Time.realtimeSinceStartupAsDouble - startedAt, simulationTime = row.totalTime, paused = Paused, values = values };
            recording.Append(sample);
            if (authenticated != 0 && authenticated == server.ClientId)
                server.Publish(authenticated, "{\"type\":\"telemetry\",\"recordingError\":" + JsonUtility.ToJson(new Failure { error = NetworkError }) + ",\"sample\":" + sample.ToJson() + "}");
        }

        void OnGUI()
        {
            if (!showConnectionPanel || !Application.isPlaying || string.IsNullOrEmpty(pairingToken)) return;
            GUILayout.BeginArea(new Rect(Mathf.Max(0, Screen.width - 350), 10, 340, 125), GUI.skin.box);
            GUILayout.Label("Unity Live Link · " + Status);
            GUILayout.Label("Session token: " + pairingToken);
            if (GUILayout.Button("Copy pairing token")) GUIUtility.systemCopyBuffer = pairingToken;
            GUILayout.Label("localhost:" + port + " · 50 Hz target");
            GUILayout.EndArea();
        }
        void OnDisable()
        {
            Application.runInBackground = previousRunInBackground;
            recording?.Dispose();
            if (!string.IsNullOrEmpty(recording?.Error)) Debug.LogError(recording.Error, this);
            recording = null;
            server?.Dispose(); server = null; commands.Clear(); authenticated = observedClient = 0;
            if (car != null) { car.onPreDynamicsStep -= ApplyCommands; car.onDynamicsStepCompleted -= SampleCompletedStep; }
            if (springChanged)
            {
                if (left != null) { if (left is VPKinematicWheelCollider kl) kl.suspensionDamper = leftOriginalDamper; SetRate(left, leftOriginalRate); }
                if (right != null) { if (right is VPKinematicWheelCollider kr) kr.suspensionDamper = rightOriginalDamper; SetRate(right, rightOriginalRate); }
            }
            if (offsetChanged && autopilot != null) autopilot.positionOffset = originalOffset;
            springChanged = offsetChanged = false; pairingToken = null;
        }
    }
}
