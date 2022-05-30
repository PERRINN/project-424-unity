using System;
using System.IO;
using UnityEngine;

namespace Perrinn424.Utilities
{
    public class DriverSettingsPersistenceController : MonoBehaviour
    {
        [SerializeField]
        private DriverCameraAdjustmentsController cameraAdjustments;

        private readonly string filename = "driverSettings.json";

        private string GetFullPath()
        {
            return Path.Combine(Application.persistentDataPath, filename);
        }

        private void OnEnable()
        {
            cameraAdjustments.onAdjustmentsChanged += AdjustmentsChangedHandler;
        }

        private void OnDisable()
        {
            cameraAdjustments.onAdjustmentsChanged -= AdjustmentsChangedHandler;
        }

        private void Start()
        {
            Load();
        }

        private void AdjustmentsChangedHandler()
        {
            Save();
        }


        [ContextMenu("Save")]
        public void Save()
        {
            DriverSettings settings = new DriverSettings()
            {
                height = cameraAdjustments.Height,
                fov = cameraAdjustments.FOV
            };

            WriteToFile(settings);
        }

        [ContextMenu("Load")]
        public void Load()
        {
            string path = GetFullPath();

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                DriverSettings settings = JsonUtility.FromJson<DriverSettings>(json);

                cameraAdjustments.SetCameraFov(settings.fov);
                cameraAdjustments.SetDriverHeight(settings.height);
            }
        }

        private void WriteToFile(DriverSettings settings)
        {
            string json = JsonUtility.ToJson(settings, true);
            string path = GetFullPath();
            try
            {
                File.WriteAllText(path, json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Error writing driver settings to file: {e.Message}\nPath: [{path}]");
            }
        }
    }
}
