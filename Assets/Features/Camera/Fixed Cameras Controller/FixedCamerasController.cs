using Perrinn424.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Perrinn424.CameraSystem
{
    internal class FixedCamerasController : MonoBehaviour
    {
        private CircularBuffer<Camera> circularBuffer;

        private void Awake()
        {
            Camera[] cameras = this.GetComponentsInChildren<Camera>(true);

            foreach (Camera c in cameras)
            {
                c.gameObject.SetActive(false);
            }

            circularBuffer = new CircularBuffer<Camera>(cameras);

            circularBuffer.Current.gameObject.SetActive(true);
        }

        public void NextCamera()
        {
            circularBuffer.Current.gameObject.SetActive(false);
            circularBuffer.MoveNext().gameObject.SetActive(true);
        }

        public void PreviousCamera()
        {
            circularBuffer.Current.gameObject.SetActive(false);
            circularBuffer.MovePrevious().gameObject.SetActive(true);
        }
    } 
}
