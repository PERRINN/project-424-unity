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
        // This controller expects a CameraTarget component in the target with "Use Custom Cameras" enabled
        // and one custom camera per each entry, except for Tv.

        public enum Mode
        {
            Driver,
            SmoothFollow,
            Orbit,
            OrbitFixed,
            LookAt,
            Free,
            PhysicsFront,
            PhysicsRear,
            Tv
        }

        public Transform tvCameraSystem;
        public Camera mainCamera;
        public GameObject physicsCameraFront;
        public GameObject physicsCameraRear;

        private VPCameraController m_vppController;
        private CinemachineBrain m_cmController;
        private CameraFovController m_fovController;
        private CircularIterator<Mode> m_modeIterator;
        private float m_savedCameraFov;


        private void OnEnable()
        {
            m_modeIterator = new CircularIterator<Mode>(new[] { Mode.Driver, Mode.SmoothFollow, Mode.Orbit, Mode.OrbitFixed, Mode.Tv, Mode.PhysicsFront, Mode.PhysicsRear });
            m_vppController = tvCameraSystem.GetComponent<VPCameraController>();
            m_cmController = tvCameraSystem.GetComponent<CinemachineBrain>();
            m_fovController = mainCamera.GetComponent<CameraFovController>();
            m_savedCameraFov = mainCamera.fieldOfView;
            UpdateMode();
        }

        private void OnDisable()
        {
        }

        public void SetMode(Mode mode)
        {
            if (isActiveAndEnabled && mode != m_modeIterator.Current)
            {
                m_modeIterator.Current = mode;
                UpdateMode();
            }
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
                case Mode.LookAt:
                case Mode.Free:
                    SetPhysicsCamera(false);
                    SetVPCamera((int)m_modeIterator.Current);
                    break;

                case Mode.PhysicsFront:
                    SetPhysicsCamera(true, rear: false);
                    SetVPCamera((int)m_modeIterator.Current);
                    break;

                case Mode.PhysicsRear:
                    SetPhysicsCamera(true, rear: true);
                    SetVPCamera((int)m_modeIterator.Current);
                    break;

                case Mode.Orbit:
                    m_vppController.orbit.targetRelative = false;
                    SetVPCamera((int)m_modeIterator.Current);
                    SetPhysicsCamera(false);
                    break;

                case Mode.OrbitFixed:
                    m_vppController.orbit.targetRelative = true;
                    SetVPCamera((int)m_modeIterator.Current);
                    SetPhysicsCamera(false);
                    break;

                case Mode.Tv:
                    SetPhysicsCamera(false);
                    SetTVMode();
                    break;
            }
        }

        private void SetVPCamera(int cameraIndex)
        {
            // TV mode may have changed the camera FoV. Restore it here.

            m_cmController.enabled = false;
            mainCamera.fieldOfView = m_savedCameraFov;

            m_vppController.enabled = true;
            m_vppController.customCameraIndex = cameraIndex;

            // Also disable the FoV controller if existing, so it can't
            // change the FoV before is disabled by the TV Camera Zoom Controller.

            if (m_fovController)
                m_fovController.enabled = false;
        }

        private void SetPhysicsCamera(bool enabled, bool rear = false)
        {
            if (enabled)
            {
                // If there's any physics camera enabled, disable it first.

                if (!rear)
                    {
                    if (physicsCameraRear != null)
                        physicsCameraRear.SetActive(false);
                    }
                else
                    {
                    if (physicsCameraFront != null)
                        physicsCameraFront.SetActive(false);
                    }

                // Now enable the expected camera.

                if (physicsCameraFront != null)
                    physicsCameraFront.SetActive(!rear);
                if (physicsCameraRear != null)
                    physicsCameraRear.SetActive(rear);
            }
            else
            {
                if (physicsCameraFront != null)
                    physicsCameraFront.SetActive(false);
                if (physicsCameraRear != null)
                    physicsCameraRear.SetActive(false);
            }
        }

        private void SetTVMode()
        {
            // Save current camera FoV so TV mode may change it.

            m_vppController.enabled = false;
            m_savedCameraFov = mainCamera.fieldOfView;

            m_cmController.enabled = true;
        }
    }
}
