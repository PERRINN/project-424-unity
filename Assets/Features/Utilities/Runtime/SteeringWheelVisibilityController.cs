using Perrinn424.UI;
using UnityEngine;

namespace Perrinn424.Utilities
{
    public class SteeringWheelVisibilityController : MonoBehaviour
    {
        public KeyCode nextOptionKey;
        public Dashboard dashboard;
        public GameObject steeringWheel;
        public CircularIndex options;

        private void OnEnable()
        {
            options = new CircularIndex(0, 3);
            SteeringWheelVisibility();
        }

        private void Update()
        {
            if (Input.GetKeyDown(nextOptionKey))
            {
                options++;
                SteeringWheelVisibility();
            }
        }

        private void SteeringWheelVisibility()
        {
            switch (options)
            {
                case 0:
                    SteeringWheelVisibility(true, false);
                    break;
                case 1:
                    SteeringWheelVisibility(false, true);
                    break;
                case 2:
                    SteeringWheelVisibility(false, false);
                    break;
            }

        }

        private void SteeringWheelVisibility(bool showSteeringWheel, bool showDashboard)
        {
            steeringWheel.SetActive(showSteeringWheel);
            dashboard.gameObject.SetActive(showDashboard);
        }
    }
}