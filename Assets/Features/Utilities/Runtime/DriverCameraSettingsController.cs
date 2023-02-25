﻿using System;
using UnityEngine;
using VehiclePhysics;

namespace Perrinn424.Utilities
{
    public class DriverCameraSettingsController : MonoBehaviour
    {

        [Header("Height Adjustment")]
        public KeyCode increaseHeight;
        public KeyCode decreaseHeight;

        public Transform firstPersonCameraTarget;

        public float heightStep = 0.025f;
        public float heightMax = 0.05f;

        [Header("FOV Adjustment")]
        public KeyCode increaseFOV;
        public KeyCode decreaseFOV;

        public VPCameraController firstPersonCamera;

        public float fovStep = 5f;
        public float fovMin = 25f;
        public float fovMax = 45f;

        [Header("View Damping")]
        public KeyCode toggleViewDamping = KeyCode.N;
        public VPHeadMotion headMotion;

        public event Action onSettingsChanged;

        public float Height => firstPersonCameraTarget.localPosition.y;
        public float FOV => firstPersonCamera.driverCameraFov;
        public bool Damping => headMotion != null? headMotion.longitudinal.mode != VPHeadMotion.HorizontalMotion.Mode.Disabled : false;

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

            if (Input.GetKeyDown(toggleViewDamping))
            {
                ToggleViewDamping();
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

        private void ToggleViewDamping()
        {
            SetViewDamping(!Damping);
        }

        public void SetViewDamping(bool viewDamping)
        {
            if (headMotion != null)
            {
                headMotion.longitudinal.mode = viewDamping ? VPHeadMotion.HorizontalMotion.Mode.Tilt
                    : VPHeadMotion.HorizontalMotion.Mode.Disabled;

                onSettingsChanged?.Invoke();
            }
        }
    }
}
