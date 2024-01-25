using Perrinn424.Utilities;
using UnityEngine;
using UnityEngine.UI;
using EdyCommonTools;

namespace Perrinn424.UI
{
    public class DriverCameraAdjustmentsUI : MonoBehaviour
    {
        [SerializeField]
        private Text heightText = default;

        [SerializeField]
        private Text fovText = default;

        [SerializeField]
        private Text dampingText = default;

        [SerializeField]
        private DriverCameraSettingsController adjustmentsController;

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
            heightText.text = string.Format("View {0:+0.0°;-0.0°}", -MathUtility.ClampAngle(adjustmentsController.Rotation));
            fovText.text = $"FOV: {adjustmentsController.FOV}°";
            string damping = adjustmentsController.Damping? "ON" : "OFF";
            dampingText.text = $"View Damping: {damping}";
        }
    }
}
