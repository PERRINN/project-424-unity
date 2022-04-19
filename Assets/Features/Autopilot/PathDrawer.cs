using Perrinn424.AutopilotSystem;
using Perrinn424.Utilities;
using UnityEngine;

public class PathDrawer : MonoBehaviour
{
    public RecordedLap recordedLap;
    public bool draw;
    public bool drawAll;
    public int index;
    public int ahead;
    public int behind;
    public Color color;
    private void OnDrawGizmosSelected()
    {
        if (!draw || recordedLap == null)
            return;

        Gizmos.color = color;
        if (drawAll)
        {
            Draw(0, recordedLap.Count, 0);
        }
        else
        {
            Draw(index, ahead, behind);
        }
    }

    private void Draw(int index, int ahead, int behind)
    {
        int count = ahead + behind;
        CircularIndex circularIndex = new CircularIndex(recordedLap.Count);
        for (int i = 0; i < count - 1; i++)
        {
            circularIndex.Assign(index - behind + i);
            Vector3 current = recordedLap[circularIndex].position;
            Vector3 next = recordedLap[circularIndex + 1].position;

            Gizmos.DrawLine(current, next);
            Gizmos.DrawSphere(current, 0.1f);
            Gizmos.DrawSphere(next, 0.1f);
        }

        Gizmos.DrawSphere(recordedLap[index].position, 0.25f);

    }

}
