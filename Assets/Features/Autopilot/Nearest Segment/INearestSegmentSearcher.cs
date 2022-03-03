using UnityEngine;

public interface INearestSegmentSearcher
{
    void Search(Transform t);

    int StartIndex { get;}
    int EndIndex { get; }
}
