using UnityEngine;

namespace Perrinn424.CameraSystem
{
    [RequireComponent(typeof(OnboardCamerasController))]
    internal class OnboardCamerasControllerInput : MonoBehaviour
    {
        [SerializeField]
        private OnboardCamerasController onboardCamerasController;
        
        [SerializeField]
        private KeyCode nextCameraKey = KeyCode.P;
        
        [SerializeField]
        private KeyCode previousCameraKey = KeyCode.O;

        private void Update()
        {
            if (Input.GetKeyDown(nextCameraKey))
            {
                onboardCamerasController.NextCamera();
            }
            else if (Input.GetKeyDown(previousCameraKey))
            {
                onboardCamerasController.PreviousCamera();
            }
        }

        private void Reset()
        {
            onboardCamerasController = this.GetComponent<OnboardCamerasController>();
        }
    } 
}
