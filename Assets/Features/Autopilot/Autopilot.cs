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

        [FormerlySerializedAs("offset")]
        public float positionOffset = 0.9f;


        private Path path;
        private INearestSegmentSearcher segmentSearcher;
        private HeuristicNearestNeighbor heuristicNN;
        private SectorSearcherNearestNeighbor sectorNN;

        private AutopilotDebugDrawer debugDrawer;

        public override void OnEnableVehicle()
        {

            CreateSearchers();
            lateralCorrector.Init(vehicle.cachedRigidbody);
            timeCorrector.Init(vehicle.cachedRigidbody);
            startup.Init(vehicle);
            debugDrawer = new AutopilotDebugDrawer();

            vehicle.onBeforeUpdateBlocks += UpdateAutopilot;
        }

        public override void OnDisableVehicle()
        {
            vehicle.onBeforeUpdateBlocks -= UpdateAutopilot;
        }

        private void CreateSearchers()
        {
            path = new Path(recordedLap);
            sectorNN = new SectorSearcherNearestNeighbor(path, 2, 10);
            int lookBehind = (int)(1f * recordedLap.frequency); //seconds to samples
            int lookAhead = (int)(2f * recordedLap.frequency); //seconds to samples
            heuristicNN = new HeuristicNearestNeighbor(path, lookBehind, lookAhead, 4);
            IProjector crossProductProjector = new CrossProductProjector();
            segmentSearcher = new NearestSegmentComposed(heuristicNN, crossProductProjector, path);
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

            float expectedSpeed = segmentSearcher.Segment.magnitude * recordedLap.frequency;
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
            sectorNN.Search(this.transform.position);
        }

        public override float PlayingTime()
        {
            float sampleIndex = IsOn ? (segmentSearcher.StartIndex + segmentSearcher.Ratio) : sectorNN.Index;
            float playingTimeBySampleIndex = sampleIndex / recordedLap.frequency;
            float offset = vehicle.speed > 10f ? positionOffset / vehicle.speed : 0f;
            return playingTimeBySampleIndex - offset;
        }

        protected override void SetStatus(bool isOn)
        {
            if (isOn)
            {
                heuristicNN.SetHeuristicIndex(sectorNN.Index);
            }

            base.SetStatus(isOn);
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

        public float CalculateDuration()
        {
            return recordedLap.lapTime;
        }

        private void OnDrawGizmos()
        {
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
