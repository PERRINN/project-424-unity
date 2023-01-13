using UnityEngine;
using VehiclePhysics;

namespace Perrinn424.CameraSystem
{
    [RequireComponent(typeof(CameraController))]
    internal class CameraControllerInput : MonoBehaviour
    {
        [SerializeField]
        private CameraController cameraController;

        [SerializeField]
        private VPCameraController VPCameraController;

        public KeyCode nextMode = KeyCode.C;
        public KeyCode driverMode = KeyCode.F1;
        public KeyCode followMode = KeyCode.F2;
        public KeyCode orbitMode = KeyCode.F3;
        public KeyCode tvMode = KeyCode.F4;


        private KeyCode vpCameraControllerKey;


        private void OnEnable()
        {
            vpCameraControllerKey = VPCameraController.changeCameraKey;
            VPCameraController.changeCameraKey = KeyCode.None;
        }

        private void OnDisable()
        {
            VPCameraController.changeCameraKey = vpCameraControllerKey;
        }

        private void Update()
        {
            if (Input.GetKeyDown(nextMode))
            {
                cameraController.NextMode();
            }

            if (Input.GetKeyDown(driverMode))
            {
                cameraController.SetVPCamera(VPCameraController.Mode.Driver);
            }

            if (Input.GetKeyDown(followMode))
            {
                cameraController.SetVPCamera(VPCameraController.Mode.SmoothFollow);
            }

            if (Input.GetKeyDown(orbitMode))
            {
                cameraController.SetVPCamera(VPCameraController.Mode.Orbit);
            }

            if (Input.GetKeyDown(tvMode))
            {
                cameraController.SetTVMode();
            }
        }

        private void Reset()
        {
            cameraController = this.GetComponent<CameraController>();
        }
    }
}
