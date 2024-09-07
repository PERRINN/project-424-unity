using Perrinn424.TelemetryLapSystem;
using Unity.Sentis;
using UnityEngine;
using VehiclePhysics;

namespace Perrinn424.AISpeedEstimatorSystem
{
    public class AISpeedEstimatorContainer : VehicleBehaviour
    {
        [SerializeField]
        private Channels channels;

        public ModelAsset modelAsset;

        public float Error { get; private set; }

        public float EstimatedSpeed { get; private set; }

        private AISpeedEstimator aiSpeedEstimator;
        private AISpeedEstimatorInput input;


        [SerializeField]
        private Frequency frequency;

        public override void OnEnableVehicle()
        {
            aiSpeedEstimator = new AISpeedEstimator(modelAsset);
            channels.Reset(vehicle);
            frequency.Reset();
        }

        public override void OnDisableVehicle()
        {
            aiSpeedEstimator.Dispose();
        }


        public override void FixedUpdateVehicle()
        {
            if (frequency.Update(Time.deltaTime))
            {
                EstimateSpeed();
            }
        }

        private void EstimateSpeed()
        {
            UpdateInput();
            aiSpeedEstimator.Estimate(ref input);
            SetEstimatedSpeed(aiSpeedEstimator.EstimatedSpeed);
        }

        private void SetEstimatedSpeed(float estimatedSpeed)
        {
            EstimatedSpeed = estimatedSpeed;
            float speed = vehicle.speed;
            Error = Mathf.Abs(speed - EstimatedSpeed);
        }

        private void UpdateInput()
        {
            input.throttle = channels.GetValue(0);
            input.brake = channels.GetValue(1);
            input.accelerationLateral = channels.GetValue(2);
            input.accelerationLongitudinal = channels.GetValue(3);
            input.accelerationVertical = channels.GetValue(4);
            input.nWheelFL = channels.GetValue(5);
            input.nWheelFR = channels.GetValue(6);
            input.nWheelRL = channels.GetValue(7);
            input.nWheelRR = channels.GetValue(8);
            input.steeringAngle = channels.GetValue(9);
        }
    } 
}
