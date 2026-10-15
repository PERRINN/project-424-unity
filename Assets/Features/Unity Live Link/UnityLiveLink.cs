using System;
using System.Collections.Generic;
using System.Globalization;
using Path = System.IO.Path;
using System.Text.RegularExpressions;
using Perrinn424.AutopilotSystem;
using UnityEngine;
using VehiclePhysics;
using VehiclePhysics.Timing;

namespace Perrinn424.LiveLink
{
    [AddComponentMenu("PERRINN/424 Telemetry")]
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
        static readonly string[] Names = { "Speed", "Throttle", "Brake", "SteeringAngle", "RideHeightFront", "RideHeightRear", "UndersteerAngle", "MotorRpmFront", "MotorRpmRear", "ElectricalPowerTotal", "PositionX", "PositionY", "PositionZ",
            "ElectricalPowerFront", "ElectricalPowerRear", "RpmDiffFront", "RpmDiffRear",
            "AccelerationLat", "AccelerationLong", "AccelerationVert", "WheelLoadFL", "WheelLoadFR", "WheelLoadRL", "WheelLoadRR",
            "PowerBalanceDelivered", "WheelTorqueBalance", "AeroRoll", "AeroScx", "AeroSczFront", "AeroSczRear", "AeroBalance", "LapDistance", "LapTrigger", "SectorTrigger", "SectorIndex", "SectorTime", "SectorValid", "LapTime", "LapValid", "LapCompleteTrigger" };
        readonly Queue<Request> commands = new Queue<Request>();
        readonly Dictionary<string, string> responses = new Dictionary<string, string>();
        readonly Queue<string> responseOrder = new Queue<string>();
        readonly int[] indices = new int[Names.Length];
        readonly float[] multipliers = new float[Names.Length];
        readonly ChannelDescription[] descriptions = new ChannelDescription[Names.Length];

