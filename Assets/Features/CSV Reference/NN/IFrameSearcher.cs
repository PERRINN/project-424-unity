using System.Collections.Generic;
using UnityEngine;
using VehiclePhysics;

public interface IFrameSearcher
{
    void Search(IReadOnlyList<VPReplay.Frame> frames, Transform t);
    int ClosestFrame1 { get; }
    int ClosestFrame2 { get; }
    float ClosestDisFrame1 { get; }
    float ClosestDisFrame2 { get; }
}
