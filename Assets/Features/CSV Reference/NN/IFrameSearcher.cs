using UnityEngine;

public interface IFrameSearcher
{
    void Search(Transform t);
    int ClosestFrame1 { get; }
    int ClosestFrame2 { get; }
    float ClosestDisFrame1 { get; }
    float ClosestDisFrame2 { get; }
}
