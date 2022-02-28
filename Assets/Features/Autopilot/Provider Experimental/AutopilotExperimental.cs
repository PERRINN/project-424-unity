using UnityEngine;
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

        public override int GetUpdateOrder()
        {
            return 10;
        }

        public override void OnEnableVehicle()
        {
            path = new Path(recordedLap);
            segmentSearcher = new NearestSegmentSearcher(path);
            corrector = new PathErrorCorrector(vehicle.cachedRigidbody);
            //vehicle.onBeforeUpdateBlocks += WriteInput;
        }

        public override void FixedUpdateVehicle()
        {
            segmentSearcher.Search(vehicle.transform.position);
            nearestInterpolatedSample = GetInterpolatedNearestSample();
            corrector.SetPIDParameters(kp, ki, kd);
            //print($"{segmentSearcher.ProjectedPosition}  ---   {nearestInterpolatedSample.position}");
            DebugPosition();
            corrector.Correct(segmentSearcher.ProjectedPosition); // why it doesn't work with nearestInterpolatedSample.position
            //corrector.Correct(nearestInterpolatedSample.position); // why it doesn't work with nearestInterpolatedSample.position
            WriteInput();
        }

        private Sample GetInterpolatedNearestSample()
        {
            Sample start = recordedLap[segmentSearcher.StartIndex];
            Sample end = recordedLap[segmentSearcher.EndIndex];
            float t = segmentSearcher.Ratio;
            Sample interpolatedSample = Sample.Lerp(start, end, t);
            
            return interpolatedSample;
        }

        private void DebugPosition()
        {
            string format = "<color={5}>source:{0} x:{1:F6} y:{2:F6} z:{3:F6} ratio : {4:F6}</color>";
            Vector3 interpolated = nearestInterpolatedSample.position;
            Vector3 projected = segmentSearcher.ProjectedPosition;
            Vector3 diff = interpolated - projected;
            //string output = string.Format(format, "interpolated", interpolated.x, interpolated.y, interpolated.z);
            //output  = output + " " + string.Format(format, "projected", projected.x, projected.y, projected.z);

            string color;
            if (Mathf.Approximately(diff.x, 0f) || Mathf.Approximately(diff.y, 0f) || Mathf.Approximately(diff.z, 0f))
            {
                color = "white";
            }
            else
            {
                color = "red";
            }
            string output = string.Format(format, "projected", diff.x, diff.y, diff.z, segmentSearcher.Ratio, color);
            print(output);
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
