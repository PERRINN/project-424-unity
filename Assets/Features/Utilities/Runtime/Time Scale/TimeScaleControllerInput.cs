using UnityEngine;

namespace Perrinn424.Utilities
{
    [RequireComponent(typeof(TimeScaleController))]
    internal class TimeScaleControllerInput : MonoBehaviour
    {
        [SerializeField]
        private TimeScaleController timeScaleController = default;

        [SerializeField]
        private KeyCode nextTimeScale = KeyCode.UpArrow;

        [SerializeField]
        private KeyCode previousTimeScale = KeyCode.DownArrow;

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

        private bool GetKeyDownAndShift(KeyCode key)
        {
            return Input.GetKeyDown(key) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
        }
    } 
}
