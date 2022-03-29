using Perrinn424.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    public class BruteForceSearcher : INearestNeighbourSearcher
    {
        private readonly IReadOnlyList<Vector3> path;

        public BruteForceSearcher(IReadOnlyList<Vector3> path)
        {
            this.path = path;
        }

        public void Search(Vector3 position)
        {
            (int index, float distance) = SearchInBoundaries(position);
            Index = index;
            Distance = distance;
            Position = path[Index];
        }

        public (int, float) SearchInBoundaries(Vector3 position)
        {
            return SearchInBoundaries(position, 0, path.Count, 1);
        }

        public (int, float) SearchInBoundaries(Vector3 position, int from, int count, int step)
        {
            float sqrtDistance = Mathf.Infinity;

            CircularIndex circularIndex = new CircularIndex(path.Count);
            int closestIndex = -1;
            for (int i = 0; i < count; i = i + step)
            {
                circularIndex.Assign(from + i);
                float tempDistance = (position - path[circularIndex]).sqrMagnitude;
                if (tempDistance < sqrtDistance)
                {
                    sqrtDistance = tempDistance;
                    closestIndex = circularIndex;
                }
            }

            return (closestIndex, Mathf.Sqrt(sqrtDistance));
        }


        public int Index { get; private set; }

        public float Distance { get; private set; }

        public Vector3 Position { get; private set; }
    } 
}
