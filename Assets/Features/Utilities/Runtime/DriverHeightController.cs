using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Perrinn424.Utilities
{
    public class DriverHeightController : MonoBehaviour
    {
        [SerializeField]
        private KeyCode increaseHeight;
        [SerializeField]
        private KeyCode decreaseHeight;

        [SerializeField]
        private Transform firstPersonCamera;

        public float step = 0.025f;
        public float max = 0.05f;

        private void Update()
        {
            if (Input.GetKeyDown(increaseHeight))
            {
                Change(step);
            }
            else if (Input.GetKeyDown(decreaseHeight))
            {
                Change(-step);
            }
        }

        private void Change(float delta)
        {
            Vector3 pos = firstPersonCamera.localPosition;
            pos.y = Mathf.Clamp(pos.y + delta, -max, max);
            firstPersonCamera.localPosition = pos;
        }
    } 
}
