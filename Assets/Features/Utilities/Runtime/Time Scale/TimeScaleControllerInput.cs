using UnityEngine;
using VersionCompatibility;

namespace Perrinn424.Utilities
{
    [RequireComponent(typeof(TimeScaleController))]
    internal class TimeScaleControllerInput : MonoBehaviour
    {
        [SerializeField]
        private TimeScaleController timeScaleController = default;

        [SerializeField]
        private UnityKey nextTimeScale = UnityKey.UpArrow;

        [SerializeField]
        private UnityKey previousTimeScale = UnityKey.DownArrow;

        private void Update()
        {
            if (GetKeyDownAndShift(nextTimeScale))
            {
                timeScaleController.NextTimeScale();
            }
            else if (GetKeyDownAndShift(previousTimeScale))
            {
                timeScaleController.PreviousTimeScale();
            }
        }

        private void Reset()
        {
            timeScaleController = this.GetComponent<TimeScaleController>();
        }

        private bool GetKeyDownAndShift(UnityKey key)
        {
            return UnityInput.GetKeyDown(key) && UnityInput.shiftKeyPressed;
        }
    }
}
