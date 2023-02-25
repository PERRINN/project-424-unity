using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    public class AutopilotSearcher : INearestSegmentSearcher
    {
        private BaseAutopilot autopilot;

        private INearestSegmentSearcher segmentSearcher;
        private AutopilotNearestNeighbourSearcher nearestNeighbourSearcher;

        //private bool wasStartup;

        public AutopilotSearcher(BaseAutopilot autopilot, RecordedLap recordedLap)
        {
            this.autopilot = autopilot;

            Path path = new Path(recordedLap);
            
            int lookBehind = (int)(1f * recordedLap.frequency); //seconds to samples
            int lookAhead = (int)(2f * recordedLap.frequency); //seconds to samples
            HeuristicNearestNeighbor heuristicNN = new HeuristicNearestNeighbor(path, lookBehind, lookAhead, 6);
            SectorSearcherNearestNeighbor sectorSearcher = new SectorSearcherNearestNeighbor(path, 2, float.PositiveInfinity);
            AutopilotNearestNeighbourSearcher nearestNeighbourSearcher = new AutopilotNearestNeighbourSearcher(heuristicNN, sectorSearcher);

            IProjector projector = new CrossProductProjector();
            segmentSearcher = new NearestSegmentComposed(nearestNeighbourSearcher, projector, path);
        }

        public void Search(Transform t)
        {
            segmentSearcher.Search(t);
        }

        public int StartIndex => segmentSearcher.StartIndex;
        public int EndIndex => segmentSearcher.EndIndex;
        public Vector3 Start => segmentSearcher.Start;
        public Vector3 End => segmentSearcher.End;
        public Vector3 Segment => segmentSearcher.Segment;
        public Vector3 ProjectedPosition => segmentSearcher.ProjectedPosition;
        public float Ratio => segmentSearcher.Ratio;
    }
}
