using UnityEngine;

public interface INearestSegmentSearcher
{
    void Search(Vector3 position);

    int StartIndex { get;}
    int EndIndex { get; }
}
