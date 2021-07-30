using System;
using UnityEngine;
using VehiclePhysics;

namespace Perrinn424.TrackMapSystem
{
    [Serializable]
    public class TelemetryTrackReference : BaseTrackReference
    {
        [SerializeField]
        private VehicleBase vehicle;

        private VPTelemetryDisplay telemetryDisplay;

        private Vector3 position;
        public override Vector3 Position => position;
        
        private bool isValid;
        protected override bool IsValid => isValid;

        private int positionXChannelIndex;
        private int positionYChannelIndex;
        private int positionZChannelIndex;

        public override void Init()
        {
            GetChannelIndices();

            telemetryDisplay = vehicle.GetComponentInChildren<VPTelemetryDisplay>();
        }

        private void GetChannelIndices()
        {
            positionXChannelIndex = vehicle.telemetry.GetChannelIndex("PositionX");
            positionYChannelIndex = vehicle.telemetry.GetChannelIndex("PositionY");
            positionZChannelIndex = vehicle.telemetry.GetChannelIndex("PositionZ");
        }

        protected override void Precalculate()
        {
            if (positionXChannelIndex < 0)
            {
                GetChannelIndices();
            }

            int telemetryEntry = telemetryDisplay.GetPointerEntry();
            if (telemetryEntry >= 0)
            {
                float positionX = telemetryDisplay.GetChannelValue(positionXChannelIndex, telemetryEntry);
                float positionY = telemetryDisplay.GetChannelValue(positionYChannelIndex, telemetryEntry);
                float positionZ = telemetryDisplay.GetChannelValue(positionZChannelIndex, telemetryEntry);

                if (!float.IsNaN(positionX) && !float.IsNaN(positionY) && !float.IsNaN(positionZ))
                {
                    position = new Vector3(positionX, positionY, positionZ);
                    isValid = true;
                    return;
                }
            }

            isValid = false;
        }
    } 
}
