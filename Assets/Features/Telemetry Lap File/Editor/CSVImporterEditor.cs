using UnityEditor;
using UnityEngine;
using VehiclePhysics;

namespace Perrinn424.TelemetryLapSystem.Editor
{
    public class CSVImporterEditor : EditorWindow
    {
        [MenuItem("Perrinn424/CSV Importer")]
        public static void ShowWindow()
        {
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
                VPReplayAsset asset = TelemetryLapToReplayAsset.Create(path);
                string filePath = $"Assets/Features/CSV Reference/Replays/{asset.name}.asset";
                AssetDatabase.CreateAsset(asset, filePath);
            }
        }
    } 
}
