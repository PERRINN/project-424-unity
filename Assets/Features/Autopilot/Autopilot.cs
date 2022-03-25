using UnityEngine;
using UnityEngine.Serialization;
using VehiclePhysics;
using VehiclePhysics.Timing;

namespace Perrinn424.AutopilotSystem
{
    public class Autopilot : BaseAutopilot
    {
        [Header("References")]
        [SerializeField]
        private RecordedLap recordedLap;

        [SerializeField]
        private LapTimer timer;

        [SerializeField]
        private PathDrawer pathDrawer;

        [Header("Setup")]

        [SerializeField]
        private AutopilotStartup startup;

        [FormerlySerializedAs("positionCorrector")]
        public PositionCorrector lateralCorrector;

        [SerializeField]
        private TimeCorrector timeCorrector;

        public float offset = 0.9f;



        private Path path;
        private NearestSegmentSearcher segmentSearcher;

        private Sample interpolatedSample;
        private Sample runningSample;
        private Vector3 targetPosition;

        public float CalculateDuration()
        {
            return recordedLap.lapTime;
        }

        public override int GetUpdateOrder()
        {
            return 10;
        }


        public override void OnEnableVehicle()
        {
            path = new Path(recordedLap);
            segmentSearcher = new NearestSegmentSearcher(path);
            lateralCorrector.Init(vehicle.cachedRigidbody);
            timeCorrector.Init(vehicle.cachedRigidbody);

            startup.Init(vehicle);

            vehicle.onBeforeUpdateBlocks += UpdateAutopilot;
        }

        public override void OnDisableVehicle()
        {
            vehicle.onBeforeUpdateBlocks -= UpdateAutopilot;
        }

        public override float PlayingTime()
        {
            float sampleIndex = segmentSearcher.StartIndex + segmentSearcher.Ratio;
            return (sampleIndex / recordedLap.frequency) - (offset/ vehicle.speed);
        }

        public void UpdateAutopilot()
        {
            segmentSearcher.Search(vehicle.transform.position);
            interpolatedSample = GetInterpolatedNearestSample();
            targetPosition = interpolatedSample.position;

            if (!IsOn)
            {
                return;
            }

            float expectedSpeed = segmentSearcher.Segment.magnitude * recordedLap.frequency;

            pathDrawer.index = segmentSearcher.StartIndex;

            if (startup.IsStartup(expectedSpeed))
            {
                Sample startupSample = startup.Correct(interpolatedSample);
                runningSample = startupSample;
            }
            else
            {
                targetPosition = segmentSearcher.ProjectedPosition;
                lateralCorrector.Correct(targetPosition);

                float currentTime = timer.currentLapTime;
                timeCorrector.Correct(PlayingTime(), currentTime);
                runningSample = interpolatedSample;
            }

            WriteInput(runningSample);
        }

        private Sample GetInterpolatedNearestSample()
        {
            Sample start = recordedLap[segmentSearcher.StartIndex];
            Sample end = recordedLap[segmentSearcher.EndIndex];
            float t = segmentSearcher.Ratio;
            Sample interpolatedSample = Sample.LerpUncampled(start, end, t);
            
            return interpolatedSample;
        }

        private void WriteInput(Sample s)
        {
            vehicle.data.Set(Channel.Input, InputData.Steer, s.rawSteer);
            vehicle.data.Set(Channel.Input, InputData.Throttle, s.rawThrottle);
            vehicle.data.Set(Channel.Input, InputData.Brake, s.rawBrake);
            vehicle.data.Set(Channel.Input, InputData.AutomaticGear, s.automaticGear);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(targetPosition, 0.05f*10);
            Gizmos.DrawRay(lateralCorrector.ApplicationPosition, lateralCorrector.Force);
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
