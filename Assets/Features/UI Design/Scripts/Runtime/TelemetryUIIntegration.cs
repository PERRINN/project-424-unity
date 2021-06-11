using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VehiclePhysics;

namespace Perrinn424
{
    public class TelemetryUIIntegration : MonoBehaviour
    {
        [SerializeField]
        private RectTransformToScreenCoordinates screenCoordinatesUtility;
        
        [SerializeField]
        private VPTelemetryDisplay telemetryDisplay;

        private void OnEnable()
        {
            screenCoordinatesUtility.onRectTransformDimensionsChange += UpdateTelemetryDimensions;
        }

        private void OnDisable()
        {
            screenCoordinatesUtility.onRectTransformDimensionsChange -= UpdateTelemetryDimensions;
        }

        private void UpdateTelemetryDimensions(Rect screenCoordinates)
        {
            telemetryDisplay.displayX = (int)screenCoordinates.x;
            telemetryDisplay.displayY = (int)screenCoordinates.y;
            telemetryDisplay.displayWidth = (int)screenCoordinates.width;
            telemetryDisplay.displayHeight = (int)screenCoordinates.height;
            //telemetryDisplay.displayWidth = (int)rect.width;
            //telemetryDisplay.displayWidth = (int)(screenCoordinatesUtility.screenCorners[2].x - screenCoordinatesUtility.screenCorners[0].x);
            //telemetryDisplay.displayHeight = (int)rect.height;
            //telemetryDisplay.displayHeight = (int)(screenCoordinatesUtility.screenCorners[1].y - screenCoordinatesUtility.screenCorners[0].y);
            //telemetryDisplay.displayX = (int)rect.x;
            //telemetryDisplay.displayX = (int)screenCoordinatesUtility.screenCorners[0].x;
            //telemetryDisplay.displayY = (int)rect.y;
            //telemetryDisplay.displayY = (int)screenCoordinatesUtility.screenCorners[0].y;
        }
    } 
}
