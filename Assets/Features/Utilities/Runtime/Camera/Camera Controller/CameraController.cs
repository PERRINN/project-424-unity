using Cinemachine;
using Perrinn424.Utilities;
using UnityEngine;
using UnityEngine.Assertions;
using VehiclePhysics;

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

        private VPCameraController m_vppController;
        private CinemachineBrain m_cmController;


        private CircularIterator<Mode> modeIterator;


        private void OnEnable()
        {
            modeIterator = new CircularIterator<Mode>(new[] { Mode.Driver, Mode.SmoothFollow, Mode.Orbit, Mode.Tv });
            m_vppController = tvCameraSystem.GetComponent<VPCameraController>();
            m_cmController = tvCameraSystem.GetComponent<CinemachineBrain>();
            UpdateMode();
        }

        private void OnDisable()
        {
        }

        public void NextMode()
        {
            if (isActiveAndEnabled)
            {
                modeIterator.MoveNext();
                UpdateMode();
            }
        }

        private void UpdateMode()
        {
            switch (modeIterator.Current)
            {
                case Mode.Driver:
                case Mode.SmoothFollow:
                case Mode.Orbit:
                case Mode.LookAt:
                case Mode.Free:
                    SetVPCamera((VPCameraController.Mode)modeIterator.Current);
                    break;
                case Mode.Tv:
                    SetTVMode();
                    break;
            }
        }


        private void SetVPCamera(VPCameraController.Mode mode)
        {
            m_cmController.enabled = false;
            m_vppController.enabled = true;
            m_vppController.mode = mode;
        }

        private void SetTVMode()
        {
            m_vppController.enabled = false;
            m_cmController.enabled = true;
        }
    }
}
