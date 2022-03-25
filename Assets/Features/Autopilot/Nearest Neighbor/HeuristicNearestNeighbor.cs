using System.Collections.Generic;
using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    public class HeuristicNearestNeighbor : INearestNeighbourSearcher
    {
        private readonly IReadOnlyList<Vector3> path;
        private int behind;
        private int ahead;
        private int lastIndex;
        private float lastDistance;
        private float tolerance = 4;

        private BruteForceSearcher bruteForceSearcher;

        public HeuristicNearestNeighbor(IReadOnlyList<Vector3> path, int behind, int ahead)
        {
            this.path = path;
            this.behind = behind;
            this.ahead = ahead;

            bruteForceSearcher = new BruteForceSearcher(path);
        }

        public (int, float) Search(Vector3 position)
        {
            (int nn, float distance) = bruteForceSearcher.Search(position, lastIndex - behind, behind + ahead, 1);

            if (distance > tolerance)
            {

                (nn, distance) = bruteForceSearcher.Search(position);
            }

            lastIndex = nn;
            lastDistance = distance;
            return (nn, distance);
        }
    } 
}
