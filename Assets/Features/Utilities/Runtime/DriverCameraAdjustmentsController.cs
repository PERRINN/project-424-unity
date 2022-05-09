using System;
using UnityEngine;
using VehiclePhysics;

namespace Perrinn424.Utilities
{
    public class DriverCameraAdjustmentsController : MonoBehaviour
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


        public event Action onAdjustmentsChanged;

        public float Height => firstPersonCameraTarget.localPosition.y;
        public float FOV => firstPersonCamera.driverCameraFov;

        private void Update()
        {
            if (Input.GetKeyDown(increaseHeight))
            {
                ChangeDriverHeight(heightStep);
            }
            else if (Input.GetKeyDown(decreaseHeight))
            {
                ChangeDriverHeight(-heightStep);
            }

            if (Input.GetKeyDown(increaseFOV))
            {
                ChangeCameraFov(fovStep);
            }
            else if (Input.GetKeyDown(decreaseFOV))
            {
                ChangeCameraFov(-fovStep);
            }
        }

        private void ChangeDriverHeight(float delta)
        {
            Vector3 pos = firstPersonCameraTarget.localPosition;
            pos.y = Mathf.Clamp(pos.y + delta, -heightMax, heightMax);
            firstPersonCameraTarget.localPosition = pos;
            
            onAdjustmentsChanged?.Invoke();
        }

        private void ChangeCameraFov(float delta)
        {
            firstPersonCamera.driverCameraFov = Mathf.Clamp(firstPersonCamera.driverCameraFov + delta, fovMin, fovMax);
            
            onAdjustmentsChanged?.Invoke();
        }
    } 
}
