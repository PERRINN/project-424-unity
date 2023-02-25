using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Perrinn424.UI
{
    public class RectTransformToScreenCoordinates : UIBehaviour
    {
        public Vector2 Size => Rect.size;
        public Vector2 Position => Rect.position;
        public Rect Rect { get; private set; }

        public event Action<Rect> RectTransformDimensionsChanged;

        private Vector3 [] screenCorners = new Vector3[4];

        protected override void OnEnable()
        {
            UpdateScreenCoordinates();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            UpdateScreenCoordinates();
        }

        private void UpdateScreenCoordinates()
        {
            CalculateScreenSpace();
            RectTransformDimensionsChanged?.Invoke(Rect);
        }

        private void CalculateScreenSpace()
        {
            RectTransform rectTransform = this.transform as RectTransform;
            rectTransform.GetWorldCorners(screenCorners);

            float x = screenCorners[0].x;
            float y = Screen.height - screenCorners[1].y;
            float width = screenCorners[2].x - screenCorners[0].x;
            float height = screenCorners[1].y - screenCorners[0].y;
            Rect = new Rect(x, y, width, height);
        }
    } 
}
