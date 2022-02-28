using Perrinn424.AutopilotSystem;
using System;
using UnityEditor;
using UnityEditor.PackageManager;
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
                        provider.replayAsset = asset;
                        Selection.activeGameObject = provider.gameObject;
                        Scene scene = provider.gameObject.scene;
                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene);
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
