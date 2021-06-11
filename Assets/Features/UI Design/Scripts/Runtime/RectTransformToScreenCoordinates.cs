using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Perrinn424
{
    public class RectTransformToScreenCoordinates : UIBehaviour
    {
        public Vector2 Size => Rect.size;
        public Vector2 Position => Rect.position;
        public Rect Rect { get; private set; }

        public event Action<Rect> onRectTransformDimensionsChange;

        private Vector3 [] screenCorners = new Vector3[4];

        protected override void OnRectTransformDimensionsChange()
        {
            CalculateScreenSpace();
            Debug.Log($"{Size} {Position}");
            onRectTransformDimensionsChange?.Invoke(Rect);
        }

        private void CalculateScreenSpace()
        {
            RectTransform rectTransform = this.transform as RectTransform;
            rectTransform.GetWorldCorners(screenCorners);

            float x = screenCorners[0].x;
            float y = screenCorners[0].y;
            float width = screenCorners[2].x - screenCorners[0].x;
            float height = screenCorners[1].y - screenCorners[0].y;
            Rect = new Rect(x, y, width, height);

            //telemetryDisplay.displayWidth = (int)rect.width;
            //telemetryDisplay.displayWidth = (int)(screenCoordinatesUtility.fourCorners[2].x - screenCoordinatesUtility.fourCorners[0].x);
            //telemetryDisplay.displayHeight = (int)rect.height;
            //telemetryDisplay.displayHeight = (int)(screenCoordinatesUtility.fourCorners[1].y - screenCoordinatesUtility.fourCorners[0].y);
            //telemetryDisplay.displayX = (int)rect.x;
            //telemetryDisplay.displayX = (int)screenCoordinatesUtility.fourCorners[0].x;
            //telemetryDisplay.displayY = (int)rect.y;
            //telemetryDisplay.displayY = (int)screenCoordinatesUtility.fourCorners[0].y;
        }
    } 
}
