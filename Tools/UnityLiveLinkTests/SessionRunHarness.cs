// Execute the real runtime bridge against deterministic timing and suspension APIs.
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Perrinn424.LiveLink;
namespace UnityEngine
{
    [AttributeUsage(AttributeTargets.Class)] public class AddComponentMenu:Attribute {public AddComponentMenu(string s){}}
    [AttributeUsage(AttributeTargets.Class)] public class DisallowMultipleComponent:Attribute {}
    [AttributeUsage(AttributeTargets.Class)] public class RequireComponent:Attribute {public RequireComponent(Type t){}}
    public class MonoBehaviour { public bool enabled=true; public Transform transform=new Transform(); public T GetComponent<T>() where T:class {return Scene.Car as T;} public static T FindObjectOfType<T>() where T:class{return Scene.Timer as T;} }
    public class Transform {public Vector3 position;}
    public struct Vector3 {public float x,y,z;}
    public class Rigidbody {public Vector3 position;}
    public static class Application {public static bool runInBackground,isPlaying=true,QuitCalled;public static string persistentDataPath="/tmp"; public static void Quit(){if(!Perrinn424.LiveLink.LocalWebSocketServer.Closed)throw new Exception("exit requested before network workers shut down");QuitCalled=true;}}
    public static class Time {public static double realtimeSinceStartupAsDouble;public static float timeScale=1;}
    public static class Debug {public static void LogError(string s,object o){throw new Exception(s);}}
    public static class JsonUtility {public static string ToJson(object o){return new JavaScriptSerializer().Serialize(o);}public static T FromJson<T>(string s){return new JavaScriptSerializer().Deserialize<T>(s);}}
    public struct Rect {public Rect(float a,float b,float c,float d){}}
    public static class Screen {public static int width=1000;}
    public static class Mathf {public static float Max(float a,float b){return Math.Max(a,b);}}
    public static class GUI {public static Skin skin=new Skin();public class Skin {public object box;}}
    public static class GUILayout {public static void BeginArea(Rect r,object o){}public static void Label(string s){}public static bool Button(string s){return false;}public static void EndArea(){}}
    public static class GUIUtility {public static string systemCopyBuffer;}
}
namespace VehiclePhysics
{
    public enum Channel {Custom}
    public class WheelColliderBehaviour {public object api;public virtual void RefreshPhysics(){}}
    public class KinematicWheel {public class SuspensionDamper {public float springRate;}}
    public class KinematicWheelCollider {public KinematicWheel.SuspensionDamper suspensionDamper;}
    public class StandardWheelCollider {public float springRate;}
    public class VPKinematicWheelCollider:WheelColliderBehaviour {public KinematicWheel.SuspensionDamper suspensionDamper=new KinematicWheel.SuspensionDamper {springRate=230000};public override void RefreshPhysics(){api=new KinematicWheelCollider {suspensionDamper=suspensionDamper};}}
    public class VPStandardWheelCollider:WheelColliderBehaviour {public float springRate;public override void RefreshPhysics(){api=new StandardWheelCollider {springRate=springRate};}}
}
namespace VehiclePhysics.Timing
{
    public class LapTimer {public Action onBeginLap;public Action<int,float> onSector;public Action<float,bool,float[],bool[]> onLap;public List<bool> currentValidSectors=new List<bool>{true,true,true};}
}
namespace Perrinn424.AutopilotSystem {public class Autopilot {public float positionOffset=.45f;}}
public static class Perrinn424Data {public const int FrontMguBase=0,RearMguBase=1,WheelsTorque=0;}
public class Perrinn424CarController
{
    public bool paused;public UnityEngine.Transform transform=new UnityEngine.Transform();
    public Action onPreDynamicsStep,onDynamicsStepCompleted;
    public Axle frontAxle=new Axle();public class Axle {public VehiclePhysics.WheelColliderBehaviour leftWheel=new VehiclePhysics.VPKinematicWheelCollider(),rightWheel=new VehiclePhysics.VPKinematicWheelCollider();}
    public Perrinn424.AutopilotSystem.Autopilot pilot=new Perrinn424.AutopilotSystem.Autopilot();
    public T GetComponentInChildren<T>(bool include) where T:class{return pilot as T;}
    public Telemetry telemetry=new Telemetry();public Data data=new Data();public class Data {public int[] Get(VehiclePhysics.Channel c){return new int[2];}}
    public class Telemetry {public Row latest=new Row();public Descriptor[] channels={new Descriptor {group=new Group {instance=new object(),actualFrequency=50}}};public int GetChannelIndex(string id){return id=="AutopilotDistanceError"?0:-1;}public Semantic GetChannelSemmantic(int i){return new Semantic {displayMultiplier=1000,displayUnits=" mm"};}}
    public class Row {public int frame=1;public double totalTime;public float[] values={.0125f};}
    public class Descriptor {public Group group=new Group();}public class Group {public object instance;public float actualFrequency;}
    public class Semantic {public float displayMultiplier;public string displayUnits;}
}
namespace Perrinn424.LiveLink
{
    public class SessionRecording:IDisposable
    {
        public string Error,DirectoryPath="/tmp/session";public int Count,EventCount;public static bool Closed;public static List<string> Changes=new List<string>();
        public SessionRecording(string p,string m,int n){Closed=false;}
        public class Sample {public int frame;public double time,simulationTime;public bool paused;public float[] values;public string ToJson(){return "{}";}}
        public static Sample Last;public void Append(Sample s){Last=s;Count++;}public void AddChange(string s){EventCount++;Changes.Add(s);}public string History(int a,int e,int l){return "{}";}public void Dispose(){Closed=true;}
    }
    public class LocalWebSocketServer:IDisposable
    {
        public string Error;public int ClientId;public static bool Closed;public struct Incoming {public int client;public string json;}
        public LocalWebSocketServer(int p,string[] o){Closed=false;}public bool TryRead(out Incoming i){i=new Incoming();return false;}public void Reply(int c,string s){}public void Publish(int c,string s){}public void Disconnect(int c){}public void Dispose(){Closed=true;}public Task<bool> ReplyWithReceipt(int c,string s){return Task.FromResult(true);}
    }
}
static class Scene {public static Perrinn424CarController Car=new Perrinn424CarController();public static VehiclePhysics.Timing.LapTimer Timer=new VehiclePhysics.Timing.LapTimer();}
class SessionRunHarness
{
    static void Call(UnityLiveLink b,string name){typeof(UnityLiveLink).GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(b,null);}
    static void Check(bool test,string message){if(!test)throw new Exception(message);}
    static float Rate=>((VehiclePhysics.VPKinematicWheelCollider)Scene.Car.frontAxle.leftWheel).suspensionDamper.springRate;
    static UnityLiveLink.LapChange Change(string p,string m,float a)=>new UnityLiveLink.LapChange{parameter=p,mode=m,amount=a};
    static void Main()
    {
        var bridge=new UnityLiveLink();Call(bridge,"OnEnable");
        Call(bridge,"ResolveChannels");
        var catalogue=(UnityLiveLink.ChannelDescription[])typeof(UnityLiveLink).GetField("descriptions",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(bridge);
        Check(catalogue.Length==41 && catalogue[40].id=="AutopilotDistanceError" && catalogue[40].unit=="mm" && catalogue[40].available,"trajectory error uses the existing provider and millimetre semantic");
        Call(bridge,"Capture");Check(Math.Abs(SessionRecording.Last.values[40]-12.5f)<.001,"capture converts trajectory error to mm without treating it as a lap timing field");
        Scene.Car.telemetry.channels[0].group.instance=null;Call(bridge,"Capture");Check(float.IsNaN(SessionRecording.Last.values[40]),"missing error provider stays unavailable");
        Scene.Car.telemetry.channels[0].group.instance=new object();
        Console.WriteLine("PASS: appended trajectory error channel, unchanged timing indices, provider unit conversion, real capture and missing-provider handling");
        try {bridge.ConfigureSession(new[]{Change("frontSpringRate","gain",100),Change("","offset",0)});throw new Exception("out of range plan accepted");}catch(ArgumentException){}
        bridge.ConfigureSession(new[]{Change("frontSpringRate","gain",1.05f),Change("autopilotPositionOffset","offset",.1f)});
        Scene.Timer.onLap(10,false,new float[0],new bool[0]);Check(!Scene.Car.paused,"ignore initial partial lap");
        Scene.Timer.onBeginLap();Check(Rate==230000,"lap 1 retains baseline");
        Scene.Timer.onLap(300,true,new float[0],new bool[0]);Check(!Scene.Car.paused,"keep running after baseline");
        Scene.Timer.onBeginLap();Check(Math.Abs(Rate-241500)<1,"apply gain at lap 2 start");Check(Math.Abs(Scene.Car.pilot.positionOffset-.45f)<.001,"other parameter stays baseline");
        Scene.Timer.onLap(299,true,new float[0],new bool[0]);Check(!Scene.Car.paused,"keep running after lap 2");
        Scene.Timer.onBeginLap();Check(Rate==230000,"restore spring baseline for independent test 3");Check(Math.Abs(Scene.Car.pilot.positionOffset-.55f)<.001,"apply offset at lap 3 start");
        Check(SessionRecording.Changes.Count==3,"record both test changes and the baseline restoration");
        Scene.Timer.onLap(298,true,new float[0],new bool[0]);Check(Scene.Car.paused,"stop physics immediately at lap 3 finish");
        Scene.Timer.onBeginLap();Check((int)typeof(UnityLiveLink).GetField("lapTrigger",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(bridge)==3,"do not create lap 4");
        UnityEngine.Time.realtimeSinceStartupAsDouble=16;Call(bridge,"Update");Check(SessionRecording.Closed,"finalize recording even without a browser");Call(bridge,"Update");Check(UnityEngine.Application.QuitCalled,"exit standalone simulation after completion grace period");
        Call(bridge,"OnDisable");Check(Rate==230000&&Math.Abs(Scene.Car.pilot.positionOffset-.45f)<.001,"restore originals on exit");
        Scene.Car = new Perrinn424CarController(); Scene.Timer = new VehiclePhysics.Timing.LapTimer();
        bridge = new UnityLiveLink(); Call(bridge,"OnEnable");
        bridge.ConfigureSession(new[]{Change("frontSpringRate","offset",10000),Change("frontSpringRate","gain",1.1f)});
        Scene.Timer.onBeginLap(); Scene.Timer.onLap(300,true,new float[0],new bool[0]); Scene.Timer.onBeginLap();
        Check(Rate==240000,"offset uses original spring rate");
        Scene.Timer.onLap(299,true,new float[0],new bool[0]); Scene.Timer.onBeginLap();
        Check(Math.Abs(Rate-253000)<1,"successive tests of the same parameter never compound");
        Call(bridge,"OnDisable");
        Console.WriteLine("PASS: real runtime bridge plan validation, baseline, lap-boundary gain/offset, independent test reset, third-lap stop, no lap 4, disconnected finalization and original setup restoration");
    }
}
