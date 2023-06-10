using Perrinn424.Utilities;
using UnityEngine;

namespace Perrinn424.CameraSystem
{
    internal class OnboardCamerasController : MonoBehaviour
    {
        private CircularIterator<Transform> iterator;

        [SerializeField]
        private Camera onboardCamera;

        [SerializeField]
        private Transform[] pivots;

        private void OnEnable()
        {
            iterator = new CircularIterator<Transform>(pivots);
            MoveCamera(iterator.Current);
        }

        public void NextCamera()
        {
            MoveCamera(iterator.MoveNext());
        }

        public void PreviousCamera()
        {
            MoveCamera(iterator.MovePrevious());
        }

        private void MoveCamera(Transform pivot)
        {
            onboardCamera.transform.SetParent(pivot, false);
        }
    }
}
