using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    public interface INearestSegmentSearcher
    {
        void Search(Vector3 position);

        int StartIndex { get; }
        int EndIndex { get; }
    } 
}