        Perrinn424CarController car;
        Rigidbody body;
        LapTimer lapTimer;
        int lapTrigger, sectorTrigger, lapCompleteTrigger;
        float sectorIndex, sectorTime = float.NaN, sectorValid = float.NaN, lapTime = float.NaN, lapValid = float.NaN;
        void BeginLap()
        {
            if (sessionComplete) return;
            lapTrigger++;
            if (sessionPlan != null) ApplyLapSetup();
            CaptureTiming();
        }
        void Sector(int index, float time)
        {
            sectorTrigger++; sectorIndex = index; sectorTime = time;
            sectorValid = index > 0 && index <= lapTimer.currentValidSectors.Count && lapTimer.currentValidSectors[index - 1] ? 1 : 0;
            CaptureTiming();
        }
        void CompleteLap(float time, bool valid, float[] sectors, bool[] validity)
        {
            lapCompleteTrigger++; lapTime = time; lapValid = valid ? 1 : 0; CaptureTiming();
            // Ignore the initial partial lap when the bridge was attached mid-lap.
            if (sessionPlan != null && lapTrigger >= 3) FinishSession();
        }
        // Capture every timing event as well as regular samples: short trigger pulses cannot be lost.
        void CaptureTiming() { if (server != null && car != null) Capture(); }
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
        bool stopping, sessionComplete;
        double completedAt;
        LapChange[] sessionPlan;
        [Serializable] public class LapChange { public string parameter; public string mode; public float amount; }
        [Serializable] class SessionStatus { public string type = "sessionComplete"; public int total; public int eventTotal; }
        public void ConfigureSession(LapChange[] plan)
        {
            if (lapTimer == null) throw new InvalidOperationException("A LapTimer is required for a three-lap session.");
            if (lapTrigger != 0) throw new InvalidOperationException("Start a new simulation to run a three-lap session.");
            if (plan == null || plan.Length != 2) throw new ArgumentException("Two test lap setups are required.");
            foreach (var change in plan)
            {
                if (change == null || string.IsNullOrEmpty(change.parameter)) continue;
                if (change.mode != "offset" && change.mode != "gain") throw new ArgumentException("Unknown change mode.");
                float baseline = change.parameter == "frontSpringRate" && springsAvailable ? leftOriginalRate : change.parameter == "autopilotPositionOffset" && autopilot != null ? originalOffset : float.NaN;
                float target = change.mode == "gain" ? baseline * change.amount : baseline + change.amount;
                if (float.IsNaN(target) || float.IsInfinity(target) || (change.parameter == "frontSpringRate" ? target < 10000 || target > 1000000 : target < -10 || target > 10))
                    throw new ArgumentException("Test lap setup is outside the parameter range.");
            }
            sessionPlan = plan;
        }
        void ApplyLapSetup()
        {
            var change = lapTrigger >= 2 && lapTrigger <= 3 ? sessionPlan[lapTrigger - 2] : null;
            // Restore the parameter used by the previous test when selecting a different one.
            if (springChanged && change?.parameter != "frontSpringRate")
            {
                SetRate(left, leftOriginalRate); SetRate(right, rightOriginalRate);
                RecordChange(new Result { requestId = "lap-" + lapTrigger + "-reset-springs", parameter = "frontSpringRate", value = leftOriginalRate, ok = true, effectiveFromFrame = car.telemetry.latest.frame + 1 });
            }
            if (offsetChanged && autopilot != null && change?.parameter != "autopilotPositionOffset")
            {
                autopilot.positionOffset = originalOffset;
                RecordChange(new Result { requestId = "lap-" + lapTrigger + "-reset-offset", parameter = "autopilotPositionOffset", value = originalOffset, ok = true, effectiveFromFrame = car.telemetry.latest.frame + 1 });
            }
            if (change == null || string.IsNullOrEmpty(change.parameter)) return;
            float baseline = change.parameter == "frontSpringRate" ? leftOriginalRate : originalOffset;
            commands.Enqueue(new Request { type = "setParameter", requestId = "lap-" + lapTrigger, parameter = change.parameter,
                value = change.mode == "gain" ? baseline * change.amount : baseline + change.amount });
            ApplyCommands();
        }
        void FinishSession()
        {
            sessionComplete = true; completedAt = Time.realtimeSinceStartupAsDouble; car.paused = true;
            if (authenticated != 0) SendSessionComplete();
        }
        void SendSessionComplete() { server.Reply(authenticated, JsonUtility.ToJson(new SessionStatus { total = recording == null ? 0 : recording.Count, eventTotal = recording == null ? 0 : recording.EventCount })); }
        void StopRun(int client, string requestId)
        {
            if (!sessionComplete) Capture();
            stopping = true; car.paused = true; commands.Clear();
            recording?.Dispose(); recordingError = recording?.Error ?? recordingError; recording = null;
            stopReply = client == 0 ? System.Threading.Tasks.Task.FromResult(true) : server.ReplyWithReceipt(client, JsonUtility.ToJson(new Stopped { requestId = requestId, recordingError = recordingError }));
        }
        System.Threading.Tasks.Task<bool> stopReply;
        static readonly Regex NumberValue = new Regex("\"value\"\\s*:\\s*(-?(?:0|[1-9]\\d*)(?:\\.\\d+)?(?:[eE][+-]?\\d+)?)(?=\\s*[,}])", RegexOptions.CultureInvariant);

        [Serializable] class Request { public string type; public int protocol; public string token; public string requestId; public string parameter; public float value; public int after; public int eventAfter; public int limit; }
        [Serializable] public class ChannelDescription { public string id; public string unit; public float sourceHz; public bool available; }
        [Serializable] class Parameter { public string id; public string label; public string unit; public float value; public float min; public float max; public bool available; }
        [Serializable] class Welcome { public string type = "welcome"; public int protocol = 2; public string session; public int rateHz = Rate; public bool canStopSimulation = true; public ChannelDescription[] channels; public Parameter[] parameters; public Parameter[] baselineParameters; public LapChange[] lapPlan; public string recordingPath; public string recordingError; public int recordedSamples; public int recordedEvents; public bool complete; }
        [Serializable] class Result { public string type = "result"; public string requestId; public string parameter; public bool ok; public float value; public int effectiveFromFrame; public string error; }
        [Serializable] class Stopped { public string type = "simulationStopped"; public string requestId; public string recordingError; }
        [Serializable] class Failure { public string type = "error"; public string error; }

