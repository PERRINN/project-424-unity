using UnityEditor;
using UnityEngine;
using VehiclePhysics;

public class TelemetryToReplayTest : MonoBehaviour
{
    public string path;

    [ContextMenu("Do it")]
    public void DoIt()
    {
        VPReplayAsset asset = TelemetryToReplay.Parse(path);
        AssetDatabase.CreateAsset(asset, "Assets/Telemetry.asset");
    }
}
