using UnityEngine;
using UnityEngine.EventSystems;
using VehiclePhysics;

namespace Perrinn424.UI
{
    public class TelemetryUIIntegration : UIBehaviour
    {
        public enum Mode
        {
            Small,
            Large
        }

        [SerializeField]
        private RectTransformToScreenCoordinates screenCoordinatesUtility;
        
        [SerializeField]
        private VPTelemetryDisplay telemetryDisplay;

        [SerializeField]
        private Mode mode;
        [SerializeField]
        private float smallRatio = 0.20f;
        [SerializeField]
        private float largeRatio = 0.35f;

        private Canvas canvas;
        private Canvas Canvas
        {
            get
            {
                if (canvas == null)
                {
                    canvas = this.GetComponentInParent<Canvas>();
                }

                return canvas;
            }
        }

        protected override void OnEnable()
        {
            screenCoordinatesUtility.RectTransformDimensionsChanged += UpdateTelemetryDimensions;
            SetMode(mode);
        }

        protected override void OnDisable()
        {
            screenCoordinatesUtility.RectTransformDimensionsChanged -= UpdateTelemetryDimensions;
        }

        private void UpdateTelemetryDimensions(Rect screenCoordinates)
        {
            telemetryDisplay.displayX = Mathf.RoundToInt(screenCoordinates.x);
            telemetryDisplay.displayY = Mathf.RoundToInt(screenCoordinates.y);
            telemetryDisplay.displayWidth = Mathf.RoundToInt(screenCoordinates.width);
            telemetryDisplay.displayHeight = Mathf.RoundToInt(screenCoordinates.height);
        }

        protected override void OnCanvasHierarchyChanged()
        {
            telemetryDisplay.showDisplay = Canvas.isActiveAndEnabled;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                SetMode(mode == Mode.Small ? Mode.Large : Mode.Small);    
            }
        }

        private void SetMode(Mode newMode)
        {
            this.mode = newMode;
            ChangeTelemetrySizeInCanvas();
        }

        private void ChangeTelemetrySizeInCanvas()
        {
            RectTransform parentRectTransform = this.transform.parent as RectTransform;
            Vector2 anchorMin = parentRectTransform.anchorMin;
            float minAnchorX = 1.0f - (mode == Mode.Small ? smallRatio : largeRatio);
            anchorMin.x = minAnchorX;
            parentRectTransform.anchorMin = anchorMin;
        }
    } 
}