        void OnEnable()
        {
            stopping = sessionComplete = false; sessionPlan = null; stopReply = null;
            previousRunInBackground = Application.runInBackground;
            Application.runInBackground = true;
            car = GetComponent<Perrinn424CarController>();
            body = GetComponent<Rigidbody>();
            lapTimer = FindObjectOfType<LapTimer>();
            lapTrigger = sectorTrigger = lapCompleteTrigger = 0;
            sectorIndex = 0; sectorTime = sectorValid = lapTime = lapValid = float.NaN;
            if (lapTimer != null) { lapTimer.onBeginLap += BeginLap; lapTimer.onSector += Sector; lapTimer.onLap += CompleteLap; }
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
            catch (Exception ex) { Debug.LogError("424 Telemetry could not start: " + ex.Message, this); enabled = false; }
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
            if (stopping)
            {
                if (stopReply == null || !stopReply.IsCompleted) return;
                // Finish all runtime network work before Unity unloads this script domain.
                // The recording is finalized and the acknowledgement has left the socket worker.
                server.Dispose(); server = null;
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                enabled = false;
                return;
            }
            double now = Time.realtimeSinceStartupAsDouble;
            // Physics stopped at the finish line. Allow the browser to recover the final rows before exiting.
            if (sessionComplete && now - completedAt > 15) { StopRun(0, ""); return; }
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
                    if (stopping) return;
                }
                catch (Exception) { server.Reply(client, JsonUtility.ToJson(new Failure { error = "Invalid message" })); }
            }
            if (client != 0 && (authenticated == 0 ? now - connectedAt > 5 : now - lastHeartbeat > 5)) server.Disconnect(client);
            if (Paused)
            {
                ApplyCommands();
                if (!sessionComplete && now - lastPauseSample >= 1.0 / Rate) { Capture(); lastPauseSample = now; }
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
            if (string.IsNullOrEmpty(request.requestId) || request.requestId.Length > 64) throw new ArgumentException();
            if (request.type == "stopSimulation")
            {
                StopRun(client, request.requestId);
                return;
            }
            if (request.type != "setParameter" || sessionPlan != null) throw new ArgumentException();
            string previous;
            if (responses.TryGetValue(request.requestId, out previous)) { server.Reply(client, previous); return; }
            foreach (var queued in commands) if (queued.requestId == request.requestId) return;
            if (commands.Count >= 16) { server.Disconnect(client); return; }
            commands.Enqueue(request);
        }
        void SampleCompletedStep()
        {
            if (stopping || sessionComplete || server == null || car == null) return;
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
            if (stopping) return;
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
                    RecordChange(result);
                }
                catch (Exception ex) { result.error = ex.Message; }
                var reply = JsonUtility.ToJson(result);
                responses[request.requestId] = reply; responseOrder.Enqueue(request.requestId);
                if (responseOrder.Count > 32) responses.Remove(responseOrder.Dequeue());
                server.Reply(authenticated, reply);
            }
        }
        void RecordChange(Result result)
        {
            EnsureRecording();
            recording?.AddChange("{\"id\":" + (recording.EventCount + 1) + ",\"time\":" + (Time.realtimeSinceStartupAsDouble - startedAt).ToString("R", CultureInfo.InvariantCulture) + ",\"sequence\":" + (recording.Count + 1) + ",\"result\":" + JsonUtility.ToJson(result) + "}");
        }
        void ResolveChannels()
        {
            for (int n = 0; n < Names.Length; n++)
            {
                if (n >= 10 && n <= 12) { descriptions[n] = new ChannelDescription { id = Names[n], available = true, unit = "m", sourceHz = Rate }; continue; }
                if (n >= 32) { descriptions[n] = new ChannelDescription { id = Names[n], available = lapTimer != null, unit = n == 35 || n == 37 ? "s" : "", sourceHz = Rate }; continue; }
                int index = car.telemetry.GetChannelIndex(Names[n]);
                indices[n] = index;
                bool active = index >= 0 && car.telemetry.channels[index].group.instance != null;
                var semantic = index >= 0 ? car.telemetry.GetChannelSemmantic(index) : null;
                multipliers[n] = semantic == null ? 1 : semantic.displayMultiplier;
                string unit = semantic == null ? "" : semantic.displayUnits.Trim();
                if (Names[n] == "LapDistance") { multipliers[n] = 1; unit = "m"; }
                if (Names[n].StartsWith("WheelLoad", StringComparison.Ordinal)) { multipliers[n] = 1; unit = "N"; }
                if (Names[n] == "AeroScx" || Names[n] == "AeroSczFront" || Names[n] == "AeroSczRear") unit = "m²";
                descriptions[n] = new ChannelDescription { id = Names[n], available = active, unit = unit, sourceHz = index < 0 ? 0 : car.telemetry.channels[index].group.actualFrequency };
            }
        }
        Welcome DescribeSession()
        {
            ResolveChannels();
            var welcome = new Welcome { lapPlan = sessionPlan, session = session, recordingPath = RecordingPath, recordingError = NetworkError, recordedSamples = recording == null ? 0 : recording.Count, recordedEvents = recording == null ? 0 : recording.EventCount, complete = false, channels = descriptions, parameters = new[] {
                new Parameter { id = "frontSpringRate", label = "Front spring rate (both wheels)", unit = "N/m", value = springsAvailable ? GetRate(left) : 0, min = 10000, max = 1000000, available = springsAvailable },
                new Parameter { id = "autopilotPositionOffset", label = "Autopilot position offset", unit = "m", value = autopilot == null ? 0 : autopilot.positionOffset, min = -10, max = 10, available = autopilot != null }
            } };
            welcome.baselineParameters = new[] {
                new Parameter { id = "frontSpringRate", label = "Front spring rate (both wheels)", unit = "N/m", value = leftOriginalRate, min = 10000, max = 1000000, available = springsAvailable },
                new Parameter { id = "autopilotPositionOffset", label = "Autopilot position offset", unit = "m", value = originalOffset, min = -10, max = 10, available = autopilot != null }
            };
            return welcome;
        }
        void EnsureRecording()
        {
            if (recording != null || recordingError != null) return;
            try
            {
                string path = Path.Combine(Application.persistentDataPath, "LiveLink", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + session);
                var metadata = DescribeSession(); metadata.recordingPath = path;
                recording = new SessionRecording(path, JsonUtility.ToJson(metadata), Names.Length);
            }
            catch (Exception ex) { recordingError = "Cannot create recording: " + ex.Message; Debug.LogError(recordingError, this); }
        }
        void SendWelcome()
        {
            EnsureRecording();
            server.Reply(authenticated, JsonUtility.ToJson(DescribeSession()));
            if (sessionComplete) SendSessionComplete();
        }
        void Capture()
        {
            if (stopping) return;
            EnsureRecording();
            if (recording == null) return;
            var row = car.telemetry.latest;
            var values = new float[Names.Length];
            for (int n = 0; n < Names.Length; n++)
            {
                if (n >= 10 && n <= 12 || n >= 32) continue;
                int index = indices[n];
                values[n] = index < 0 || index >= row.values.Length || car.telemetry.channels[index].group.instance == null ? float.NaN : row.values[index] * multipliers[n];
            }
            // Front / signed total: zero totals are undefined, including when both axles are idle.
            if (descriptions[24].available) values[24] = TelemetryRatios.FrontPercent(values[13], values[14]);
            if (descriptions[25].available)
            {
                int[] custom = car.data.Get(Channel.Custom);
                values[25] = TelemetryRatios.FrontPercent(custom[Perrinn424Data.FrontMguBase + Perrinn424Data.WheelsTorque], custom[Perrinn424Data.RearMguBase + Perrinn424Data.WheelsTorque]);
            }
            // Unity world coordinates; X/Z form the ground plane and Y is elevation.
            Vector3 position = body != null ? body.position : car.transform.position;
            values[10] = position.x; values[11] = position.y; values[12] = position.z;
            values[32] = lapTimer == null ? float.NaN : lapTrigger;
            values[33] = lapTimer == null ? float.NaN : sectorTrigger;
            values[34] = lapTimer == null ? float.NaN : sectorIndex;
            values[35] = sectorTime; values[36] = sectorValid;
            values[37] = lapTime; values[38] = lapValid;
            values[39] = lapTimer == null ? float.NaN : lapCompleteTrigger;
            var sample = new SessionRecording.Sample { frame = row.frame, time = Time.realtimeSinceStartupAsDouble - startedAt, simulationTime = row.totalTime, paused = Paused, values = values };
            recording.Append(sample);
            if (authenticated != 0 && authenticated == server.ClientId)
                server.Publish(authenticated, "{\"type\":\"telemetry\",\"recordingError\":" + JsonUtility.ToJson(new Failure { error = NetworkError }) + ",\"sample\":" + sample.ToJson() + "}");
        }

        void OnGUI()
        {
            if (!showConnectionPanel || !Application.isPlaying || string.IsNullOrEmpty(pairingToken)) return;
            GUILayout.BeginArea(new Rect(Mathf.Max(0, Screen.width - 350), 10, 340, 125), GUI.skin.box);
            GUILayout.Label("424 Telemetry · " + Status);
            GUILayout.Label("Session token: " + pairingToken);
            if (GUILayout.Button("Copy pairing token")) GUIUtility.systemCopyBuffer = pairingToken;
            GUILayout.Label("localhost:" + port + " · 50 Hz target");
            GUILayout.EndArea();
        }
        void OnDisable()
        {
            if (lapTimer != null) { lapTimer.onBeginLap -= BeginLap; lapTimer.onSector -= Sector; lapTimer.onLap -= CompleteLap; }
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
