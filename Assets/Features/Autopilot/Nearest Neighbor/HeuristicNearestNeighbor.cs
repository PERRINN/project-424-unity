using System;
using System.Collections.Generic;
using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    public class HeuristicNearestNeighbor : INearestNeighbourSearcher
    {
        private readonly IReadOnlyList<Vector3> path;
        private int behind;
        private int ahead;
        private readonly float tolerance = 4;

        private BruteForceSearcher bruteForceSearcher;

        public HeuristicNearestNeighbor(IReadOnlyList<Vector3> path, int behind, int ahead, float tolerance)
        {
            this.path = path;
            this.behind = behind;
            this.ahead = ahead;
            this.tolerance = tolerance;

            bruteForceSearcher = new BruteForceSearcher(path);
        }

        public int Index { get; private set; }

        public float Distance { get; private set; }

        public Vector3 Position { get; private set; }

        public void Search(Vector3 position)
        {
            (int nn, float distance) = bruteForceSearcher.SearchInBoundaries(position, Index - behind, behind + ahead, 1);

            if (distance > tolerance)
            {
                throw new InvalidOperationException($"Waypoint closest to {tolerance} meters not found");
            }

            Index = nn;
            Distance = distance;
            Position = path[Index];
        }

        public void SetHeuristicIndex(int heuristicIndex)
        {
            Index = heuristicIndex;
        }
    } 
}
