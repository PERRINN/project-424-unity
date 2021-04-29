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
            if (Input.GetKeyDown(nextTimeScale) && Input.GetKey(KeyCode.LeftShift))
            {
                timeScaleController.NextTimeScale();
            }
            else if (Input.GetKeyDown(previousTimeScale) && Input.GetKey(KeyCode.LeftShift))
            {
                timeScaleController.PreviousTimeScale();
            }
        }

        private void Reset()
        {
            timeScaleController = this.GetComponent<TimeScaleController>();
        }
    } 
}
