using UnityEngine;
using VehiclePhysics;
using VersionCompatibility;

namespace Perrinn424.CameraSystem
{
    [RequireComponent(typeof(CameraController))]
    internal class CameraControllerInput : MonoBehaviour
    {
        [SerializeField]
        private CameraController cameraController;

        [SerializeField]
        private VPCameraController VPCameraController;

        public UnityKey nextMode = UnityKey.C;
        public UnityKey driverMode = UnityKey.F1;
        public UnityKey followMode = UnityKey.F2;
        public UnityKey orbitMode = UnityKey.F3;
        public UnityKey tvMode = UnityKey.F4;
        public UnityKey physicsFrontMode = UnityKey.F5;
        public UnityKey physicsRearMode = UnityKey.F6;


        private UnityKey vpCameraControllerKey;


        private void OnEnable()
        {
            vpCameraControllerKey = VPCameraController.changeCameraKey;
            VPCameraController.changeCameraKey = UnityKey.None;
        }

        private void OnDisable()
        {
            VPCameraController.changeCameraKey = vpCameraControllerKey;
        }

        private void Update()
        {
            if (UnityInput.GetKeyDown(nextMode))
            {
                cameraController.NextMode();
            }
            // Disable F-keys on WebGL
            #if !UNITY_WEBGL
            if (UnityInput.GetKeyDown(driverMode))
            {
                cameraController.SetMode(CameraController.Mode.Driver);
            }

            if (UnityInput.GetKeyDown(followMode))
            {
                cameraController.SetMode(CameraController.Mode.SmoothFollow);
            }

            if (UnityInput.GetKeyDown(orbitMode))
            {
                if (VPCameraController.orbit.targetRelative)
                    cameraController.SetMode(CameraController.Mode.OrbitFixed);
                else
                    cameraController.SetMode(CameraController.Mode.Orbit);
            }

            if (UnityInput.GetKeyDown(tvMode))
            {
                cameraController.SetMode(CameraController.Mode.Tv);
            }

            if (UnityInput.GetKeyDown(physicsFrontMode))
            {
                cameraController.SetMode(CameraController.Mode.PhysicsFront);
            }

            if (UnityInput.GetKeyDown(physicsRearMode))
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
