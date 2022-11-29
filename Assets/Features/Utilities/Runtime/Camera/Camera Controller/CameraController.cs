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
        public VPCameraController cameraController;


        private CircularIterator<Mode> modeIterator;


        private void OnEnable()
        {
            modeIterator = new CircularIterator<Mode>(new[] { Mode.Driver, Mode.SmoothFollow, Mode.Orbit, Mode.Tv });
            UpdateMode();
        }

        private void OnDisable()
        {
        }

        public void NextMode()
        {
            modeIterator.MoveNext();
            UpdateMode();
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
            cameraController.gameObject.SetActive(true);
            tvCameraSystem.gameObject.SetActive(false);
            cameraController.mode = mode;
        }

        private void SetTVMode()
        {
            cameraController.gameObject.SetActive(false);
            tvCameraSystem.gameObject.SetActive(true);
        }
    }
}
