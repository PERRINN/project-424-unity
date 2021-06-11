using UnityEngine;
using UnityEngine.EventSystems;
using VehiclePhysics;

namespace Perrinn424
{
    public class TelemetryUIIntegration : UIBehaviour
    {
        [SerializeField]
        private RectTransformToScreenCoordinates screenCoordinatesUtility;
        
        [SerializeField]
        private VPTelemetryDisplay telemetryDisplay;

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
            screenCoordinatesUtility.onRectTransformDimensionsChange += UpdateTelemetryDimensions;
        }

        protected override void OnDisable()
        {
            screenCoordinatesUtility.onRectTransformDimensionsChange -= UpdateTelemetryDimensions;
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
    } 
}
