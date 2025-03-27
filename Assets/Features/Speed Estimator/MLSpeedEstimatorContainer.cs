using Perrinn424.TelemetryLapSystem;
using Unity.Sentis;
using UnityEngine;
using UnityEngine.Assertions;
using VehiclePhysics;
using VehiclePhysics.Timing;

namespace Perrinn424.SpeedEstimatorSystem
{
    public class MLSpeedEstimatorContainer : VehicleBehaviour
    {
        [SerializeField]
        private LapTimer lapTimer;

        [SerializeField]
        private Channels channels;

        public ModelAsset modelAsset;

        public float Error { get; private set; }

        public float EstimatedSpeed { get; private set; }

        private MLSpeedEstimator aiSpeedEstimator;
        private MLSpeedEstimatorInput input;


        [SerializeField]
        private Frequency frequency;

        public float EstimatedLapDistance { get; private set; }

        public override void OnEnableVehicle()
        {
            aiSpeedEstimator = new MLSpeedEstimator(modelAsset);
            frequency.Reset();

            lapTimer.onBeginLap += LapBeginEventHandler;
        }

        private void Start()
        {
            channels.Reset(vehicle);
            Assert.IsTrue(channels.AreAllChannelsValid(), "AISpeedEstimatorContainer channels are not valid. Please check");
        }

        public override void FixedUpdateVehicle()
        {
            EstimatedLapDistance += EstimatedSpeed * Time.deltaTime;

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
            Error=(EstimatedSpeed-speed)/Mathf.Max(10,speed)*100;
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

        public override void OnDisableVehicle()
        {
            aiSpeedEstimator.Dispose();
            lapTimer.onBeginLap -= LapBeginEventHandler;
        }

        private void LapBeginEventHandler()
        {
            EstimatedLapDistance = 0;
        }
    }
}
