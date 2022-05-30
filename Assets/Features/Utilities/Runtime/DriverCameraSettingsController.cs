using System;
using UnityEngine;
using VehiclePhysics;

namespace Perrinn424.Utilities
{
    public class DriverCameraSettingsController : MonoBehaviour
    {

        [Header("Height Adjustment")]
        [SerializeField]
        private KeyCode increaseHeight;
        [SerializeField]
        private KeyCode decreaseHeight;

        [SerializeField]
        private Transform firstPersonCameraTarget;

        public float heightStep = 0.025f;
        public float heightMax = 0.05f;

        [Header("FOV Adjustment")]
        [SerializeField]
        private KeyCode increaseFOV;
        [SerializeField]
        private KeyCode decreaseFOV;

        [SerializeField]
        private VPCameraController firstPersonCamera;

        public float fovStep = 5f;
        public float fovMin = 25f;
        public float fovMax = 45f;


        public event Action onSettingsChanged;

        public float Height => firstPersonCameraTarget.localPosition.y;
        public float FOV => firstPersonCamera.driverCameraFov;

        private void Update()
        {
            if (Input.GetKeyDown(increaseHeight))
            {
                SetDriverHeightDelta(heightStep);
            }
            else if (Input.GetKeyDown(decreaseHeight))
            {
                SetDriverHeightDelta(-heightStep);
            }

            if (Input.GetKeyDown(increaseFOV))
            {
                SetCameraFovDelta(fovStep);
            }
            else if (Input.GetKeyDown(decreaseFOV))
            {
                SetCameraFovDelta(-fovStep);
            }
        }

        private void SetDriverHeightDelta(float delta)
        {
            Vector3 pos = firstPersonCameraTarget.localPosition;
            SetDriverHeight(pos.y + delta);
        }

        public void SetDriverHeight(float height)
        {
            Vector3 pos = firstPersonCameraTarget.localPosition;
            pos.y = Mathf.Clamp(height, -heightMax, heightMax);
            firstPersonCameraTarget.localPosition = pos;

            onSettingsChanged?.Invoke();
        }

        private void SetCameraFovDelta(float delta)
        {
            SetCameraFov(firstPersonCamera.driverCameraFov + delta);
        }

        public void SetCameraFov(float fov)
        {
            firstPersonCamera.driverCameraFov = Mathf.Clamp(fov, fovMin, fovMax);

            onSettingsChanged?.Invoke();
        }
    } 
}
