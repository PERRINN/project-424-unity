using UnityEngine;
using UnityEngine.Serialization;
using VehiclePhysics;
using VehiclePhysics.Timing;

namespace Perrinn424.AutopilotSystem
{
    public class Autopilot : BaseAutopilot
    {
        public RecordedLap recordedLap;

        private Path path;
        //private NearestSegmentSearcher segmentSearcher;
        private ExperimentalNNSegmentSearcher segmentSearcher;

        [FormerlySerializedAs("positionCorrector")]
        public PositionCorrector lateralCorrector;
        
        public PositionCorrector forwardCorrector;
        public TimeCorrector timeCorrector;

        public Sample nearestInterpolatedSample;
        public Sample runningSample;

        public LapTimer timer;



        public AutopilotStartup startup;

        public PathDrawer pathDrawer;

        public Vector3 targetPosition;

        public float offset = 0.9f;
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
            //segmentSearcher = new NearestSegmentSearcher(path);
            segmentSearcher = new ExperimentalNNSegmentSearcher(path);
            lateralCorrector.Init(vehicle.cachedRigidbody);
            forwardCorrector.Init(vehicle.cachedRigidbody);
            timeCorrector.Init(vehicle.cachedRigidbody);

            startup.Init(vehicle);

            vehicle.onBeforeUpdateBlocks += WriteInputs;
        }

        public override void OnDisableVehicle()
        {
            vehicle.onBeforeUpdateBlocks -= WriteInputs;
        }

        public void WriteInputs()
        {
            WriteInput(runningSample);
        }



        public override float PlayingTime()
        {
            float sampleIndex = segmentSearcher.StartIndex + segmentSearcher.Ratio;
            return (sampleIndex / recordedLap.frequency) - (offset/ vehicle.speed);
        }

        public override void FixedUpdateVehicle()
        {
            //segmentSearcher.Search(vehicle.transform.position);
            segmentSearcher.Search(vehicle.transform);
            nearestInterpolatedSample = GetInterpolatedNearestSample();
            targetPosition = nearestInterpolatedSample.position;

            if (!IsOn)
                return;

            float expectedSpeed = segmentSearcher.Segment.magnitude * recordedLap.frequency;

            //SteeringScreen.bestTime = Mathf.Lerp(segmentSearcher.StartIndex, segmentSearcher.EndIndex, segmentSearcher.Ratio) /recordedLap.frequency;
            pathDrawer.index = segmentSearcher.StartIndex;

            if (startup.IsStartup(expectedSpeed))
            {
                Sample startupSample = startup.Correct(nearestInterpolatedSample);
                runningSample = startupSample;
                //WriteInput(startupSample);
                print("Startup!");
            }
            else
            {
                targetPosition = RayProjection();
                targetPosition = segmentSearcher.ProjectedPosition;
                targetPosition = segmentSearcher.ExperimentalProjectedPosition;
                lateralCorrector.Correct(targetPosition); // why it doesn't work with nearestInterpolatedSample.position

                float currentTime = timer.currentLapTime;
                timeCorrector.Correct(PlayingTime(), currentTime);
                runningSample = nearestInterpolatedSample;
                //forwardCorrector.Correct(segmentSearcher.ProjectedPosition);
                //WriteInput(nearestInterpolatedSample);
            }

            //DebugGraph.Log("lateralError", lateralCorrector.Error);
            //DebugGraph.Log("forwardError", forwardCorrector.Error);
        }

        /// <summary>
        /// Distance Between Two Lines, defined as rays. Returns a position in ray 1 which is the closest point to ray 2
        /// </summary>
        /// <remarks>
        /// Mathematics for 3D Game Programming and Computer Graphics, Third Edition
        /// Eric Lengyel
        /// 5.1.2 Distance Between Two Lines
        /// </remarks>
        /// <returns>A point in ray 1 which is the closest position to ray 2</returns>
        public static Vector3 GetClosestPointInRay1ToRay2(Ray ray1, Ray ray2)
        {
            Vector3 pq = ray2.origin - ray1.origin;
            float scalarDir = Vector3.Dot(ray1.direction, ray2.direction);
            float t1Num = -Vector3.Dot(pq, ray1.direction) + scalarDir * Vector3.Dot(pq, ray2.direction);
            float t1Den = scalarDir * scalarDir - 1f;
            float t1 = t1Num / t1Den;
            return ray1.GetPoint(t1);
        }

        private Vector3 RayProjection()
        {


            Vector3 segment = segmentSearcher.Segment;
            //segment = b - a;
            Ray r1 = new Ray(segmentSearcher.Start, segment);
            //Ray r2 = new Ray(vehicle.transform.position + vehicle.cachedRigidbody.velocity*Time.deltaTime, vehicle.transform.right);
            Ray r2 = new Ray(vehicle.transform.position, vehicle.transform.right);
            Vector3 closest = GetClosestPointInRay1ToRay2(r1, r2);
            return closest;

            //dot = Vector3.Dot(segment, this.transform.position - a);
        }

        private Sample GetInterpolatedNearestSample()
        {
            Sample start = recordedLap[segmentSearcher.StartIndex];
            Sample end = recordedLap[segmentSearcher.EndIndex];
            //float t = segmentSearcher.Ratio;
            float t = segmentSearcher.ExperimentalRatio;
            //Sample interpolatedSample = Sample.Lerp(start, end, t);
            Sample interpolatedSample = Sample.LerpUncampled(start, end, t);
            
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
