using Perrinn424.AutopilotSystem;
using System;
using UnityEditor;
using UnityEditor.PackageManager;
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
                try
                {
                    VPReplayAsset asset = TelemetryLapToReplayAsset.Create(path);
                    string filePath = $"Assets/Replays/{asset.name}.asset";
                    AssetDatabase.CreateAsset(asset, filePath);
                    if (EditorUtility.DisplayDialog("CSV Importer", $"Replay Asset correctly created at {filePath}. Do you want to use it at the autopilot?", "ok", "cancel"))
                    {
                        AutopilotProvider provider = FindObjectOfType<AutopilotProvider>();
                        provider.replayAssets = new[] { asset };
                        Undo.RecordObject(provider, "Autopilot Replay Asset");
                        Selection.activeGameObject = provider.gameObject;
                    }
                    else
                    {
                        Selection.activeObject = asset;
                    }
                }
                catch (Exception e)
                {
                    string msg = $"Error importing. {e.Message}";
                    EditorUtility.DisplayDialog("CSV Importer", msg, "ok");
                }
            }
        }
    } 
}
