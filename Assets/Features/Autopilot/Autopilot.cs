using UnityEngine;
using UnityEngine.Serialization;
using VehiclePhysics;
using VehiclePhysics.Timing;

namespace Perrinn424.AutopilotSystem
{
    public class Autopilot : BaseAutopilot
    {

        [Header("References")]
        public RecordedLap recordedLap;

        [SerializeField]
        private LapTimer timer;

        [SerializeField]
        private PathDrawer pathDrawer;

        [Header("Setup")]

        [SerializeField]
        private bool autoStart = false;

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
        private IPIDInfo PIDInfo => lateralCorrector;

        public Sample ReferenceSample { get; private set; }
        public float ReferenceSpeed { get; private set; }
        public override float PlayingTime => playingTime;
        private float playingTime;

        public override float DeltaTime => deltaTime;
        private float deltaTime;

        public override float Duration => recordedLap.lapTime;
        public override long Timestamp => recordedLap.timestamp;

        public override void OnEnableVehicle()
        {
            autopilotSearcher = new AutopilotSearcher(this, recordedLap);
            lateralCorrector.Init(vehicle.cachedRigidbody);
            timeCorrector.Init(vehicle.cachedRigidbody);
            startup.Init(vehicle);
            debugDrawer = new AutopilotDebugDrawer();
            pathDrawer.recordedLap = recordedLap;

            vehicle.onBeforeUpdateBlocks += UpdateAutopilot;
            UpdateAutopilot();

            if (autoStart)
            {
                SetStatus(true);
            }
        }

        public override void OnDisableVehicle()
        {
            vehicle.onBeforeUpdateBlocks -= UpdateAutopilot;
        }

        public void UpdateAutopilot()
        {
            autopilotSearcher.Search(vehicle.transform);
            ReferenceSample = GetInterpolatedNearestSample();
            ReferenceSpeed = ReferenceSample.speed;
            playingTime = CalculatePlayingTime();
            deltaTime = timer.currentLapTime - PlayingTime;
            pathDrawer.index = autopilotSearcher.StartIndex;

            if (IsOn)
            {
                UpdateAutopilotInOnStatus();
            }
        }

        private void UpdateAutopilotInOnStatus()
        {
            startup.IsStartup(ReferenceSpeed);
            Sample runningSample = ReferenceSample;
            Vector3 targetPosition = autopilotSearcher.ProjectedPosition;

            float yawError = RotationCorrector.YawError(vehicle.transform.rotation, runningSample.rotation);

            if (yawError > 90f)
            {
                SetStatus(false);
                return;
            }

            if (IsStartup) //startup block
            {
                lateralCorrector.Correct(targetPosition);
                runningSample = startup.Correct(runningSample);
            }
            else //main block
            {
                lateralCorrector.Correct(targetPosition);
                float currentTime = timer.currentLapTime;
                timeCorrector.Correct(CalculatePlayingTime(), currentTime);
            }

            debugDrawer.Set(targetPosition, lateralCorrector.ApplicationPosition, lateralCorrector.Force);
            WriteInput(runningSample);

        }

        private float CalculatePlayingTime()
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
            else
            {
                vehicle.data.Set(Channel.Custom, Perrinn424Data.EnableProcessedInput, 0);
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
            vehicle.data.Set(Channel.Custom, Perrinn424Data.EnableProcessedInput, 1);
            vehicle.data.Set(Channel.Custom, Perrinn424Data.InputDrsPosition, (int)(s.drsPosition * 10.0f));
            vehicle.data.Set(Channel.Custom, Perrinn424Data.InputSteerAngle, (int)(s.steeringAngle * 10000.0f));
            vehicle.data.Set(Channel.Custom, Perrinn424Data.InputThrottlePosition, (int)(s.throttle * 100.0f));
            vehicle.data.Set(Channel.Custom, Perrinn424Data.InputBrakePosition, (int)(s.brake * 100.0f));
            vehicle.data.Set(Channel.Custom, Perrinn424Data.InputGear, s.gear); //TODO
            vehicle.data.Set(Channel.Custom, Perrinn424Data.InputLiftAndCoast, s.liftAndCoast);
        }

        private void OnDrawGizmos()
        {
            if(debugDrawer != null)
                debugDrawer.Draw();
        }

        public override float Error => PIDInfo.Error;
        public override float P => PIDInfo.P;
        public override float I => PIDInfo.I;
        public override float D => PIDInfo.D;
        public override float PID => PIDInfo.PID;
        public override float MaxForceP => PIDInfo.MaxForceP;
        public override float MaxForceD => PIDInfo.MaxForceD; //TODO remove MaxForceD

        public override bool IsStartup => startup.isStartUp;
    }
}
