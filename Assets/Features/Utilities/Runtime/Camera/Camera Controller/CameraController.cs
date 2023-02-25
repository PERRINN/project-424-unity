using Cinemachine;
using Perrinn424.Utilities;
using UnityEngine;
using UnityEngine.Assertions;
using VehiclePhysics;
using EdyCommonTools;


namespace Perrinn424.CameraSystem
{
    public class CameraController : MonoBehaviour
    {
        public enum Mode
        {
            Driver,
            SmoothFollow,
            Orbit,
            LookAt,
            Free,
            Tv
        }

        public Transform tvCameraSystem;
        public Camera mainCamera;

        private VPCameraController m_vppController;
        private CinemachineBrain m_cmController;
        private CameraFovController m_fovController;
        private CircularIterator<Mode> m_modeIterator;
        private float m_savedCameraFov;


        private void OnEnable()
        {
            m_modeIterator = new CircularIterator<Mode>(new[] { Mode.Driver, Mode.SmoothFollow, Mode.Orbit, Mode.Tv });
            m_vppController = tvCameraSystem.GetComponent<VPCameraController>();
            m_cmController = tvCameraSystem.GetComponent<CinemachineBrain>();
            m_fovController = mainCamera.GetComponent<CameraFovController>();
            m_savedCameraFov = mainCamera.fieldOfView;
            UpdateMode();
        }

        private void OnDisable()
        {
        }

        public void NextMode()
        {
            if (isActiveAndEnabled)
            {
                m_modeIterator.MoveNext();
                UpdateMode();
            }
        }

        private void UpdateMode()
        {
            switch (m_modeIterator.Current)
            {
                case Mode.Driver:
                case Mode.SmoothFollow:
                case Mode.Orbit:
                case Mode.LookAt:
                case Mode.Free:
                    SetVPCamera((VPCameraController.Mode)m_modeIterator.Current);
                    break;
                case Mode.Tv:
                    SetTVMode();
                    break;
            }
        }


        public void SetVPCamera(VPCameraController.Mode mode)
        {
            // TV mode may have changed the camera FoV. Restore it here.

            m_cmController.enabled = false;
            mainCamera.fieldOfView = m_savedCameraFov;

            m_vppController.enabled = true;
            m_vppController.mode = mode;

            // Also disable the FoV controller if existing, so it can't
            // change the FoV before is disabled by the TV Camera Zoom Controller.

            if (m_fovController)
                m_fovController.enabled = false;
        }

        public void SetTVMode()
        {
            // Save current camera FoV so TV mode may change it.

            m_vppController.enabled = false;
            m_savedCameraFov = mainCamera.fieldOfView;

            m_cmController.enabled = true;
        }
    }
}
