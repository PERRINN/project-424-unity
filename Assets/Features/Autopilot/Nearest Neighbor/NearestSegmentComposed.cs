using Perrinn424.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    public class NearestSegmentComposed : INearestSegmentSearcher
    {
        private IReadOnlyList<Vector3> path;
        private INearestNeighbourSearcher seacher;
        private IProjector projector;
        private CircularIndex index;

        public NearestSegmentComposed(INearestNeighbourSearcher seacher, IProjector projector, IReadOnlyList<Vector3> path)
        {
            this.seacher = seacher;
            this.projector = projector;
            this.path = path;
            index = new CircularIndex(0, path.Count);
        }

        public void Search(Transform t)
        {
            seacher.Search(t.position);

            index.Assign(seacher.Index);
            Vector3 wayPoint = seacher.Position;

            Vector3 localWaypoint = t.transform.InverseTransformPoint(wayPoint);

            if (localWaypoint.z > 0)
                index--;

            StartIndex = index;
            EndIndex = index + 1;
            Start = path[StartIndex];
            End = path[EndIndex];
            Segment = End - Start;

            (ProjectedPosition, Ratio) = projector.Project(t, Start, End);
        }

        public int StartIndex { get; private set; }
        public int EndIndex { get; private set; }
        public Vector3 Start { get; private set; }
        public Vector3 End { get; private set; }
        public Vector3 Segment { get; private set; }
        public float Ratio { get; private set; }
        public Vector3 ProjectedPosition { get; set; }
    } 
}
