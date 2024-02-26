using Perrinn424.Utilities;
using UnityEngine;
using UnityEngine.UI;
using EdyCommonTools;

namespace Perrinn424.UI
{
    public class DriverCameraAdjustmentsUI : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("heightText")]
        public Text driverViewText = default;

        public Text fovText = default;

        public Text dampingText = default;

        public DriverCameraSettingsController adjustmentsController;

        private void OnEnable()
        {
            adjustmentsController.onSettingsChanged += OnAdjustmentsChangedEventHandler;
        }

        private void OnDisable()
        {
            adjustmentsController.onSettingsChanged -= OnAdjustmentsChangedEventHandler;
        }

        private void OnAdjustmentsChangedEventHandler()
        {
            UpdateLabels();
        }

        private void Start()
        {
            UpdateLabels();
        }

        private void UpdateLabels()
        {
            driverViewText.text = string.Format("View {0:+0. mm;-0. mm} {1:+0.0°;-0.0°}", adjustmentsController.Height * 1000.0f, -MathUtility.ClampAngle(adjustmentsController.Rotation));
            fovText.text = $"FOV: {adjustmentsController.FOV}°";
            string damping = adjustmentsController.Damping? "ON" : "OFF";
            dampingText.text = $"View Damping: {damping}";
        }
    }
}
