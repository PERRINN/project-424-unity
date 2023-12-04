using Perrinn424.AutopilotSystem;
using Perrinn424.TelemetryLapSystem;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Perrinn424.PerformanceBenchmarkSystem.Editor
{
    public class PerformanceBenchmarkImporter : EditorWindow
    {
        private static string rootFolder = "Assets/Replays/Performance Benchmarks";
        private static string dialogTitle = "Performance Benchmark CSV Importer";

        private float frequency = 10f;

        private string timeHeader = "time(s)";
        private float timeScale = 1.0f;

        private string distanceHeader = "distance (km)";
        private float distanceScale = 996.4f;

        private string throttleHeader = "percentThrottle";
        private float throttleScale = 0.01f;

        private string brakeHeader = "percentBrake";
        private float brakeScale = 0.01f;

        private string speedHeader = "speed";
        private float speedScale = 0.277778f;



        [MenuItem("Perrinn424/Performance Benchmark CSV Importer", priority = 2)]
        public static void ShowWindow()
        {
            PerformanceBenchmarkImporter window = (PerformanceBenchmarkImporter)EditorWindow.GetWindow(typeof(PerformanceBenchmarkImporter));
            window.Show();
        }

        void OnGUI()
        {
            GUILayout.Label("Performance Benchmark Importer Settings", EditorStyles.boldLabel);

            frequency = EditorGUILayout.FloatField("Frequency", frequency);

            EditorGUILayout.Space(10);
            timeHeader = EditorGUILayout.TextField("Time Header", timeHeader);
            timeScale = EditorGUILayout.FloatField("Time Scale", timeScale);

            EditorGUILayout.Space(10);
            distanceHeader = EditorGUILayout.TextField("Time Header", distanceHeader);
            distanceScale = EditorGUILayout.FloatField("Time Scale", distanceScale);

            EditorGUILayout.Space(10);
            throttleHeader = EditorGUILayout.TextField("Time Header", throttleHeader);
            throttleScale = EditorGUILayout.FloatField("Time Scale", throttleScale);

            EditorGUILayout.Space(10);
            brakeHeader = EditorGUILayout.TextField("Time Header", brakeHeader);
            brakeScale = EditorGUILayout.FloatField("Time Scale", brakeScale);

            EditorGUILayout.Space(10);
            speedHeader = EditorGUILayout.TextField("Time Header", speedHeader);
            speedScale = EditorGUILayout.FloatField("Time Scale", speedScale);

            if (GUILayout.Button("Import"))
            {
                Import();
            }
        }


        private void Import()
        {
            var myDocumentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            string path = EditorUtility.OpenFilePanel(dialogTitle, myDocumentsFolder, "csv");

            if (path.Length != 0)
            {
                using (var s = new StreamReader(path))
                {
                    Table table = Table.FromStream(s, ';', false);
                    
                    PerformanceBenchmarkData performanceBenchmarkData = ScriptableObject.CreateInstance<PerformanceBenchmarkData>();
                    performanceBenchmarkData.frenquency = frequency;
                    performanceBenchmarkData.samples = new List<PerformanceBenchmarkSample>(table.RowCount);

                    for (int rowIndex = 0; rowIndex < table.RowCount; rowIndex++)
                    {
                        PerformanceBenchmarkSample newSample = new PerformanceBenchmarkSample()
                        {

                            time = table[rowIndex, timeHeader] * timeScale,
                            distance = table[rowIndex, distanceHeader] * distanceScale,
                            throttle = table[rowIndex, throttleHeader] * throttleScale,
                            brake = table[rowIndex, brakeHeader] * brakeScale,
                            speed = table[rowIndex, speedHeader] * speedScale,
                        };

                        performanceBenchmarkData.samples.Add(newSample);
                    }

                    long timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                    string filename = System.IO.Path.GetFileName(path);
                    string filePath = $"{rootFolder}/{filename}_{timestamp}.asset";
                    AssetDatabase.CreateAsset(performanceBenchmarkData, filePath);

                    EditorUtility.DisplayDialog(dialogTitle, "Done!", "ok");
                    Selection.activeObject = performanceBenchmarkData;
                }

            }
        }
    }
}