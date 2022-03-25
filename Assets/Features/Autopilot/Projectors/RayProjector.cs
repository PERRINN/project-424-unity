using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    public class RayProjector : IProjector
    {
        public (Vector3, float) Project(Transform t, Vector3 start, Vector3 end)
        {
            Vector3 segment = end - start;

            Ray segmentRay = new Ray(start, segment);
            Ray transformRightRay = new Ray(t.position, t.right);
            Vector3 projectedPosition = GetClosestPointInRay1ToRay2(segmentRay, transformRightRay);
            float ratio = (projectedPosition - start).magnitude / segment.magnitude;

            return (projectedPosition, ratio);
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
        private static Vector3 GetClosestPointInRay1ToRay2(Ray ray1, Ray ray2)
        {
            Vector3 pq = ray2.origin - ray1.origin;
            float scalarDir = Vector3.Dot(ray1.direction, ray2.direction);
            float t1Num = -Vector3.Dot(pq, ray1.direction) + scalarDir * Vector3.Dot(pq, ray2.direction);
            float t1Den = scalarDir * scalarDir - 1f;
            float t1 = t1Num / t1Den;
            return ray1.GetPoint(t1);
        }
    } 
}
