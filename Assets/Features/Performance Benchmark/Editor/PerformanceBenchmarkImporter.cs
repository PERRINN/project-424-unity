using Perrinn424.TelemetryLapSystem;
using System;
using System.IO;
using UnityEditor;

namespace Perrinn424
{
    public class PerformanceBenchmarkImporter : EditorWindow
    {
        [MenuItem("Perrinn424/Performance Benchmark CSV Importer", priority = 2)]
        public static void ShowWindow()
        {
            Import();
        }

        private static void Import()
        {
            var myDocumentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            string path = EditorUtility.OpenFilePanel("Performance Benchmark CSV Importer", myDocumentsFolder, "csv");

            if (path.Length != 0)
            {
                using (var s = new StreamReader(path))
                {
                    Table table = Table.FromStream(s, ';', false);

                    for (int rowIndex = 0; rowIndex < table.RowCount; rowIndex++)
                    {
                        float distance = table[rowIndex, "distance (km)"];
                        UnityEngine.Debug.Log(distance);
                    }

                }

            }
        }
    }
}