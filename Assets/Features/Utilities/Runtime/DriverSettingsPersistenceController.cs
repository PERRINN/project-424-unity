using System;
using System.IO;
using UnityEngine;

namespace Perrinn424.Utilities
{
    public class DriverSettingsPersistenceController : MonoBehaviour
    {
        [SerializeField]
        private DriverCameraSettingsController driverCameraSettingsController;

        [SerializeField]
        private SteeringWheelVisibilityController steeringWheelVisibilityController;

        private readonly string filename = "driverSettings.json";

        private string GetFullPath()
        {
            return Path.Combine(Application.persistentDataPath, filename);
        }

        private void OnEnable()
        {
            driverCameraSettingsController.onSettingsChanged += SaveSettings;
            steeringWheelVisibilityController.onVisibilityChanged += SaveSettings;
        }

        private void OnDisable()
        {
            driverCameraSettingsController.onSettingsChanged -= SaveSettings;
            steeringWheelVisibilityController.onVisibilityChanged -= SaveSettings;
        }

        private void Start()
        {
            Load();
        }


        [ContextMenu("Save")]
        public void SaveSettings()
        {
            DriverSettings settings = new DriverSettings()
            {
                height = driverCameraSettingsController.Height,
                fov = driverCameraSettingsController.FOV,
                steeringWheelVisibility = steeringWheelVisibilityController.VisibilityOption
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

                driverCameraSettingsController.SetCameraFov(settings.fov);
                driverCameraSettingsController.SetDriverHeight(settings.height);
                steeringWheelVisibilityController.SetVisbilityOption(settings.steeringWheelVisibility);
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
