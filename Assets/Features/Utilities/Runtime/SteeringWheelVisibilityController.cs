using Perrinn424.UI;
using System;
using UnityEngine;

namespace Perrinn424.Utilities
{
    public class SteeringWheelVisibilityController : MonoBehaviour
    {
        public KeyCode nextOptionKey;
        public Dashboard dashboard;
        public GameObject steeringWheel;
        private CircularIndex visibilityOption;

        public int VisibilityOption => visibilityOption;

        public event Action onVisibilityChanged;


        private void OnEnable()
        {
            visibilityOption = new CircularIndex(0, 3);
            RefreshVisibility();
        }

        private void Update()
        {
            if (Input.GetKeyDown(nextOptionKey))
            {
                SetVisbilityOption(visibilityOption + 1);
            }
        }

        public void SetVisbilityOption(int newOption)
        {
            visibilityOption.Assign(newOption);
            RefreshVisibility();
            onVisibilityChanged?.Invoke();
        }

        private void RefreshVisibility()
        {
            switch (visibilityOption)
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