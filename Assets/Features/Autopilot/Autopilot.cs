using Perrinn424.Utilities;
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


        private Path path;
        private INearestSegmentSearcher segmentSearcher;
        private HeuristicNearestNeighbor heuristicNN;
        private AutopilotOffModeSearcher autopilotOffModeSearcher;

        private AutopilotDebugDrawer debugDrawer;

        public override void OnEnableVehicle()
        {

            CreateSearchers();
            lateralCorrector.Init(vehicle.cachedRigidbody);
            timeCorrector.Init(vehicle.cachedRigidbody);
            startup.Init(vehicle);
            debugDrawer = new AutopilotDebugDrawer();
            pathDrawer.recordedLap = recordedLap;

            vehicle.onBeforeUpdateBlocks += UpdateAutopilot;
        }

        public override void OnDisableVehicle()
        {
            vehicle.onBeforeUpdateBlocks -= UpdateAutopilot;
        }

        private void CreateSearchers()
        {
            path = new Path(recordedLap);
            autopilotOffModeSearcher = new AutopilotOffModeSearcher(path);
            int lookBehind = (int)(1f * recordedLap.frequency); //seconds to samples
            int lookAhead = (int)(2f * recordedLap.frequency); //seconds to samples
            heuristicNN = new HeuristicNearestNeighbor(path, lookBehind, lookAhead, 4);
            IProjector projector = new CrossProductProjector();
            segmentSearcher = new NearestSegmentComposed(heuristicNN, projector, path);
        }

        public void UpdateAutopilot()
        {
            if (IsOn)
            {
                UpdateAutopilotInOnStatus();
            }
            else
            {
                UpdateAutopilotInOffStatus();
            }
        }

        private void UpdateAutopilotInOnStatus()
        {
            segmentSearcher.Search(vehicle.transform);
            pathDrawer.index = segmentSearcher.StartIndex;
            Sample runningSample = GetInterpolatedNearestSample();
            Vector3 targetPosition = segmentSearcher.ProjectedPosition;

            float expectedSpeed = CalculateExpectedSpeed(segmentSearcher.Segment);
            if (startup.IsStartup(expectedSpeed)) //startup block
            {
                runningSample = startup.Correct(runningSample);
            }
            else //main block
            {
                lateralCorrector.Correct(targetPosition);
                float currentTime = timer.currentLapTime;
                timeCorrector.Correct(PlayingTime(), currentTime);
            }

            debugDrawer.Set(targetPosition, lateralCorrector.ApplicationPosition, lateralCorrector.Force);
            WriteInput(runningSample);
        }

        private void UpdateAutopilotInOffStatus()
        {
            autopilotOffModeSearcher.Search(this.transform.position);
        }

        public override float PlayingTime()
        {
            float sampleIndex = IsOn ? (segmentSearcher.StartIndex + segmentSearcher.Ratio) : autopilotOffModeSearcher.Index;
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


                heuristicNN.SetHeuristicIndex(autopilotOffModeSearcher.Index);
            }

            base.SetStatus(isOn);
        }

        private bool CanOperate()
        {

            CircularIndex index = new CircularIndex(autopilotOffModeSearcher.Index, recordedLap.Count);
            Vector3 segment = path[index + 1] - path[index];

            float expectedSpeed = CalculateExpectedSpeed(segment);
            Quaternion pathRotation = recordedLap.samples[index].rotation;
            float yawError = RotationCorrector.YawError(vehicle.transform.rotation, pathRotation);
            float distance = heuristicNN.Distance;

            if (!startup.isStartUp && (Mathf.Abs(yawError) > 10f || distance > 2f || vehicle.speed < expectedSpeed*0.9f))
            {
                return false;
            }

            return true;
        }

        private Sample GetInterpolatedNearestSample()
        {
            Sample start = recordedLap[segmentSearcher.StartIndex];
            Sample end = recordedLap[segmentSearcher.EndIndex];
            float t = segmentSearcher.Ratio;
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

        public float CalculateDuration()
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

        public override float Error => lateralCorrector.Error;
        public override float P => lateralCorrector.PID.proportional;
        public override float I => lateralCorrector.PID.integral;
        public override float D => lateralCorrector.PID.derivative;
        public override float PID => lateralCorrector.PID.output;
        public override float MaxForceP => lateralCorrector.max;
        public override float MaxForceD => lateralCorrector.max; //TODO remove MaxForceD
    }
}
