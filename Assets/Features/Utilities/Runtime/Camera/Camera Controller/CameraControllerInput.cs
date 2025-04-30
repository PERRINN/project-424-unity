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
        public KeyCode physicsFrontMode = KeyCode.F5;
        public KeyCode physicsRearMode = KeyCode.F6;


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
            // Disable F-keys on WebGL
            #if !UNITY_WEBGL
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

            if (Input.GetKeyDown(physicsFrontMode))
            {
                cameraController.SetMode(CameraController.Mode.PhysicsFront);
            }

            if (Input.GetKeyDown(physicsRearMode))
            {
                cameraController.SetMode(CameraController.Mode.PhysicsRear);
            }
            #endif
        }

        private void Reset()
        {
            cameraController = this.GetComponent<CameraController>();
        }
    }
}
