using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    public interface INearestSegmentSearcher
    {
        void Search(Transform t);

        int StartIndex { get; }
        int EndIndex { get; }
        Vector3 Start { get; }
        Vector3 End { get; }
        Vector3 Segment { get; }

        Vector3 ProjectedPosition { get; }
        float Ratio { get; }

    } 
}
