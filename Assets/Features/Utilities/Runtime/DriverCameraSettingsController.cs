﻿using System;
using UnityEngine;
using VehiclePhysics;
using EdyCommonTools;

namespace Perrinn424.Utilities
{
    public class DriverCameraSettingsController : MonoBehaviour
    {

        [Header("Cockpit Camera Adjustment")]
        public Transform firstPersonCameraTarget;
        [Space(5)]
        public KeyCode increaseHeight;
        public KeyCode decreaseHeight;
        public float heightStep = 0.025f;
        public float heightMax = 0.1f;
        public float heightMin = -0.1f;
        [Space(5)]
        public KeyCode increaseRotation;
        public KeyCode decreaseRotation;
        public float rotationStep = 0.2f;
        public float rotationMin = 0.0f;
        public float rotationMax = 25.0f;

        [Header("FOV Adjustment")]
        public VPCameraController firstPersonCamera;
        [Space(5)]
        public KeyCode increaseFOV;
        public KeyCode decreaseFOV;
        public float fovStep = 5f;
        public float fovMin = 25f;
        public float fovMax = 45f;

        [Header("View Damping")]
        public VPHeadMotion headMotion;
        public KeyCode toggleViewDamping = KeyCode.N;

        [Header("Minidashboard Adjustment")]
        public Transform miniDashboard;
        [Space(5)]
        public KeyCode increaseMiniDashboardPosition = KeyCode.Alpha5;
        public KeyCode decreaseMiniDashboardPosition = KeyCode.Alpha4;
        public float miniDashboardHeightStep = 0.0025f;

        public event Action onSettingsChanged;

        public float Height => firstPersonCameraTarget != null? firstPersonCameraTarget.localPosition.y : 0.0f;
        public float Rotation => firstPersonCameraTarget != null? firstPersonCameraTarget.localRotation.eulerAngles.x : 0.0f;
        public float FOV => firstPersonCamera != null? firstPersonCamera.driverCameraFov : 0.0f;
        public bool Damping => headMotion != null? headMotion.longitudinal.mode != VPHeadMotion.HorizontalMotion.Mode.Disabled : false;
        public float MiniDashboardPosition => miniDashboard != null? miniDashboard.localPosition.y : 0.0f;

        private void Update()
        {
            if (Input.GetKeyDown(increaseHeight))
            {
                SetCameraHeightDelta(heightStep);
            }

            if (Input.GetKeyDown(decreaseHeight))
            {
                SetCameraHeightDelta(-heightStep);
            }

            if (Input.GetKeyDown(increaseRotation))
            {
                SetCameraRotationDelta(rotationStep);
            }

            if (Input.GetKeyDown(decreaseRotation))
            {
                SetCameraRotationDelta(-rotationStep);
            }

            if (Input.GetKeyDown(increaseFOV))
            {
                SetCameraFovDelta(fovStep);
            }

            if (Input.GetKeyDown(decreaseFOV))
            {
                SetCameraFovDelta(-fovStep);
            }

            if (Input.GetKeyDown(toggleViewDamping))
            {
                ToggleViewDamping();
            }

            if (Input.GetKeyDown(increaseMiniDashboardPosition))
            {
                SetMiniDashboardPositionDelta(miniDashboardHeightStep);
            }

            if (Input.GetKeyDown(decreaseMiniDashboardPosition))
            {
                SetMiniDashboardPositionDelta(-miniDashboardHeightStep);
            }
        }

        private void SetCameraHeightDelta(float delta)
        {
            if (firstPersonCameraTarget != null)
            {
                Vector3 pos = firstPersonCameraTarget.localPosition;
                SetCameraHeight(pos.y + delta);
            }
        }

        public void SetCameraHeight(float height)
        {
            if (firstPersonCameraTarget != null)
            {
                Vector3 pos = firstPersonCameraTarget.localPosition;
                pos.y = Mathf.Clamp(height, heightMin, heightMax);
                firstPersonCameraTarget.localPosition = pos;

                onSettingsChanged?.Invoke();
            }
        }

        private void SetCameraRotationDelta(float delta)
        {
            if (firstPersonCameraTarget != null)
            {
                Vector3 rot = firstPersonCameraTarget.localRotation.eulerAngles;
                SetCameraRotation(rot.x - delta);
            }
        }

        public void SetCameraRotation(float rotation)
        {
            if (firstPersonCameraTarget != null)
            {
                Vector3 rot = firstPersonCameraTarget.localRotation.eulerAngles;
                rot.x = Mathf.Clamp(MathUtility.ClampAngle(rotation), -rotationMax, -rotationMin);
                firstPersonCameraTarget.localRotation = Quaternion.Euler(rot);

                onSettingsChanged?.Invoke();
            }
        }

        private void SetCameraFovDelta(float delta)
        {
            if (firstPersonCamera != null)
            {
                SetCameraFov(firstPersonCamera.driverCameraFov + delta);
            }
        }

        public void SetCameraFov(float fov)
        {
            if (firstPersonCamera != null)
            {
                firstPersonCamera.driverCameraFov = Mathf.Clamp(fov, fovMin, fovMax);

                onSettingsChanged?.Invoke();
            }
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


        private void SetMiniDashboardPositionDelta(float delta)
        {
            SetMiniDashboardPosition(MiniDashboardPosition + delta);
        }

        public void SetMiniDashboardPosition(float position)
        {
            if (miniDashboard != null)
            {
                Vector3 localPos = miniDashboard.localPosition;
                localPos.y = position;
                miniDashboard.localPosition = localPos;
                onSettingsChanged?.Invoke();
            }
        }
    }
}
