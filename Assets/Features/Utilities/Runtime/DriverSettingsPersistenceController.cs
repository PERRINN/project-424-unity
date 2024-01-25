using System;
using System.IO;
using UnityEngine;

namespace Perrinn424.Utilities
{
    public class DriverSettingsPersistenceController : MonoBehaviour
    {
        public DriverCameraSettingsController driverCameraSettingsController;
        public SteeringWheelVisibilityController steeringWheelVisibilityController;

        private readonly string filename = "DriverSettings.json";

        [Header("Restore Settings")]
        public bool height = true;
        public bool rotation = true;
        public bool fov = true;
        public bool viewDamping = true;
        public bool steeringWheelVisibility = true;
        public bool miniDashboardPosition = true;


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
                rotation = driverCameraSettingsController.Rotation,
                fov = driverCameraSettingsController.FOV,
                damping = driverCameraSettingsController.Damping,
                steeringWheelVisibility = steeringWheelVisibilityController.VisibilityOption,
                miniDashboardPosition = driverCameraSettingsController.MiniDashboardPosition
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

                if (height) driverCameraSettingsController.SetCameraHeight(settings.height);
                if (rotation) driverCameraSettingsController.SetCameraRotation(settings.rotation);
                if (fov) driverCameraSettingsController.SetCameraFov(settings.fov);
                if (viewDamping) driverCameraSettingsController.SetViewDamping(settings.damping);
                if (steeringWheelVisibility) steeringWheelVisibilityController.SetVisbilityOption(settings.steeringWheelVisibility);
                if (miniDashboardPosition) driverCameraSettingsController.SetMiniDashboardPosition(settings.miniDashboardPosition);
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
