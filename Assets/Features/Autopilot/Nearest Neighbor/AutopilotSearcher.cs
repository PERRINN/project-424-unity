using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    public class AutopilotSearcher : INearestSegmentSearcher
    {
        private BaseAutopilot autopilot;
        private Path path;
        private INearestSegmentSearcher autopilotOnSearcher;
        private INearestSegmentSearcher autopilotOffSearcher;
        private INearestSegmentSearcher currentSearcher;
        private HeuristicNearestNeighbor heuristicNN;
        private AutopilotOffModeSearcher autopilotOffModeSearcher;

        //private bool wasStartup;

        public AutopilotSearcher(BaseAutopilot autopilot, RecordedLap recordedLap)
        {
            this.autopilot = autopilot;

            path = new Path(recordedLap);
            IProjector projector = new CrossProductProjector();
            
            int lookBehind = (int)(1f * recordedLap.frequency); //seconds to samples
            int lookAhead = (int)(2f * recordedLap.frequency); //seconds to samples
            heuristicNN = new HeuristicNearestNeighbor(path, lookBehind, lookAhead, 4);
            autopilotOnSearcher = new NearestSegmentComposed(heuristicNN, projector, path);
            
            autopilotOffModeSearcher = new AutopilotOffModeSearcher(path);
            autopilotOffSearcher = new NearestSegmentComposed(autopilotOffModeSearcher, projector, path);

            currentSearcher = SelectSearcher();
        }

        public void Search(Transform t)
        {
            //if (wasStartup && !autopilot.IsStartup)
            //{
            //    RefreshHeuristicIndex();
            //}

            currentSearcher = SelectSearcher();
            currentSearcher.Search(t);

            //wasStartup = autopilot.IsStartup;
        }

        private INearestSegmentSearcher SelectSearcher()
        {
            if (autopilot.IsOn && autopilot.IsStartup)
            {
                return autopilotOffSearcher;
            }
            else if (autopilot.IsOn && !autopilot.IsStartup)
            {
                return autopilotOnSearcher;
            }
            else
            {
                return autopilotOffSearcher;
            }
        }

        public void RefreshHeuristicIndex()
        {
            heuristicNN.SetHeuristicIndex(autopilotOffModeSearcher.Index);
        }

        public int StartIndex => currentSearcher.StartIndex;
        public int EndIndex => currentSearcher.StartIndex;
        public Vector3 Start => currentSearcher.Start;
        public Vector3 End => currentSearcher.End;
        public Vector3 Segment => currentSearcher.Segment;
        public Vector3 ProjectedPosition => currentSearcher.ProjectedPosition;
        public float Ratio => currentSearcher.Ratio;
    }



}
