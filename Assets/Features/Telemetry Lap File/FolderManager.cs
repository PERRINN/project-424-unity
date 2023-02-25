using System;
using System.IO;
using UnityEngine;

namespace Perrinn424.TelemetryLapSystem
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public static class FolderManager
    {
        public static string RootFolder { get; private set; }

        static FolderManager()
        {
            Init();
        }

        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            RootFolder = GetRoot();
            CreateRootFolderIfnotExists();
        }

        private static string GetRoot()
        {
            var myDocumentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var telemetryFolder = Path.Combine(myDocumentsFolder, "PERRINN 424", "Lap Data");
            return telemetryFolder;
        }

        private static void CreateRootFolderIfnotExists()
        {
            if (!Directory.Exists(RootFolder))
            {
                Directory.CreateDirectory(RootFolder);
            }
        }

        public static string Combine(string path)
        {
            return Path.Combine(RootFolder, path);
        }

        public static void Rename(string oldPath, string newPath)
        {
            File.Move(oldPath, newPath); 
        }

        public static void Delete(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public static string GetFullPath(string filename)
        {
            if (!filename.Contains(RootFolder))
            {
                filename = Combine(filename);
            }

            return filename;
        }
    } 
}
