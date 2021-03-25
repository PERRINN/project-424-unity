using Perrinn424.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Perrinn424.UI
{
    public class TimeScaleUI : MonoBehaviour
    {
        [SerializeField]
        private Text text = default;

        [SerializeField]
        private TimeScaleController timeScaleController;

        private void OnEnable()
        {
            timeScaleController.onTimeScaleChanged += OnTimeScaleChangedEventHandler;
        }

        private void OnDisable()
        {
            timeScaleController.onTimeScaleChanged -= OnTimeScaleChangedEventHandler;
        }

        private void OnTimeScaleChangedEventHandler(float timeScale)
        {
            UpdateLabel();
        }

        private void Start()
        {
            UpdateLabel();
        }

        private void UpdateLabel()
        {
            string label = TimeScaleToLabel(timeScaleController.TimeScale);
            text.text = label;
        }

        private string TimeScaleToLabel(float timeScale)
        {
            if (timeScale == 1.0f)
                return "Real time";
            if (timeScale < 1.0f)
                return $"x{timeScale:F1}";

            return $"x{timeScale:F0}";
        }
    } 
}
