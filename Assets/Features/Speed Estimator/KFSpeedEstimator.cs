using Perrinn424.TelemetryLapSystem;
using UnityEngine;
using UnityEngine.Assertions;
using VehiclePhysics;
using VehiclePhysics.Timing;

namespace Perrinn424.SpeedEstimatorSystem
{
    public class KFSpeedEstimator : VehicleBehaviour
    {

        [SerializeField]
        private LapTimer lapTimer;

        [SerializeField]
        private Channels channels;

        /// <summary>
        /// Kalman Filter Speed in m/s
        /// </summary>
        public float KFSpeed => kfSpeed/3.6f;
        public float KFError => kfError;

        public float EstimatedLapDistance { get; private set; }


        private float kfSpeed;
        private float kfError;
        private float absError;
        private bool longitudinalAccelerationGreaterThanZero;

        private void Start()
        {
            channels.Reset(vehicle);
            Assert.IsTrue(channels.AreAllChannelsValid(), "KFSpeedEstimator channels are not valid. Please check");
        }

        public override void OnEnableVehicle()
        {
            lapTimer.onBeginLap += LapBeginEventHandler;
        }

        private void LapBeginEventHandler()
        {
            EstimatedLapDistance = 0;
        }

        public override void OnDisableVehicle()
        {
            lapTimer.onBeginLap -= LapBeginEventHandler;
        }


        public override void FixedUpdateVehicle()
        {
            GetKFByChannels();
            EstimatedLapDistance += KFSpeed * Time.deltaTime;

        }

        private void GetKFByChannels()
        {
            float localAccelerationG = channels.GetValue(0); // G
            float wheelSpeedFL = channels.GetValue(1); //kph
            float wheelSpeedFR = channels.GetValue(2); //kph
            float wheelSpeedRL = channels.GetValue(3); //kph
            float wheelSpeedRR = channels.GetValue(4); //kph

            kfSpeed = CalculateKFSpeed(kfSpeed, wheelSpeedFL, wheelSpeedFR, wheelSpeedRL, wheelSpeedRR, localAccelerationG);
            kfError = CalculateKFError(kfSpeed, vehicle.speed * 3.6f);
            absError = Mathf.Abs(kfSpeed - vehicle.speed * 3.6f);

            longitudinalAccelerationGreaterThanZero = localAccelerationG > 0;
        }

        //private void GetKFByVehicle()
        //{
        //    Vector3 localAcceleration = vehicle.localAcceleration;
        //    float longitudinalAcceleration = localAcceleration.z;

        //    float[] wheelSpeed = new float[4];
        //    for (int i = 0; i < vehicle.wheelCount; i++)
        //    {
        //        float wheelRadius = vehicle.GetWheelRadius(i);
        //        WheelState ws = vehicle.wheelState[i];

        //        wheelSpeed[i] = ws.angularVelocity * wheelRadius * 3.6f;
        //    }

        //    KFSpeed = CalculateKFSpeed(KFSpeed, wheelSpeed[0], wheelSpeed[1], wheelSpeed[2], wheelSpeed[3], longitudinalAcceleration / 9.81f);
        //    KFError = CalculateKFError(KFSpeed, vehicle.speed * 3.6f);
        //    absError = Mathf.Abs(KFSpeed - vehicle.speed * 3.6f);

        //    longitudinalAccelerationGreaterThanZero = longitudinalAcceleration > 0;
        //}

        /// <summary>
        /// KFSpeed
        /// </summary>
        /// <param name="previousKFSpeed">kph</param>
        /// <param name="wheelSpeedFL">kph</param>
        /// <param name="wheelSpeedFR">kph</param>
        /// <param name="wheelSpeedRL">kph</param>
        /// <param name="wheelSpeedRR">kph</param>
        /// <param name="longitudinalAcceleration">G</param>
        /// <returns>kph</returns>
        private static float CalculateKFSpeed(
            float previousKFSpeed,
            float wheelSpeedFL, float wheelSpeedFR, float wheelSpeedRL, float wheelSpeedRR,
            float longitudinalAcceleration)
        {
            float averageWheelSpeed = wheelSpeedFL + wheelSpeedFR + wheelSpeedRL + wheelSpeedRR;
            averageWheelSpeed /= 4;
            float KFSpeedCandidate = previousKFSpeed + longitudinalAcceleration * Time.deltaTime * 9.81f * 3.6f;
            if (longitudinalAcceleration > 0)
            {
                return Mathf.Min(averageWheelSpeed, KFSpeedCandidate);
            }
            else
            {
                return Mathf.Max(averageWheelSpeed, KFSpeedCandidate);
            }
        }

        /// <summary>
        /// KFError
        /// </summary>
        /// <param name="KFSpeed">kph</param>
        /// <param name="speed">kph</param>
        /// <returns></returns>
        private static float CalculateKFError(float KFSpeed, float speed)
        {
            float error = KFSpeed - speed;
            return 100f * (error / Mathf.Max(10f, speed));
        }
    } 
}
