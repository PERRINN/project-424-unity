using Perrinn424.Utilities;
using System.Collections.Generic;
using UnityEngine;
using VehiclePhysics;

namespace Perrinn424.AutopilotSystem
{
    /// <summary>
    /// Search the closest frame to a position with the heuristic that the closest frame should be placed around the last found frame
    /// This heuristic works in a path following algorithm, with the frames sorted as a path
    /// </summary>
    public class HeuristicFrameSearcher : IFrameSearcher
    {
        public int ClosestFrame1 { get; private set; }
        public int ClosestFrame2 { get; private set; }
        public float ClosestDisFrame1 { get; private set; }
        public float ClosestDisFrame2 { get; private set; }

        private bool hasHeuristics;
        IReadOnlyList<VPReplay.Frame> frames;

        private readonly float distanceThreshold;
        private readonly int lookBehind;
        private readonly int lookCount;
        public HeuristicFrameSearcher(IReadOnlyList<VPReplay.Frame> frames, float distanceThreshold, int lookBehind, int lookAhead)
        {
            this.frames = frames;
            this.distanceThreshold = distanceThreshold;
            this.lookBehind = lookBehind;
            this.lookCount = lookAhead - lookBehind;
        }

        public void Search(Transform t)
        {
            int start = 0;
            int count = frames.Count;

            if (hasHeuristics)
            {
                start = new CircularIndex(ClosestFrame1 - lookBehind, frames.Count);
                count = lookCount;
            }

            int index = FindClosest(start, count, t.position);

            Vector3 closestPosition = frames[index].position;
            CircularIndex i = new CircularIndex(index, frames.Count);

            float localZ = t.InverseTransformPoint(closestPosition).z; // if the point is ahead, the segment is [closest-1, closest]
            if (localZ > 0) i--;

            ClosestFrame1 = i;
            ClosestFrame2 = i + 1;
            ClosestDisFrame1 = Mathf.Sqrt(Squared2DDistance(frames[ClosestFrame1].position, t.position));
            ClosestDisFrame2 = Mathf.Sqrt(Squared2DDistance(frames[ClosestFrame2].position, t.position));

            if (ClosestDisFrame1 > distanceThreshold && hasHeuristics == true) // not good enough. Search again with recursion without heuristics
            {
                hasHeuristics = false;
                Search(t);
            }
            else
            {
                hasHeuristics = true;
            }
        }

        private int FindClosest(int start, int count, Vector3 pos)
        {
            int closestIndex = -1;
            float minDistance = float.PositiveInfinity;

            for (int index = start; index < (start + count); index++)
            {
                int circularIndex = CircularIndex.FitCircular(index, frames.Count);
                Vector3 checkPosition = frames[circularIndex].position;
                float dist = Squared2DDistance(checkPosition, pos);
                if (dist < minDistance)
                {
                    closestIndex = circularIndex;
                    minDistance = dist;
                }
            }

            return closestIndex;
        }

        private float Squared2DDistance(Vector3 a, Vector3 b)
        {
            float xDiff = (a.x - b.x);
            float zDiff = (a.z - b.z);
            return xDiff * xDiff + zDiff * zDiff;
        }
    } 
}
