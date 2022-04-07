using Perrinn424.Utilities;
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
        private CrossProductProjector projector;
        private CircularIndex circularIndex;

        public HeuristicNearestNeighbor(IReadOnlyList<Vector3> path, int behind, int ahead, float tolerance)
        {
            this.path = path;
            this.behind = behind;
            this.ahead = ahead;
            this.tolerance = tolerance;

            bruteForceSearcher = new BruteForceSearcher(path);
            projector = new CrossProductProjector();
            circularIndex = new CircularIndex(path.Count);
        }

        public int Index { get; private set; }

        public float Distance { get; private set; }

        public Vector3 Position { get; private set; }

        public void Search(Vector3 position)
        {
            if (TryCurrentIndex(position))
            {
                return;
            }

            (int nn, float distance) = bruteForceSearcher.SearchInBoundaries(position, Index - behind, behind + ahead, 1);

            Index = nn;
            Distance = distance;
            Position = path[Index];

            if (distance > tolerance)
            {
                throw new InvalidOperationException($"Waypoint closest to {tolerance} meters not found");
            }
        }

        public void SetHeuristicIndex(int heuristicIndex)
        {
            Index = heuristicIndex;
        }

        private bool TryCurrentIndex(Vector3 position)
        {
            circularIndex.Assign(Index);
            Vector3 currentWaypoint = path[circularIndex];
            Vector3 nextWaypoint = path[circularIndex + 1];
            Vector3 nextButOneWaypoint = path[circularIndex + 2];

            //check if position is between the current waypoint and the next waypoint
            if(IsInSegment(position, currentWaypoint, nextWaypoint, out float ratio))
            {
                if (ratio > 0.5f) //if ration is greater than 0.5, it's closer to the end
                {
                    Index = circularIndex + 1;
                }

                Position = path[Index];
                Distance = Vector3.Distance(position, Position);
                return true;
            }
            //if the position is not between the current waypoint and the next, very likely it's going to be
            //between the next and the next but one
            else if(IsInSegment(position, nextWaypoint, nextButOneWaypoint, out ratio) && ratio < 0.5f)
            {
                Index = circularIndex + 1;
                Position = path[Index];
                Distance = Vector3.Distance(position, Position);
                return true;
            }

            return false;
        }

        private bool IsInSegment(Vector3 position, Vector3 start, Vector3 end, out float ratio)
        {
            (_, ratio) = projector.Project(position, start, end);
            return ratio >= 0f && ratio <= 1f;
        }
    } 
}
