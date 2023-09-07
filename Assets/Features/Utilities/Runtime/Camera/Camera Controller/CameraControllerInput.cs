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
                cameraController.SetMode(CameraController.Mode.Driver);
            }

            if (Input.GetKeyDown(followMode))
            {
                cameraController.SetMode(CameraController.Mode.SmoothFollow);
            }

            if (Input.GetKeyDown(orbitMode))
            {
                if (VPCameraController.orbit.targetRelative)
                    cameraController.SetMode(CameraController.Mode.OrbitFixed);
                else
                    cameraController.SetMode(CameraController.Mode.Orbit);
            }

            if (Input.GetKeyDown(tvMode))
            {
                cameraController.SetMode(CameraController.Mode.Tv);
            }
        }

        private void Reset()
        {
            cameraController = this.GetComponent<CameraController>();
        }
    }
}
