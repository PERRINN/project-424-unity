using Perrinn424.AutopilotSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathDrawer : MonoBehaviour
{
    public RecordedLap recordedLap;
    public bool draw;
    private void OnDrawGizmosSelected()
    {
        if (!draw)
            return;

        for (int i = 0; i < recordedLap.Count - 1; i++)
        {
            Vector3 current = recordedLap[i].position;
            Vector3 next = recordedLap[i+1].position;

            Gizmos.DrawLine(current, next);
            Gizmos.DrawSphere(current, 0.1f);
            Gizmos.DrawSphere(next, 0.1f);
        }
    }

}
