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


        [SerializeField]
        private Transform tvCameraSystem;
        private CinemachineVirtualCameraBase cinemachineVirtualCameraBase;

        [SerializeField]
        private VPCameraController cameraController;


        private CircularIterator<Mode> modeIterator;
        public Transform microphoneAnchor;
        private GameObject microphone;

        private void Awake()
        {
            cinemachineVirtualCameraBase = tvCameraSystem.GetComponentInChildren<CinemachineVirtualCameraBase>();
            Assert.IsNotNull(cinemachineVirtualCameraBase);
        }

        private void OnEnable()
        {
            modeIterator = new CircularIterator<Mode>(new[] { Mode.Driver, Mode.SmoothFollow, Mode.Orbit, Mode.LookAt, Mode.Free, Mode.Tv });
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
