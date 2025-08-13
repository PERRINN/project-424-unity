using System;
using UnityEngine;
using VersionCompatibility;

namespace Perrinn424.Utilities
{
    public class SteeringWheelVisibilityController : MonoBehaviour
    {
        public UnityKey nextOptionKey;
        public GameObject steeringWheel;
        private CircularIndex visibilityOption;

        public int VisibilityOption => visibilityOption;

        public event Action onVisibilityChanged;


        private void OnEnable()
        {
            visibilityOption = new CircularIndex(0, 2);
            RefreshVisibility();
        }

        private void Update()
        {
            if (UnityInput.GetKeyDown(nextOptionKey))
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
                    SteeringWheelVisibility(true);
                    break;
                case 1:
                    SteeringWheelVisibility(false);
                    break;
            }

        }

        private void SteeringWheelVisibility(bool showSteeringWheel)
        {
            steeringWheel.SetActive(showSteeringWheel);
        }
    }
}