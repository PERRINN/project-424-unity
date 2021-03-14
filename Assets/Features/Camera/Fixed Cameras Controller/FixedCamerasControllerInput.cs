using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Perrinn424.CameraSystem
{
    [RequireComponent(typeof(FixedCamerasController))]
    public class FixedCamerasControllerInput : MonoBehaviour
    {
        [SerializeField]
        private FixedCamerasController fixedCamerasController;
        
        [SerializeField]
        private KeyCode nextCameraKey = KeyCode.P;
        
        [SerializeField]
        private KeyCode previousCameraKey = KeyCode.O;

        private void Update()
        {
            if (Input.GetKeyDown(nextCameraKey))
            {
                fixedCamerasController.NextCamera();
            }
            else if (Input.GetKeyDown(previousCameraKey))
            {
                fixedCamerasController.PreviousCamera();
            }
        }

        private void Reset()
        {
            fixedCamerasController = this.GetComponent<FixedCamerasController>();
        }
    } 
}
