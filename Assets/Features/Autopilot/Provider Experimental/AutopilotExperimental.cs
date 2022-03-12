using EdyCommonTools;
using UnityEngine;
using VehiclePhysics;
using VehiclePhysics.UI;

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
        public PositionCorrector positionCorrector;

        public Sample nearestInterpolatedSample;

        [Header("startup")]
        public float steerKp;
        public float steerKd;
        public float mult;

        public PidController steerPID;

        public Vector3 localForce;

        public AutopilotStartup startup;

        public override int GetUpdateOrder()
        {
            return 10;
        }


        public override void OnEnableVehicle()
        {
            path = new Path(recordedLap);
            segmentSearcher = new NearestSegmentSearcher(path);
            positionCorrector.Init(vehicle.cachedRigidbody);
            steerPID = new PidController();

            startup.Init(vehicle);
            //vehicle.onBeforeUpdateBlocks += WriteInput;
        }

        //public override void FixedUpdateVehicle()
        //{
        //    segmentSearcher.Search(vehicle.transform.position);
        //    nearestInterpolatedSample = GetInterpolatedNearestSample();
        //    corrector.SetPIDParameters(kp, ki, kd);
        //    //print($"{segmentSearcher.ProjectedPosition}  ---   {nearestInterpolatedSample.position}");
        //    DebugPosition();
        //    corrector.Correct(segmentSearcher.ProjectedPosition); // why it doesn't work with nearestInterpolatedSample.position
        //    //corrector.Correct(nearestInterpolatedSample.position); // why it doesn't work with nearestInterpolatedSample.position
        //    WriteInput();
        //}


        public override void FixedUpdateVehicle()
        {
            segmentSearcher.Search(vehicle.transform.position);
            nearestInterpolatedSample = GetInterpolatedNearestSample();

            float expectedSpeed = segmentSearcher.Segment.magnitude * recordedLap.frequency;

            SteeringScreen.bestTime = Mathf.Lerp(segmentSearcher.StartIndex, segmentSearcher.EndIndex, segmentSearcher.Ratio) /recordedLap.frequency;

            if (startup.IsStartup(expectedSpeed))
            {
                Sample startupSample = startup.Correct(nearestInterpolatedSample);
                WriteInput(startupSample);
                print("Startup!");
            }

            //if (vehicle.speed < expectedSpeed * 0.4f)
            //{
            //    Vector3 localPos = vehicle.transform.InverseTransformPoint(segmentSearcher.ProjectedPosition);
            //    steerPID.SetParameters(steerKp, 0f, steerKd);
            //    steerPID.input = -localPos.x;
            //    steerPID.Compute();
            //    //print(steerPID.input);

            //    Sample startupSample = nearestInterpolatedSample;
            //    startupSample.rawBrake = 0;
            //    startupSample.rawThrottle = 7000;
            //    //startupSample.rawSteer = (int)steerPID.output;

            //    float angleDiff = Quaternion.Angle(vehicle.transform.rotation, startupSample.rotation);
            //    print(angleDiff);
            //    Vector3 localDiff = vehicle.transform.InverseTransformPoint(segmentSearcher.ProjectedPosition);
            //    //float localX = Mathf.Clamp(mult*localDiff.x, -10000f, 10000f);
            //    float localX = Mathf.Clamp(mult* angleDiff, -10000f, 10000f);
            //    localForce = Vector3.right * localX;
            //    vehicle.cachedRigidbody.AddForceAtPosition(vehicle.transform.TransformDirection(localForce), vehicle.transform.position);
            //    DebugGraph.Log("Steer", startupSample.rawSteer);
            //    DebugGraph.Log("Force", localForce);
            //    WriteInput(startupSample);
            //    DebugGraph.Log("startup", false);

            //    //print());

            //}
            else
            {
                positionCorrector.Correct(segmentSearcher.ProjectedPosition); // why it doesn't work with nearestInterpolatedSample.position
                WriteInput(nearestInterpolatedSample);
                DebugGraph.Log("startup", true);

            }
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
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(nearestInterpolatedSample.position, 0.3f);
            Gizmos.DrawRay(vehicle.transform.position, vehicle.transform.TransformDirection(localForce));
        }
    }
}
