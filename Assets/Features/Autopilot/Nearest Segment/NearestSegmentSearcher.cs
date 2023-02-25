using Perrinn424.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    public class NearestSegmentSearcher : INearestSegmentSearcher
    {

        private readonly IReadOnlyList<Vector3> path;
        private CircularIndex index;
        public NearestSegmentSearcher(IReadOnlyList<Vector3> path)
        {
            this.path = path;
            index = new CircularIndex(path.Count);
        }

        public int StartIndex { get; private set; }
        public int EndIndex { get; private set; }
        public Vector3 Start { get; private set; }
        public Vector3 End { get; private set; }
        public Vector3 Segment { get; private set; }
        public float Ratio { get; private set; }

        public float Distance { get; private set; }

        public Vector3 ProjectedPosition { get; set; }
        public float RejectionDistance { get; set; }

        public int Operations { get; private set; }

        public void Search(Transform t)
        {

            SearchNearest(t.position);
            UpdateValues(t.position);
        }

        private void SearchNearest(Vector3 position)
        {
            Operations = 0;


            if (Belong(Start, End, position))
            {
                return;
            }
            else if (Belong(path[index + 1], path[index + 2], position))
            {
                index++;
                return;
            }
            else if (Belong(path[index - 1], path[index], position))
            {
                index--;
                return;
            }

            Debug.Log("Search!");
            Debug.Break();

            int cacheIndx = index;
            float minDistance = float.PositiveInfinity;
            for (int i = 0; i < path.Count; i++)
            {
                Operations++;
                var sqrtDistance = Squared2DDistance(path[i], position);
                if (sqrtDistance < minDistance)
                {
                    minDistance = sqrtDistance;
                    index.Assign(i);
                }
            }

            if (!Belong(path[index], path[index + 1], position))
            {
                index--;
            }

            Debug.Log($"Found. It was {cacheIndx} and now is {index}");

        }

        private bool Belong(Vector3 start, Vector3 end, Vector3 p)
        {
            Vector3 segment = end - start;
            Vector3 pointVector = p - start;

            Vector3 crossProduct = Vector3.Cross(segment.normalized, pointVector.normalized);
            float dotProduct_point = Vector3.Dot(segment, pointVector);
            float dotProduct_segment = Vector3.Dot(segment, segment);

            //bool belong = dotProduct_point >= 0f && dotProduct_point <= dotProduct_segment;
            float ratio = dotProduct_point / segment.sqrMagnitude; //TODO ratio = dotProduct_point/segment.sqrtdistance

            bool belong = (ratio > 0 || Approx(ratio, 0)) && (ratio < 1.0f || Approx(ratio, 1.0f));

            if ((Approx(ratio, 0f) || Approx(ratio, 1f)) && !belong)
            {
                Debug.Log($"Error! {ratio}");
            }
            return belong;
        }

        private bool Approx(float a, float b, float error = 1e-2f)
        {
            return Mathf.Abs(a - b) <= error;
        }

        private void UpdateValues(Vector3 p)
        {
            StartIndex = index;
            EndIndex = index + 1;
            Start = path[StartIndex];
            End = path[EndIndex];
            Segment = End - Start;

            Vector3 pointVector = p - Start;

            float dotProduct_point = Vector3.Dot(Segment, pointVector);
            float dotProduct_segment = Vector3.Dot(Segment, Segment);

            Ratio = dotProduct_point / dotProduct_segment;
            ProjectedPosition = Start + Segment * Ratio;
            RejectionDistance = Vector3.Distance(p, ProjectedPosition);
            Distance = Vector3.Distance(Start, p);
        }

        private float Squared2DDistance(Vector3 a, Vector3 b)
        {
            float xDiff = (a.x - b.x);
            float zDiff = (a.z - b.z);
            return xDiff * xDiff + zDiff * zDiff;
        }
    } 
}
