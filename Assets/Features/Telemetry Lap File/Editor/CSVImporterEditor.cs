using Perrinn424.AutopilotSystem;
using System;
using System.IO;
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

                    TelemetryLapAsset lapAsset = Create(path);
                    AssetDatabase.CreateAsset(lapAsset, "Assets/lap.asset");
                    if (EditorUtility.DisplayDialog("CSV Importer", $"Replay Asset correctly created at {filePath}. Do you want to use it at the autopilot?", "ok", "cancel"))
                    {
                        AutopilotProvider provider = FindObjectOfType<AutopilotProvider>();
                        provider.replayAsset = asset;
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
        private static TelemetryLapAsset Create(string filePath)
        {
            TelemetryLapMetadata metadata = JsonUtility.FromJson<TelemetryLapMetadata>(File.ReadAllText(filePath));

            string directoryPath = Path.GetDirectoryName(filePath);
            string telemetryPath = Path.Combine(directoryPath, metadata.csvFile);

            TelemetryLapAsset asset = ScriptableObject.CreateInstance<TelemetryLapAsset>();
            asset.metadata = metadata;

            using (var s = new StreamReader(telemetryPath))
            {
                asset.table = Table.FromStream(s);
            }

            return asset;

        }
    } 
}
