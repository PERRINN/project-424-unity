using Perrinn424.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using VehiclePhysics;

namespace Perrinn424.UI
{
    public class TelemetryUIIntegration : UIBehaviour
    {
        public enum Mode
        {
            Slim,
            Wide,
            ChannelList,
            Off
        }

        [SerializeField]
        private RectTransformToScreenCoordinates screenCoordinatesUtility;
        
        [SerializeField]
        private VPTelemetryDisplay telemetryDisplay;
        [SerializeField]
        private VPTelemetryTools telemetryTools;

        [SerializeField]
        private Mode mode;
        [SerializeField]
        private float slimRatio = 0.20f;
        [SerializeField]
        private float wideRatio = 0.35f;

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

        private CircularIterator<Mode> modes;

        protected override void Awake()
        {
            modes = new CircularIterator<Mode>(new Mode[] { Mode.Slim, Mode.Wide, Mode.ChannelList, Mode.Off });
        }

        protected override void OnEnable()
        {
            screenCoordinatesUtility.RectTransformDimensionsChanged += UpdateTelemetryDimensions;
            SetMode(modes.Current);
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

            telemetryTools.listSettings.position = screenCoordinates.position;

        }

        protected override void OnCanvasHierarchyChanged()
        {
            telemetryDisplay.showDisplay = Canvas.isActiveAndEnabled;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                modes.MoveNext();
                SetMode(modes.Current);
            }
        }

        private void SetMode(Mode newMode)
        {
            this.mode = newMode;
            switch (mode)
            {
                case Mode.Slim:
                    SetTelemetryProperties(false, true, slimRatio);
                    break;
                case Mode.Wide:
                    SetTelemetryProperties(false, true, wideRatio);
                    break;
                case Mode.ChannelList:
                    SetTelemetryProperties(true, false, wideRatio);
                    break;
                case Mode.Off:
                    SetTelemetryProperties(false, false, slimRatio);
                    break;
            }
        }

        private void SetTelemetryProperties(bool showChannelList, bool showDisplay, float size)
        {
            telemetryTools.showChannelList = showChannelList;
            telemetryDisplay.showDisplay = showDisplay;
            SetSize(size);
        }

        private void SetSize(float size)
        {
            RectTransform parentRectTransform = this.transform.parent as RectTransform;
            Vector2 anchorMin = parentRectTransform.anchorMin;
            float minAnchorX = 1.0f - size;
            anchorMin.x = minAnchorX;
            parentRectTransform.anchorMin = anchorMin;
        }
    }
}
