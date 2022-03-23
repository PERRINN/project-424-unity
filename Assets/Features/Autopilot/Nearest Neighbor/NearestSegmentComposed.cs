using Perrinn424.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    public class NearestSegmentComposed
    {

        private IReadOnlyList<Vector3> path;
        private HeuristicNearestNeighbor nnSearcher;
        private CircularIndex index;

        public NearestSegmentComposed(IReadOnlyList<Vector3> path)
        {
            this.path = path;
            nnSearcher = new HeuristicNearestNeighbor(path, 100, 100);
            index = new CircularIndex(0, path.Count);
        }

        public void Search(Transform t)
        {
            (int nn, _) = nnSearcher.Search(t.position);

            index.Assign(nn);
            Vector3 wayPoint = path[nn];

            Vector3 localWaypoint = t.transform.InverseTransformPoint(wayPoint);

            if (localWaypoint.z > 0)
                index--;

            StartIndex = index;
            EndIndex = index + 1;
            Start = path[StartIndex];
            End = path[EndIndex];
            Segment = End - Start;

            Vector3 endLocal = t.transform.InverseTransformPoint(End);
            Vector3 startLocal = t.transform.InverseTransformPoint(Start);
            Ratio = Mathf.InverseLerp(startLocal.z, endLocal.z, 0f);

            ProjectedPosition = Vector3.Lerp(Start, End, Ratio);

            //CalculateExperimentalProjectedPosition(t);
        }


        public int StartIndex { get; private set; }
        public int EndIndex { get; private set; }
        public Vector3 Start { get; private set; }
        public Vector3 End { get; private set; }
        public Vector3 Segment { get; private set; }
        public float Ratio { get; private set; }

        public Vector3 ProjectedPosition { get; set; }



        //private void CalculateExperimentalProjectedPosition(Transform t)
        //{
        //    ProjectedPosition = RayProjection(t.position, t.right);

        //    Ratio = (ProjectedPosition - Start).magnitude / Segment.magnitude;
        //}

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

        private Vector3 RayProjection(Vector3 position, Vector3 right)
        {
            Ray r1 = new Ray(Start, Segment);
            Ray r2 = new Ray(position, right);
            Vector3 closest = GetClosestPointInRay1ToRay2(r1, r2);
            return closest;
        }

    } 
}
