using UnityEngine;
using UnityEngine.Serialization;
using VehiclePhysics;
using VehiclePhysics.Timing;

namespace Perrinn424.AutopilotSystem
{
    public class Autopilot : BaseAutopilot
    {
        public enum InputType
        {
            Raw,
            Processed
        }


        [Header("References")]
        public RecordedLap recordedLap;

        [SerializeField]
        private LapTimer timer;

        [SerializeField]
        private PathDrawer pathDrawer;

        [Header("Setup")]

        public InputType inputType;
        
        [SerializeField]
        private AutopilotStartup startup;

        [FormerlySerializedAs("positionCorrector")]
        public PositionCorrector lateralCorrector;

        [SerializeField]
        private TimeCorrector timeCorrector;

        [FormerlySerializedAs("offset")]
        public float positionOffset = 0.9f;

        private AutopilotSearcher autopilotSearcher;
        private AutopilotDebugDrawer debugDrawer;
        private IPIDInfo pidInfo;

        public override void OnEnableComponent()
        {
            pidInfo = lateralCorrector;
        }

        public override void OnEnableVehicle()
        {
            autopilotSearcher = new AutopilotSearcher(this, recordedLap);
            lateralCorrector.Init(vehicle.cachedRigidbody);
            timeCorrector.Init(vehicle.cachedRigidbody);
            startup.Init(vehicle);
            debugDrawer = new AutopilotDebugDrawer();
            pathDrawer.recordedLap = recordedLap;
            pidInfo = lateralCorrector;

            vehicle.onBeforeUpdateBlocks += UpdateAutopilot;
        }

        public override void OnDisableVehicle()
        {
            vehicle.onBeforeUpdateBlocks -= UpdateAutopilot;
        }

        public void UpdateAutopilot()
        {
            autopilotSearcher.Search(vehicle.transform);
            pathDrawer.index = autopilotSearcher.StartIndex;

            if (IsOn)
            {
                UpdateAutopilotInOnStatus();
            }
        }

        private void UpdateAutopilotInOnStatus()
        {
            float expectedSpeed = CalculateExpectedSpeed(autopilotSearcher.Segment);
            startup.IsStartup(expectedSpeed);
            Sample runningSample = GetInterpolatedNearestSample();
            Vector3 targetPosition = autopilotSearcher.ProjectedPosition;

            float yawError = RotationCorrector.YawError(vehicle.transform.rotation, runningSample.rotation);

            if (yawError > 90f)
            {
                SetStatus(false);
            }
            else if (yawError > 30)
            {
                return;
            }


            if (IsStartup) //startup block
            {
                runningSample = startup.Correct(runningSample);
                pidInfo = startup.lateralCorrector;
            }
            else //main block
            {
                lateralCorrector.Correct(targetPosition);
                pidInfo = lateralCorrector;

                float currentTime = timer.currentLapTime;
                timeCorrector.Correct(PlayingTime(), currentTime);
            }

            debugDrawer.Set(targetPosition, lateralCorrector.ApplicationPosition, lateralCorrector.Force);
            WriteInput(runningSample);
        }

        public override float PlayingTime()
        {
            float sampleIndex = (autopilotSearcher.StartIndex + autopilotSearcher.Ratio);
            float playingTimeBySampleIndex = sampleIndex / recordedLap.frequency;
            float offset = vehicle.speed > 10f ? positionOffset / vehicle.speed : 0f;
            return playingTimeBySampleIndex - offset;
        }

        protected override void SetStatus(bool isOn)
        {
            if (isOn)
            {
                if (!CanOperate())
                {
                    Debug.LogWarning("Autopilot can't operate from these conditions");
                    return;
                }
            }

            base.SetStatus(isOn);
        }

        private bool CanOperate()
        {

            Quaternion pathRotation = recordedLap.samples[autopilotSearcher.StartIndex].rotation;
            float yawError = RotationCorrector.YawError(vehicle.transform.rotation, pathRotation);

            if (Mathf.Abs(yawError) > 30f)
            {
                return false;
            }

            return true;
        }


        private Sample GetInterpolatedNearestSample()
        {
            Sample start = recordedLap[autopilotSearcher.StartIndex];
            Sample end = recordedLap[autopilotSearcher.EndIndex];
            float t = autopilotSearcher.Ratio;
            Sample interpolatedSample = Sample.Lerp(start, end, t);
            
            return interpolatedSample;
        }

        private void WriteInput(Sample s)
        {
            if (inputType == InputType.Raw)
            {
                vehicle.data.Set(Channel.Custom, Perrinn424Data.EnableProcessedInput, 0);
                vehicle.data.Set(Channel.Input, InputData.Steer, s.rawSteer);
                vehicle.data.Set(Channel.Input, InputData.Throttle, s.rawThrottle);
                vehicle.data.Set(Channel.Input, InputData.Brake, s.rawBrake);
                vehicle.data.Set(Channel.Input, InputData.AutomaticGear, s.automaticGear);
            }
            else
            {
                vehicle.data.Set(Channel.Custom, Perrinn424Data.EnableProcessedInput, 1);
                vehicle.data.Set(Channel.Custom, Perrinn424Data.InputSteerAngle, (int)(s.steeringAngle * 10000.0f));
                vehicle.data.Set(Channel.Custom, Perrinn424Data.InputMguThrottle, (int)(s.throttle * 100.0f));
                vehicle.data.Set(Channel.Custom, Perrinn424Data.InputBrakePressure, (int)(s.brakePressure * 10000.0f));
                vehicle.data.Set(Channel.Custom, Perrinn424Data.InputGear, 1); //TODO
            }
        }

        public override float CalculateDuration()
        {
            return recordedLap.lapTime;
        }

        private float CalculateExpectedSpeed(Vector3 segment)
        {
            return segment.magnitude * recordedLap.frequency;
        }

        private void OnDrawGizmos()
        {
            if(debugDrawer != null)
                debugDrawer.Draw();
        }

        public override float Error => pidInfo.Error;
        public override float P => pidInfo.P;
        public override float I => pidInfo.I;
        public override float D => pidInfo.D;
        public override float PID => pidInfo.PID;
        public override float MaxForceP => pidInfo.MaxForceP;
        public override float MaxForceD => pidInfo.MaxForceD; //TODO remove MaxForceD

        public override bool IsStartup => startup.isStartUp;
    }
}
