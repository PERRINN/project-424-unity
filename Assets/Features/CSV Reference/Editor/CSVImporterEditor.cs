using UnityEditor;
using UnityEngine;
using VehiclePhysics;

public class CSVImporterEditor : EditorWindow
{
    [MenuItem("Perrinn424/CSV Importer")]
    public static void ShowWindow()
    {
        //EditorWindow.GetWindow(typeof(CSVImporterEditor));
        Import();
    }

    void OnGUI()
    {
        if (GUILayout.Button("Import"))
        {
            Import();
        }
    }

    private static void Import()
    {
        string path = EditorUtility.OpenFilePanel("CSV Importer", "./Telemetry", "metadata");
        if (path.Length != 0)
        {
            VPReplayAsset asset = TelemetryToReplay.Parse(path);
            string filePath = $"Assets/Features/CSV Reference/Replays/{asset.name}.asset";
            AssetDatabase.CreateAsset(asset, filePath);
        }
    }
}
