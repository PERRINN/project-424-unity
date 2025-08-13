using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using VehiclePhysics;
﻿using Perrinn424.Utilities;
using VersionCompatibility;

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

        public RectTransformToScreenCoordinates screenCoordinatesUtility;
        public VPTelemetryDisplay telemetryDisplay;
        public VPTelemetryTools telemetryTools;

        public Mode mode;
        public float slimRatio = 1.0f;
        public float wideRatio = 2.5f;
        public int minHeightForThickLines = 1000;

        private bool initialized = false;
        private bool vrEnabled = false;
        private Mode preVrMode;

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
            if (telemetryDisplay == null || !telemetryDisplay.enabled)
            {
                modes = new CircularIterator<Mode>(new Mode[] { Mode.ChannelList, Mode.Off });
                if (mode == Mode.Slim || mode == Mode.Wide)
                    mode = Mode.Off;
            }
            else
            {
                modes = new CircularIterator<Mode>(new Mode[] { Mode.Slim, Mode.Wide, Mode.ChannelList, Mode.Off });
            }

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
            telemetryDisplay.thickLines = telemetryDisplay.displayHeight > minHeightForThickLines;

            Vector2 pos = screenCoordinates.position;
            pos.x = screenCoordinates.x + screenCoordinates.size.x - telemetryTools.channelListRect.width;
            telemetryTools.listSettings.position = pos;
        }

        protected override void OnCanvasHierarchyChanged()
        {
            if (isActiveAndEnabled)
                telemetryDisplay.showDisplay = Canvas.isActiveAndEnabled;
        }

        private void Update()
        {
            if (UnityInput.GetKeyDown(UnityKey.T))
            {
                modes.MoveNext();
                SetMode(modes.Current);
            }

            // Detect VR and auto-disable telemetry

            if (!vrEnabled && XRSettings.isDeviceActive)
            {
                vrEnabled = true;
                preVrMode = modes.Current;
                modes.Current = Mode.Off;
                SetMode(Mode.Off);
            }
            else
            if (vrEnabled && !XRSettings.isDeviceActive)
            {
                vrEnabled = false;
                if (modes.Current == Mode.Off)
                {
                    modes.Current = preVrMode;
                    SetMode(preVrMode);
                }
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
            RectTransform parentRect = this.transform.parent as RectTransform;
            float width = parentRect.rect.width;
            (this.transform as RectTransform).SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, width * ratio);
        }
    }
}
