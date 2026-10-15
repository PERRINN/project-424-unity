// Executes the real Editor coordinator against simulated Unity lifecycle APIs.
// Transport behavior is covered separately by the real socket harness.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web.Script.Serialization;
using Perrinn424.LiveLink;

namespace UnityEngine
{
    public class Object
    {
        public static readonly List<Object> Instances = new List<Object>();
        readonly int id;
        public Object() { id = Instances.Count + 1; Instances.Add(this); }
        public int GetInstanceID() { return id; }
        public static T[] FindObjectsOfType<T>() { return Instances.Where(o => !(o is Behaviour) || ((Behaviour)o).gameObject.activeInHierarchy).OfType<T>().ToArray(); }
    }
    public class Behaviour : Object { public bool enabled = true; public GameObject gameObject = new GameObject(); }
    public class Camera : Behaviour { }
    public class Canvas : Behaviour { }
    public class AudioSource : Behaviour { }
    public static class QualitySettings { public static int vSyncCount = 2; }
    public static class Debug { public static void LogWarning(string message) { throw new Exception(message); } }
    public class GameObject
    {
        public bool activeInHierarchy = true;
        public Scene scene = new Scene { valid = true, isLoaded = true };
        public struct Scene { public bool valid, isLoaded; public bool IsValid() { return valid; } }
        public T GetComponentInParent<T>() where T : class { return TestScene.Car as T; }
        public T AddComponent<T>() where T : new() { var value = new T(); TestScene.Car.Bridge = value as UnityLiveLink; return value; }
    }
    public static class Application { public static string dataPath = "/SimulatedEditor/Assets"; public static bool runInBackground; public static int targetFrameRate = 120; }
    public static class Time { public static float timeScale = 1, fixedDeltaTime = .002f; }
    public class AudioListener : Behaviour { public static float volume = .7f; public static bool pause; }
    public static class JsonUtility
    {
        public static T FromJson<T>(string json) { return new JavaScriptSerializer().Deserialize<T>(json); }
        public static string ToJson(object value, bool pretty = false) { return new JavaScriptSerializer().Serialize(value); }
    }
}
namespace UnityEditor
{
    [AttributeUsage(AttributeTargets.Class)] public class InitializeOnLoadAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Method)] public class MenuItem : Attribute { public MenuItem(string name) { } }
    public enum PlayModeStateChange { EnteredEditMode, EnteredPlayMode, ExitingPlayMode }
    public static class EditorApplication
    {
        public static event Action update;
        public static event Action<PlayModeStateChange> playModeStateChanged;
        public static double timeSinceStartup = 1;
        public static bool isPlaying, isCompiling, isUpdating, isPaused, pendingPlay;
        public static int Entries;
        public static bool isPlayingOrWillChangePlaymode => isPlaying || pendingPlay;
        public static event Action quitting;
        public static void EnterPlaymode() { Entries++; pendingPlay = true; }
        public static void Tick() { timeSinceStartup += .01; update?.Invoke(); }
        public static void FinishEnter() { pendingPlay = false; isPlaying = true; playModeStateChanged?.Invoke(PlayModeStateChange.EnteredPlayMode); }
        public static void BeginExit() { playModeStateChanged?.Invoke(PlayModeStateChange.ExitingPlayMode); }
        public static void Stop() { isPlaying = false; playModeStateChanged?.Invoke(PlayModeStateChange.EnteredEditMode); }
    }
    public static class AssemblyReloadEvents { public static event Action beforeAssemblyReload; public static void Reload() { beforeAssemblyReload?.Invoke(); } }
    public static class EditorUtility { public static UnityEngine.Object InstanceIDToObject(int id) { return UnityEngine.Object.Instances.FirstOrDefault(o => o.GetInstanceID() == id); } }
    public static class EditorGUIUtility { public static string systemCopyBuffer; }
    public static class Selection { public static UnityEngine.GameObject activeGameObject; }
    public static class EditorPrefs
    {
        static readonly Dictionary<string,string> values = new Dictionary<string,string>();
        public static string GetString(string key, string fallback) { return values.ContainsKey(key) ? values[key] : fallback; }
        public static void SetString(string key, string value) { values[key] = value; }
    }
    public static class SessionState
    {
        static readonly Dictionary<string,string> values = new Dictionary<string,string>();
        public static string GetString(string key, string fallback) { return values.ContainsKey(key) ? values[key] : fallback; }
        public static void SetString(string key, string value) { values[key] = value; }
        public static bool GetBool(string key, bool fallback) { return GetString(key,fallback.ToString()) == "True"; }
        public static void SetBool(string key, bool value) { SetString(key,value.ToString()); }
        public static float GetFloat(string key, float fallback) { return float.Parse(GetString(key,fallback.ToString())); }
        public static void SetFloat(string key, float value) { SetString(key,value.ToString()); }
    }
    public struct GlobalObjectId
    {
        public static GlobalObjectId GetGlobalObjectIdSlow(object value) { return new GlobalObjectId(); }
        public static bool TryParse(string value, out GlobalObjectId id) { id = new GlobalObjectId(); return true; }
        public static int Lookups;
        public static UnityEngine.Object GlobalObjectIdentifierToObjectSlow(GlobalObjectId id) { Lookups++; return TestScene.Car; }
        public override string ToString() { return "car"; }
    }
}
namespace Perrinn424.AutopilotSystem
{
    public class RecordedLap { public List<int> samples = new List<int> {1,2}; public int Count => samples.Count; public float frequency = 50; }
    public class Autopilot
    {
        public bool isActiveAndEnabled = true, IsOn, AllowStart = true;
        public int Toggles;
        public RecordedLap recordedLap = new RecordedLap();
        public void ToggleStatus() { Toggles++; if (AllowStart) IsOn = !IsOn; }
    }
}
namespace VehiclePhysics.InputManagement
{
    public class InputManager { public static InputManager instance = new InputManager(); public Runtime runtime = new Runtime(); public class Runtime { public bool disableForceFeedback; } }
}
public class Perrinn424CarController : UnityEngine.Object
{
    public UnityEngine.GameObject gameObject = new UnityEngine.GameObject();
    public Perrinn424.AutopilotSystem.Autopilot Pilot = new Perrinn424.AutopilotSystem.Autopilot();
    public UnityLiveLink Bridge;
    public bool paused = true;
    public Telemetry telemetry = new Telemetry();
    public class Telemetry { public Row latest = new Row(); }
    public class Row { public int frame = 1; }
    public T GetComponentInChildren<T>(bool inactive) where T : class { return Pilot as T; }
    public T GetComponent<T>() where T : class { return Bridge as T; }
}
namespace Perrinn424.LiveLink
{
    public class UnityLiveLink : UnityEngine.Behaviour { public bool showConnectionPanel = true; public string RecordingPath; public string pairingToken = new string('e',32); public class LapChange { public string parameter; public string mode; public float amount; } public LapChange[] Plan; public void ConfigureSession(LapChange[] plan) { Plan = plan; } }
    public class LocalWebSocketServer : IDisposable
    {
        public struct Incoming { public int client; public string json; }
        public static LocalWebSocketServer Current;
        public int ClientId;
        readonly Queue<Incoming> queue = new Queue<Incoming>();
        public readonly List<string> Replies = new List<string>();
        public LocalWebSocketServer(int port, IEnumerable<string> origins) { Current = this; }
        public bool TryRead(out Incoming value) { if(queue.Count>0) {value=queue.Dequeue();return true;}value=new Incoming();return false; }
        public void Reply(int client,string json) { Replies.Add(json); }
        public void Disconnect(int client) { ClientId = 0; }
        public void Dispose() { ClientId = 0; }
        public void Inject(int client, string json) { ClientId=client;queue.Enqueue(new Incoming {client=client,json=json}); }
    }
}
namespace UnityEngine.Rendering { public static class OnDemandRendering { public static int renderFrameInterval = 3; } }
namespace VehiclePhysics
{
    public class VPTelemetryDisplay : UnityEngine.Behaviour { }
    public class VPTelemetryProvider : UnityEngine.Behaviour { }
    public class VPKinematicWheelCollider : UnityEngine.Behaviour { }
}
namespace Perrinn424.UI { public class TelemetryUIIntegration : UnityEngine.Behaviour { } }
namespace Cinemachine { public class CinemachineBrain : UnityEngine.Behaviour { } }
class UnreviewedComponent : UnityEngine.Behaviour { }
static class TestScene { public static Perrinn424CarController Car = new Perrinn424CarController(); }
class EditorControlHarness
{
    static void Check(bool value,string message) { if(!value)throw new Exception(message); }
    static void Send(string json) { LocalWebSocketServer.Current.Inject(1,json);UnityEditor.EditorApplication.Tick(); }
    static string Last => LocalWebSocketServer.Current.Replies.Last();
    static void Pair() { Send("{\"type\":\"editorHello\",\"token\":\""+new string('a',32)+"\"}");Check(Last.Contains("editorReady"),"pairing"); }
    static void Main()
    {
        var camera = new UnityEngine.Camera(); var disabledCamera = new UnityEngine.Camera { enabled = false };
        var canvas = new UnityEngine.Canvas(); var audio = new UnityEngine.AudioSource();
        var display = new VehiclePhysics.VPTelemetryDisplay(); var ui = new Perrinn424.UI.TelemetryUIIntegration();
        var brain = new Cinemachine.CinemachineBrain(); var provider = new VehiclePhysics.VPTelemetryProvider();
        var wheel = new VehiclePhysics.VPKinematicWheelCollider(); var unreviewed = new UnreviewedComponent();
        var assetCamera = new UnityEngine.Camera(); assetCamera.gameObject.scene = new UnityEngine.GameObject.Scene { valid = false, isLoaded = false };
        var inactiveCamera = new UnityEngine.Camera(); inactiveCamera.gameObject.activeInHierarchy = false;
        UnityEditor.EditorPrefs.SetString("Perrinn424.Telemetry.Editor."+UnityEngine.Application.dataPath,new string('a',32));
        RuntimeHelpers.RunClassConstructor(typeof(TelemetryEditorControl).TypeHandle);UnityEditor.EditorApplication.Tick();
        Send("{\"type\":\"editorProbe\"}");Check(Last.Contains("editorAvailable") && !Last.Contains(new string('a',32)),"presence probe exposes availability without credentials");
        Send("{\"type\":\"startSimulation\",\"requestId\":\"one\"}");Check(UnityEditor.EditorApplication.Entries==0,"unpaired client must not start Unity");
        Pair();Send("{\"type\":\"startSimulation\",\"requestId\":\"one\"}");Check(UnityEditor.EditorApplication.Entries==1,"start enters Play Mode once");
        UnityEditor.AssemblyReloadEvents.Reload();UnityEditor.EditorApplication.Tick();Pair();
        Send("{\"type\":\"startSimulation\",\"requestId\":\"one\"}");Check(UnityEditor.EditorApplication.Entries==1,"domain reload must not replay startup");
        TestScene.Car.telemetry.latest.frame=0;
        UnityEditor.EditorApplication.FinishEnter();UnityEditor.EditorApplication.Tick();
        Check(UnityEngine.Application.runInBackground && TestScene.Car.Bridge==null,"background Editor can initialize its first dynamics row without browser focus switching");
        Check(UnityEngine.AudioListener.volume==0 && UnityEngine.Time.timeScale==10,"mute and accelerate before the first dynamics row");
        Check(!camera.enabled && !canvas.enabled && !audio.enabled && !display.enabled && !ui.enabled && !brain.enabled,"disable audited presentation before the first dynamics row");
        Check(provider.enabled && wheel.enabled && unreviewed.enabled && assetCamera.enabled && inactiveCamera.enabled,"preserve telemetry providers, physics, unknown scripts, prefab assets and inactive objects");
        Check(TestScene.Car.gameObject.activeInHierarchy && UnityEngine.Time.fixedDeltaTime == .002f,"preserve scene objects and physics timestep");
        Check(Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobWorkerCount==1,"apply measured default physics worker count");
        Check(UnityEngine.QualitySettings.vSyncCount==0 && UnityEngine.Application.targetFrameRate==-1 && UnityEngine.Rendering.OnDemandRendering.renderFrameInterval==8 && UnityEngine.AudioListener.pause,"remove presentation pacing and suspend audio");
        System.IO.Directory.CreateDirectory("/tmp/perrinn-live-link/audit-test");
        TelemetrySimulationPerformance.WriteReport("/tmp/perrinn-live-link/audit-test");
        var audit=System.IO.File.ReadAllText("/tmp/perrinn-live-link/audit-test/performance.json");
        Check(audit.Contains("telemetry-fast-v2") && audit.Contains("VPTelemetryDisplay") && audit.Contains("retain"),"write reproducible disabled/retained component audit beside recording");
        TestScene.Car.telemetry.latest.frame=1;UnityEditor.EditorApplication.Tick();
        Check(TestScene.Car.Pilot.IsOn && TestScene.Car.Bridge!=null,"startup must enable Autopilot and telemetry");
        Check(TestScene.Car.Bridge.Plan.Length == 2, "configure both test laps before enabling Autopilot");
        Check(VehiclePhysics.InputManagement.InputManager.instance.runtime.disableForceFeedback,"match Autopilot input behavior");
        Check(Last.Contains("running") && Last.Contains(new string('e',32)),"runtime handoff only after confirmed Autopilot");
        Send("{\"type\":\"startSimulation\",\"requestId\":\"one\"}");Check(TestScene.Car.Pilot.Toggles==1,"idempotent start must not toggle Autopilot off");
        Check(!TestScene.Car.Bridge.showConnectionPanel,"hide runtime pairing overlay for browser-owned runs");
        int lookups = UnityEditor.GlobalObjectId.Lookups;
        for(int i=0;i<10;i++)UnityEditor.EditorApplication.Tick();
        Check(UnityEditor.GlobalObjectId.Lookups==lookups,"cache the selected runtime car instead of resolving its scene identifier every update");
        UnityEngine.AudioListener.volume=1;UnityEngine.Time.timeScale=1;
        camera.enabled=true; UnityEngine.QualitySettings.vSyncCount=1;
        typeof(TelemetrySimulationPerformance).GetField("snapshot",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static).SetValue(null,null);
        UnityEditor.AssemblyReloadEvents.Reload();UnityEditor.EditorApplication.Tick();Pair();
        Check(UnityEngine.AudioListener.volume==0 && UnityEngine.Time.timeScale==10,"keep running simulation muted at 10x across reloads and setting changes");
        Check(!camera.enabled && UnityEngine.QualitySettings.vSyncCount==0,"reapply performance mode from saved snapshot across domain reloads");
        UnityEditor.EditorApplication.BeginExit();
        Check(Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobWorkerCount==1,"retain worker count until Play Mode teardown completes");
        UnityEditor.EditorApplication.Stop();Check(!UnityEngine.Application.runInBackground,"restore previous background setting after Play Mode");Send("{\"type\":\"startSimulation\",\"requestId\":\"one\"}");Check(UnityEditor.EditorApplication.Entries==1,"a stopped operation must not restart on reconnect");
        Check(UnityEngine.AudioListener.volume==.7f && UnityEngine.Time.timeScale==1,"restore original volume and time scale after Play Mode");
        Check(UnityEngine.QualitySettings.vSyncCount==2 && UnityEngine.Application.targetFrameRate==120 && UnityEngine.Rendering.OnDemandRendering.renderFrameInterval==3 && !UnityEngine.AudioListener.pause,"restore original global settings after Play Mode");
        Check(Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobWorkerCount==11,"restore original physics worker count");
        Check(!disabledCamera.enabled,"originally disabled camera must remain disabled");
        Check(TestScene.Car.Bridge.showConnectionPanel,"restore the original runtime overlay setting");
        camera.enabled = true; // Simulate Unity restoring scene state on Play Mode exit.
        TestScene.Car.Pilot.recordedLap=null;Send("{\"type\":\"startSimulation\",\"requestId\":\"two\"}");Check(Last.Contains("valid recorded lap"),"missing lap must fail before Play Mode");
        TestScene.Car.Pilot.recordedLap=new Perrinn424.AutopilotSystem.RecordedLap();TestScene.Car.Pilot.IsOn=false;TestScene.Car.Pilot.AllowStart=false;UnityEditor.EditorApplication.isPlaying=true;
        Send("{\"type\":\"startSimulation\",\"requestId\":\"three\"}");Check(Last.Contains("alignment") && !Last.Contains(new string('e',32)),"refused Autopilot cannot receive a ready handoff");
        Check(camera.enabled && UnityEngine.QualitySettings.vSyncCount==2 && !UnityEngine.AudioListener.pause,"startup failure restores presentation and global settings");
        Check(Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobWorkerCount==11,"startup failure restores worker count");
        Check(UnityEngine.Time.timeScale==1 && UnityEngine.AudioListener.volume==.7f && !UnityEngine.Application.runInBackground,"failed startup restores acceleration, volume and background settings too");
        Console.WriteLine("PASS: automatic presentation allowlist, retained providers/physics/unknowns, scene filtering, audit recording, render pacing, domain reload and failure/exit restoration");
        Console.WriteLine("PASS: real Editor coordinator, authenticated startup, domain reload, one Play transition, Autopilot/telemetry handoff, force feedback, idempotency, stopped operation and invalid/refused Autopilot");
    }
}

namespace Unity.Jobs.LowLevel.Unsafe
{
    public static class JobsUtility { public static int JobWorkerCount = 11; public static int JobWorkerMaximumCount = 11; }
}