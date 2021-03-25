using UnityEngine;

namespace Perrinn424.Utilities
{
    [RequireComponent(typeof(OnboardCamerasController))]
    internal class OnboardCamerasControllerInput : MonoBehaviour
    {
        [SerializeField]
        private OnboardCamerasController onboardCamerasController;
        
        [SerializeField]
        private KeyCode nextCameraKey = KeyCode.V;
        
        private void Update()
        {
            if (Input.GetKeyDown(nextCameraKey))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    onboardCamerasController.PreviousCamera();
                }
                else
                {
                    onboardCamerasController.NextCamera();
                }
            }
        }

        private void Reset()
        {
            onboardCamerasController = this.GetComponent<OnboardCamerasController>();
        }
    } 
}
