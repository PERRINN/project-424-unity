using System;
using Perrinn424.AutopilotSystem;
using UnityEditor;
using UnityEngine;
using VehiclePhysics.InputManagement;

namespace Perrinn424.LiveLink
{
    // Editor-owned listener survives outside Play Mode; SessionState carries a start across domain reloads.
    [InitializeOnLoad]
    public static class TelemetryEditorControl
    {
        const string Prefix = "Perrinn424.Telemetry.Editor.";
        const int Port = 42482;
        static readonly string[] Origins = { "http://localhost:4200", "http://127.0.0.1:4200", "http://localhost:5004", "https://perrinn.com", "https://www.perrinn.com" };
        static LocalWebSocketServer server;
        static int observed, authenticated;
        static Perrinn424CarController runtimeCar;
        static double connectedAt, lastMessage, retryAt;
        static string TokenKey => Prefix + Application.dataPath;
        static string Token
        {
            get
            {
                var value = EditorPrefs.GetString(TokenKey, "");
                if (string.IsNullOrEmpty(value)) { value = Guid.NewGuid().ToString("N"); EditorPrefs.SetString(TokenKey, value); }
                return value;
            }
        }
        [Serializable] class Request { public string type; public string token; public string requestId; public UnityLiveLink.LapChange[] laps; }
        [Serializable] class Status
        {
            public string type = "editorStatus";
            public string requestId;
            public string phase;
            public string error;
            public string token;
            public bool autopilot;
        }
        static TelemetryEditorControl()
        {
            // A start accepted just before compilation is still an app-owned new run.
            if (SessionState.GetBool(Prefix + "pending", false)) SessionState.SetBool(Prefix + "performanceRequested", true);
            EditorApplication.update += Update;
            EditorApplication.playModeStateChanged += state => {
                if (state == PlayModeStateChange.ExitingPlayMode)
                {
                    SessionState.SetBool(Prefix + "performanceRequested", false);
                    // Restore worker/render settings only once Unity has finished tearing down Play Mode.
                }
                if (state == PlayModeStateChange.EnteredEditMode)
                {
                    TelemetrySimulationPerformance.Restore(false);
                    SessionState.SetBool(Prefix + "performanceRequested", false);
                    SessionState.SetBool(Prefix + "pending", false); SessionState.SetString(Prefix + "phase", "idle");
                    RestoreSimulationSettings();
                }
            };
            AssemblyReloadEvents.beforeAssemblyReload += Dispose;
            EditorApplication.quitting += Dispose;
        }
        [MenuItem("PERRINN/Copy 424 Telemetry Editor pairing token")]
        static void CopyToken() { EditorGUIUtility.systemCopyBuffer = Token; }
        static void Dispose() { server?.Dispose(); server = null; authenticated = observed = 0; }
        static void Update()
        {
            double now = EditorApplication.timeSinceStartup;
            if (server == null && now >= retryAt)
            {
                try { server = new LocalWebSocketServer(Port, Origins); }
                catch { retryAt = now + 5; return; }
            }
            if (server == null) return;
            int client = server.ClientId;
            if (client != observed) { observed = client; authenticated = 0; connectedAt = lastMessage = now; }
            LocalWebSocketServer.Incoming incoming;
            for (int n = 0; n < 16 && server.TryRead(out incoming); n++)
            {
                if (incoming.client != client || client == 0) continue;
                try
                {
                    var request = JsonUtility.FromJson<Request>(incoming.json);
                    if (request == null) throw new ArgumentException("Invalid Editor control message.");
                    if (authenticated == 0)
                    {
                        // Presence reveals no token or simulation data and never authorizes a start.
                        if (request.type == "editorProbe") { server.Reply(client, "{\"type\":\"editorAvailable\"}"); continue; }
                        if (request.type != "editorHello" || request.token != Token) throw new ArgumentException("Editor pairing failed. Copy the Editor pairing token from PERRINN in Unity.");
                        authenticated = client; lastMessage = now;
                        server.Reply(client, "{\"type\":\"editorReady\"}"); continue;
                    }
                    lastMessage = now;
                    if (request.type == "editorStatus") { ReplyStatus(client); continue; }
                    if (request.type != "startSimulation" || string.IsNullOrEmpty(request.requestId) || request.requestId.Length > 64) throw new ArgumentException("Invalid start request.");
                    if (request.requestId == SessionState.GetString(Prefix + "request", "")) { ReplyStatus(client); continue; }
                    if (SessionState.GetBool(Prefix + "pending", false)) throw new ArgumentException("A simulation is already starting.");
                    var car = FindCar(); ValidateAutopilot(car);
                    SessionState.SetBool(Prefix + "performanceRequested", true);
                    SessionState.SetString(Prefix + "plan", JsonUtility.ToJson(request));
                    SessionState.SetString(Prefix + "car", GlobalObjectId.GetGlobalObjectIdSlow(car).ToString());
                    SessionState.SetString(Prefix + "request", request.requestId);
                    SessionState.SetString(Prefix + "error", ""); SessionState.SetString(Prefix + "phase", "starting");
                    SessionState.SetString(Prefix + "deadline", DateTime.UtcNow.AddSeconds(90).Ticks.ToString());
                    SessionState.SetBool(Prefix + "pending", true);
                    ReplyStatus(client);
                    if (!EditorApplication.isPlayingOrWillChangePlaymode) EditorApplication.EnterPlaymode();
                }
                catch (Exception ex) { server.Reply(client, JsonUtility.ToJson(new Status { phase = "error", error = ex.Message })); }
            }
            if (client != 0 && (authenticated == 0 ? now - connectedAt > 5 : now - lastMessage > 10)) server.Disconnect(client);
            if (EditorApplication.isPlaying && (SessionState.GetBool(Prefix + "pending", false) || SessionState.GetString(Prefix + "phase", "") == "running"))
            {
                try { ApplySimulationSettings(); }
                catch (Exception ex) { Fail("Simulation settings could not be applied: " + ex.Message); return; }
            }
            if (!SessionState.GetBool(Prefix + "pending", false)) return;
            long deadline;
            if (long.TryParse(SessionState.GetString(Prefix + "deadline", "0"), out deadline) && DateTime.UtcNow.Ticks > deadline)
            { Fail("Unity did not finish starting within 90 seconds."); return; }
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            try
            {
                var car = FindCar(true); var autopilot = ValidateAutopilot(car);
                // The browser remains focused: allow the first dynamics row before attaching the runtime bridge.
                if (car.telemetry.latest.frame <= 0) { EditorApplication.isPaused = false; car.paused = false; return; }
                var bridge = car.GetComponent<UnityLiveLink>();
                if (bridge == null) bridge = car.gameObject.AddComponent<UnityLiveLink>();
                bridge.enabled = true;
                var plan = JsonUtility.FromJson<Request>(SessionState.GetString(Prefix + "plan", "{}"));
                bridge.ConfigureSession(plan.laps ?? new[] { new UnityLiveLink.LapChange(), new UnityLiveLink.LapChange() });
                if (!bridge.enabled || string.IsNullOrEmpty(bridge.pairingToken)) throw new InvalidOperationException("The telemetry bridge could not start. Check the Unity Console.");
                EditorApplication.isPaused = false; car.paused = false;
                if (!autopilot.IsOn) autopilot.ToggleStatus();
                if (!autopilot.IsOn) throw new InvalidOperationException("Autopilot refused to start. Check the car's alignment with its recorded lap.");
                InputManager.instance.runtime.disableForceFeedback = true;
                SessionState.SetBool(Prefix + "pending", false); SessionState.SetString(Prefix + "phase", "running");
                if (authenticated != 0) ReplyStatus(authenticated);
            }
            catch (Exception ex) { Fail(ex.Message); }
        }
        static void ApplySimulationSettings()
        {
            if (!SessionState.GetBool(Prefix + "backgroundManaged", false))
            {
                SessionState.SetBool(Prefix + "backgroundOriginal", Application.runInBackground);
                SessionState.SetFloat(Prefix + "volumeOriginal", AudioListener.volume);
                SessionState.SetFloat(Prefix + "timeScaleOriginal", Time.timeScale);
                SessionState.SetBool(Prefix + "backgroundManaged", true);
            }
            Application.runInBackground = true;
            AudioListener.volume = 0;
            Time.timeScale = 10;
            if (SessionState.GetBool(Prefix + "performanceRequested", false))
            {
                TelemetrySimulationPerformance.Apply();
                var car = FindCar(true); var bridge = car.GetComponent<UnityLiveLink>();
                if (bridge != null) { TelemetrySimulationPerformance.ConfigureBridge(bridge); TelemetrySimulationPerformance.WriteReport(bridge.RecordingPath); }
            }
        }
        static void RestoreSimulationSettings()
        {
            runtimeCar = null;
            if (!SessionState.GetBool(Prefix + "backgroundManaged", false)) return;
            Application.runInBackground = SessionState.GetBool(Prefix + "backgroundOriginal", false);
            AudioListener.volume = SessionState.GetFloat(Prefix + "volumeOriginal", 1);
            Time.timeScale = SessionState.GetFloat(Prefix + "timeScaleOriginal", 1);
            SessionState.SetBool(Prefix + "backgroundManaged", false);
        }
        static Perrinn424CarController FindCar(bool runtime = false)
        {
            if (runtime && runtimeCar != null) return runtimeCar;
            if (runtime && GlobalObjectId.TryParse(SessionState.GetString(Prefix + "car", ""), out var id))
            {
                var saved = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) as Perrinn424CarController;
                if (saved != null) { runtimeCar = saved; return saved; }
            }
            var cars = UnityEngine.Object.FindObjectsOfType<Perrinn424CarController>();
            var selected = !runtime && Selection.activeGameObject != null ? Selection.activeGameObject.GetComponentInParent<Perrinn424CarController>() : null;
            if (selected != null) return selected;
            if (cars.Length != 1) throw new InvalidOperationException("Open a driving scene with one 424 car, or select the intended car before starting.");
            if (runtime) runtimeCar = cars[0];
            return cars[0];
        }
        static Autopilot ValidateAutopilot(Perrinn424CarController car)
        {
            var autopilot = car.GetComponentInChildren<Autopilot>(true);
            if (autopilot == null || !autopilot.isActiveAndEnabled || autopilot.recordedLap == null || autopilot.recordedLap.samples == null || autopilot.recordedLap.Count < 2 || autopilot.recordedLap.frequency <= 0)
                throw new InvalidOperationException("The car needs an enabled Autopilot with a valid recorded lap.");
            return autopilot;
        }
        static void Fail(string error)
        {
            TelemetrySimulationPerformance.Restore(true);
            RestoreSimulationSettings();
            SessionState.SetBool(Prefix + "performanceRequested", false);
            SessionState.SetBool(Prefix + "pending", false); SessionState.SetString(Prefix + "phase", "error"); SessionState.SetString(Prefix + "error", error);
            if (authenticated != 0) ReplyStatus(authenticated);
        }
        static void ReplyStatus(int client)
        {
            var status = new Status { requestId = SessionState.GetString(Prefix + "request", ""), phase = SessionState.GetString(Prefix + "phase", "idle"), error = SessionState.GetString(Prefix + "error", "") };
            if (status.phase == "running" && EditorApplication.isPlaying)
            {
                var car = FindCar(true); var bridge = car.GetComponent<UnityLiveLink>();
                status.autopilot = ValidateAutopilot(car).IsOn;
                if (status.autopilot && bridge != null && bridge.enabled) status.token = bridge.pairingToken;
                else { status.phase = "error"; status.error = "Autopilot or telemetry stopped before connecting."; }
            }
            server.Reply(client, JsonUtility.ToJson(status));
        }
    }
}
