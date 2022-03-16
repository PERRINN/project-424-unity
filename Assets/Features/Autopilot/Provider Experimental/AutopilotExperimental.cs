using UnityEngine;
using UnityEngine.Serialization;
using VehiclePhysics;
using VehiclePhysics.UI;

namespace Perrinn424.AutopilotSystem
{
    public class AutopilotExperimental : VehicleBehaviour, IPIDInfo
    {
        public RecordedLap recordedLap;

        private Path path;
        private NearestSegmentSearcher segmentSearcher;
        
        [FormerlySerializedAs("positionCorrector")]
        public PositionCorrector lateralCorrector;
        
        public PositionCorrector forwardCorrector;

        public Sample nearestInterpolatedSample;





        public AutopilotStartup startup;

        public float Error => lateralCorrector.Error;

        public float P => lateralCorrector.PID.proportional;

        public float I => lateralCorrector.PID.integral;

        public float D => lateralCorrector.PID.derivative;

        public float PID => lateralCorrector.PID.output;

        public float MaxForceP => lateralCorrector.max;

        public float MaxForceD => lateralCorrector.max; //TODO remove MaxForceD

        public PathDrawer pathDrawer;

        public float CalculateDuration()
        {
            return recordedLap.Count / recordedLap.frequency;
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
            forwardCorrector.Init(vehicle.cachedRigidbody);

            startup.Init(vehicle);
        }




        public override void FixedUpdateVehicle()
        {
            segmentSearcher.Search(vehicle.transform.position);
            nearestInterpolatedSample = GetInterpolatedNearestSample();

            float expectedSpeed = segmentSearcher.Segment.magnitude * recordedLap.frequency;

            SteeringScreen.bestTime = Mathf.Lerp(segmentSearcher.StartIndex, segmentSearcher.EndIndex, segmentSearcher.Ratio) /recordedLap.frequency;
            pathDrawer.index = segmentSearcher.StartIndex;

            if (startup.IsStartup(expectedSpeed))
            {
                Sample startupSample = startup.Correct(nearestInterpolatedSample);
                WriteInput(startupSample);
                print("Startup!");
            }
            else
            {
                lateralCorrector.Correct(segmentSearcher.ProjectedPosition); // why it doesn't work with nearestInterpolatedSample.position
                //forwardCorrector.Correct(segmentSearcher.ProjectedPosition);
                WriteInput(nearestInterpolatedSample);
            }

            //DebugGraph.Log("lateralError", lateralCorrector.Error);
            //DebugGraph.Log("forwardError", forwardCorrector.Error);
        }

        private Vector3 Pos()
        {
            
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
            //Gizmos.DrawSphere(nearestInterpolatedSample.position, 0.1f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(segmentSearcher.ProjectedPosition, 0.05f);
            Gizmos.DrawRay(lateralCorrector.ApplicationPosition, lateralCorrector.Force);
        }
    }
}
