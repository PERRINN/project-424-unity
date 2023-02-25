using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    public class NearestNeighborDebug : MonoBehaviour
    {
        NearestSegmentComposed searcher;
        public RecordedLap lap;
        public Vector3 projected;
        public float ratio;
        public float experimentalRatio;
        public PathDrawer pathDrawer;

        void Start()
        {
            Path path = new Path(lap);
            HeuristicNearestNeighbor nnSearcher = new HeuristicNearestNeighbor(path, 100, 100, 4);
            LocalCoordinatesProjector projector = new LocalCoordinatesProjector();
            searcher = new NearestSegmentComposed(nnSearcher, projector, path);
        }

        // Update is called once per frame
        void Update()
        {
            searcher.Search(this.transform);

            projected = RayProjection();

            experimentalRatio = (projected - searcher.Start).magnitude / searcher.Segment.magnitude;
            ratio = searcher.Ratio;

            pathDrawer.index = searcher.StartIndex;
        }

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
            Vector3 segment = searcher.Segment;
            //segment = b - a;
            Ray r1 = new Ray(searcher.Start, segment);
            //Ray r2 = new Ray(vehicle.transform.position + vehicle.cachedRigidbody.velocity*Time.deltaTime, vehicle.transform.right);
            Ray r2 = new Ray(this.transform.position, this.transform.right);
            Vector3 closest = GetClosestPointInRay1ToRay2(r1, r2);
            return closest;

            //dot = Vector3.Dot(segment, this.transform.position - a);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(searcher.Start, 0.3f);
            Gizmos.DrawSphere(searcher.End, 0.3f);
            Gizmos.DrawLine(this.transform.position, searcher.ProjectedPosition);
            Gizmos.DrawSphere(searcher.ProjectedPosition, 0.1f);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(this.transform.position, projected);
            Gizmos.DrawSphere(projected, 0.1f);
        }
    } 
}
