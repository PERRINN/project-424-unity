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

        [SerializeField]
        private KeyCode nextMode = KeyCode.C;

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
        }

        private void Reset()
        {
            cameraController = this.GetComponent<CameraController>();
        }
    }
}
