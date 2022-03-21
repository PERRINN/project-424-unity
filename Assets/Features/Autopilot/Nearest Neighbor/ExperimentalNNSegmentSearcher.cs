using Perrinn424.Utilities;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentalNNSegmentSearcher 
{

    private IReadOnlyList<Vector3> path;
    private HeuristicNN nnSearcher;
    CircularIndex index;

    public ExperimentalNNSegmentSearcher(IReadOnlyList<Vector3> path)
    {
        this.path = path;
        nnSearcher = new HeuristicNN(path, 100, 100);
        index = new CircularIndex(0, path.Count);
    }

    public void Search(Transform t)
    {
        (int nn, _) = nnSearcher.Search(t.position);

        index.Assign(nn);
        Vector3 wayPoint = path[nn];

        Vector3 localWaypoint = t.transform.InverseTransformPoint(wayPoint);

        if (localWaypoint.z > 0)
            index--;

        StartIndex = index;
        EndIndex = index + 1;
        Start = path[StartIndex];
        End = path[EndIndex];
        Segment = End - Start;

        Vector3 endLocal = t.transform.InverseTransformPoint(End);
        Vector3 startLocal = t.transform.InverseTransformPoint(Start);
        Ratio = Mathf.InverseLerp(startLocal.z, endLocal.z, 0f);

        Debug.Log(Start);
        ProjectedPosition = Vector3.Lerp(Start, End, Ratio);
    }

    public int StartIndex { get; private set; }
    public int EndIndex { get; private set; }
    public Vector3 Start { get; private set; }
    public Vector3 End { get; private set; }
    public Vector3 Segment { get; private set; }
    public float Ratio { get; private set; }

    public Vector3 ProjectedPosition { get; set; }

}
