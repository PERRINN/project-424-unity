using Perrinn424.AutopilotSystem;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
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

                    if (EditorUtility.DisplayDialog("CSV Importer", $"CSV correctly created at {telemetryLapFilePath}. Do you want to use the replay assset at the autopilot?", "ok", "cancel"))
                    {
                        Autopilot autopilot = FindObjectInSceneEvenIfIsDisabled<Autopilot>();
                        autopilot.recordedLap = recordedLap;
                        PrefabUtility.RecordPrefabInstancePropertyModifications(autopilot);
                        AutopilotProvider provider = FindObjectInSceneEvenIfIsDisabled<AutopilotProvider>();
                        provider.replayAsset = replayAsset;

                        Selection.activeGameObject = autopilot.gameObject;
                        Scene scene = autopilot.gameObject.scene;
                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene);
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

        private static T FindObjectInSceneEvenIfIsDisabled<T>() where T : UnityEngine.Object
        {
            T[] objects = Resources.FindObjectsOfTypeAll<T>();

            return objects.First(o => !EditorUtility.IsPersistent(o)); //first object, enabled or disabled that it's on the scene
        }


        [MenuItem("Assets/Create Autopilot file from Replay")]
        static void CreateAutopilotFileFromReplay()
        {
            RecordedLap recordedLap = FileFormatConverterUtils.ReplayAssetToRecordedLap(Selection.activeObject as VPReplayAsset);
            string recorededLapFilePath = $"Assets/Replays/{recordedLap.name}_autopilot.asset";
            AssetDatabase.CreateAsset(recordedLap, recorededLapFilePath);
        }

        [MenuItem("Assets/Create Autopilot file from Replay", true)]
        static bool ValidateCreateAutopilotFileFromReplay()
        {
            // Return false if no transform is selected.
            return Selection.activeObject is VPReplayAsset;
        }

    } 
}
