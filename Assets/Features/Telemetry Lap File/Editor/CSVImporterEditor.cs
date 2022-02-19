using Perrinn424.AutopilotSystem;
using System;
using System.IO;
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
                try
                {
                    TelemetryLapAsset telemetryLap = FileFormatConverterUtils.CSVToTelemetryLapAsset(path);
                    VPReplayAsset replayAsset = FileFormatConverterUtils.TelemetryLapToReplayAsset(telemetryLap);
                    RecordedLap recordedLap = FileFormatConverterUtils.TelemetryLapToRecordedLap(telemetryLap);

                    string telemetryLapFilePath = $"Assets/Replays/{telemetryLap.name}.asset";
                    AssetDatabase.CreateAsset(telemetryLap, telemetryLapFilePath);

                    string replayFilePath = $"Assets/Replays/{replayAsset.name}_replay.asset";
                    AssetDatabase.CreateAsset(replayAsset, replayFilePath);

                    string recorededLapFilePath = $"Assets/Replays/{recordedLap.name}_autopilot.asset";
                    AssetDatabase.CreateAsset(recordedLap, recorededLapFilePath);

                    if (EditorUtility.DisplayDialog("CSV Importer", $"CSV correctly created at {telemetryLapFilePath}. Do you want to use the replayassset at the autopilot?", "ok", "cancel"))
                    {
                        AutopilotProvider provider = FindObjectOfType<AutopilotProvider>();
                        provider.replayAsset = replayAsset;
                        Undo.RecordObject(provider, "Autopilot Replay Asset");
                        Selection.activeGameObject = provider.gameObject;
                    }
                    else
                    {
                        Selection.activeObject = telemetryLap;
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
