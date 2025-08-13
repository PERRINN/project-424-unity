using UnityEngine;
using VersionCompatibility;

namespace Perrinn424.CameraSystem
{
    [RequireComponent(typeof(OnboardCamerasController))]
    internal class OnboardCamerasControllerInput : MonoBehaviour
    {
        [SerializeField]
        private OnboardCamerasController onboardCamerasController;

        [SerializeField]
        private UnityKey nextCameraKey = UnityKey.V;

        private void Update()
        {
            if (UnityInput.GetKeyDown(nextCameraKey))
            {
                if (UnityInput.GetKey(UnityKey.LeftShift))
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
