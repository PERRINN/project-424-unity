using Perrinn424.Utilities;
using System.Collections.Generic;
using UnityEngine;

public class BruteForceSearcher
{
    private readonly IReadOnlyList<Vector3> path;

    public BruteForceSearcher(IReadOnlyList<Vector3> path)
    {
        this.path = path;
    }

    public (int, float) Search(Vector3 position)
    {
        return Search(position, 0, path.Count, 1);
    }

    public (int, float) Search(Vector3 position, int from, int count, int step)
    {
        float sqrtDistance = Mathf.Infinity;

        CircularIndex circularIndex = new CircularIndex(path.Count);
        int closestIndex = -1;
        for (int i = 0; i < count; i = i + step)
        {
            circularIndex.Assign(from + i);
            float tempDistance = (position - path[circularIndex]).sqrMagnitude;
            if (tempDistance < sqrtDistance)
            {
                sqrtDistance = tempDistance;
                closestIndex = circularIndex;
            }
        }

        return (closestIndex, Mathf.Sqrt(sqrtDistance));
    }
}
