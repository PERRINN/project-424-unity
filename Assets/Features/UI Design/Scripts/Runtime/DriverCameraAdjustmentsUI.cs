using Perrinn424.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Perrinn424.UI
{
    public class DriverCameraAdjustmentsUI : MonoBehaviour
    {
        [SerializeField]
        private Text heightText = default;

        [SerializeField]
        private Text fovText = default;

        [SerializeField]
        private DriverCameraAdjustmentsController adjustmentsController;

        private void OnEnable()
        {
            adjustmentsController.onAdjustmentsChanged += OnAdjustmentsChangedEventHandler;
        }

        private void OnDisable()
        {
            adjustmentsController.onAdjustmentsChanged -= OnAdjustmentsChangedEventHandler;
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
            heightText.text = string.Format("Height {0:+0.000;-0.000}", adjustmentsController.Height);
            fovText.text = $"vFOV: {adjustmentsController.FOV}°";
        }
    } 
}
