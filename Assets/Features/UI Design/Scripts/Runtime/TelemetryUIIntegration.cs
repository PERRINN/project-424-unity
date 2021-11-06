using Perrinn424.Utilities;
using System.Collections;
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

        private bool initialized = false;

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

        protected override void OnEnable()
        {
            modes = new CircularIterator<Mode>(new Mode[] { Mode.Slim, Mode.Wide, Mode.ChannelList, Mode.Off });

            screenCoordinatesUtility.RectTransformDimensionsChanged += UpdateTelemetryDimensions;
            modes.Current = mode;
            SetMode(modes.Current);
            StartCoroutine(DoDeferredInit());
        }

        protected override void OnDisable()
        {
            screenCoordinatesUtility.RectTransformDimensionsChanged -= UpdateTelemetryDimensions;
            SetTelemetryProperties(false, false, 0f);
        }

        private IEnumerator DoDeferredInit()
        {
            if (initialized)
                yield break;

            while (telemetryTools.vehicle == null || !telemetryTools.vehicle.initialized)
                yield return null;

            telemetryTools.showChannelList = true;
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            UpdateTelemetryDimensions(screenCoordinatesUtility.Rect);
            telemetryTools.showChannelList = mode == Mode.ChannelList;

            initialized = true;
        }

        private void UpdateTelemetryDimensions(Rect screenCoordinates)
        {
            telemetryDisplay.displayX = Mathf.RoundToInt(screenCoordinates.x);
            telemetryDisplay.displayY = Mathf.RoundToInt(screenCoordinates.y);
            telemetryDisplay.displayWidth = Mathf.RoundToInt(screenCoordinates.width);
            telemetryDisplay.displayHeight = Mathf.RoundToInt(screenCoordinates.height);

            Vector2 pos = screenCoordinates.position;
            pos.x = screenCoordinates.x + screenCoordinates.size.x - telemetryTools.channelListRect.width;
            telemetryTools.listSettings.position = pos;
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

        private void SetTelemetryProperties(bool showChannelList, bool showDisplay, float ratio)
        {
            SetRatio(ratio);

            telemetryDisplay.showDisplay = showDisplay;
            telemetryTools.showChannelList = showChannelList;
        }

        private void SetRatio(float ratio)
        {
            RectTransform parentRectTransform = this.transform.parent as RectTransform;
            Vector2 anchorMin = parentRectTransform.anchorMin;
            float minAnchorX = 1.0f - ratio;
            anchorMin.x = minAnchorX;
            parentRectTransform.anchorMin = anchorMin;
        }
    }
}
