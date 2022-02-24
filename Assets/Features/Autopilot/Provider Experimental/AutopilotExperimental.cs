using VehiclePhysics;

namespace Perrinn424.AutopilotSystem
{
    public class AutopilotExperimental : VehicleBehaviour
    {

        public float kp;
        public float ki;
        public float kd;
        public RecordedLap recordedLap;

        private Path path;
        private NearestSegmentSearcher segmentSearcher;
        public PathErrorCorrector corrector;

        public Sample nearestInterpolatedSample;

        public override void OnEnableVehicle()
        {
            path = new Path(recordedLap);
            segmentSearcher = new NearestSegmentSearcher(path);
            corrector = new PathErrorCorrector(vehicle.cachedRigidbody);
            vehicle.onBeforeUpdateBlocks += WriteInput;
        }

        public override void FixedUpdateVehicle()
        {
            nearestInterpolatedSample = GetInterpolatedNearestSample();
            corrector.SetPIDParameters(kp, ki, kd);
            corrector.Correct(nearestInterpolatedSample.position);
        }

        private Sample GetInterpolatedNearestSample()
        {
            segmentSearcher.Search(vehicle.transform.position);
            
            Sample start = recordedLap[segmentSearcher.StartIndex];
            Sample end = recordedLap[segmentSearcher.EndIndex];
            float t = segmentSearcher.Ratio;
            Sample interpolatedSample = Sample.Lerp(start, end, t);
            
            return interpolatedSample;
        }

        private void WriteInput()
        {
            vehicle.data.Set(Channel.Input, InputData.Steer, nearestInterpolatedSample.rawSteer);
            vehicle.data.Set(Channel.Input, InputData.Throttle, nearestInterpolatedSample.rawThrottle);
            vehicle.data.Set(Channel.Input, InputData.Brake, nearestInterpolatedSample.rawBrake);
            vehicle.data.Set(Channel.Input, InputData.AutomaticGear, nearestInterpolatedSample.automaticGear);
        }
    }
}
