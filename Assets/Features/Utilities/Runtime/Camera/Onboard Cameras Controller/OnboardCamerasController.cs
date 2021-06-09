using UnityEngine;

namespace Perrinn424.Utilities
{
    internal class OnboardCamerasController : MonoBehaviour
    {
        private CircularIterator<Camera> iterator;

        private void OnEnable()
        {
            Camera[] cameras = this.GetComponentsInChildren<Camera>(true);

            foreach (Camera c in cameras)
            {
                c.gameObject.SetActive(false);
            }

            iterator = new CircularIterator<Camera>(cameras);

            iterator.Current.gameObject.SetActive(true);
        }

        public void NextCamera()
        {
            iterator.Current.gameObject.SetActive(false);
            iterator.MoveNext().gameObject.SetActive(true);
        }

        public void PreviousCamera()
        {
            iterator.Current.gameObject.SetActive(false);
            iterator.MovePrevious().gameObject.SetActive(true);
        }
    }
}
