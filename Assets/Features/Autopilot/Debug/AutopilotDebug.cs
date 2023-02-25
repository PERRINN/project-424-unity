using EdyCommonTools;
using Perrinn424.AutopilotSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutopilotDebug : MonoBehaviour
{
    public Rigidbody rb;
    public RecordedLap lap;

    private NearestSegmentSearcher segmentSearcher;
    private Path path;
    private PidController pid;

    public float force;

    private Vector3 Position => rb.transform.position;

    public float kp;
    public float ki;
    public float kd;

    public bool debugPath = false;

    private void OnEnable()
    {
        pid = new PidController();
        path = new Path(lap);
        segmentSearcher = new NearestSegmentSearcher(path);
    }

    void Start()
    {
        rb.AddForce(rb.transform.forward * force, ForceMode.VelocityChange);

    }

    private void FixedUpdate()
    {
        pid.SetParameters(kp, ki, kd);

        segmentSearcher.Search(rb.transform);
        Sample sample = Sample.Lerp(lap[segmentSearcher.StartIndex], lap[segmentSearcher.EndIndex], segmentSearcher.Ratio);
        Vector3 distanceError = Position - sample.position;

        float distanceErrorMagnitude = distanceError.magnitude;
        Vector3 distanceErrorLongitude = distanceError.normalized;

        pid.input = distanceErrorMagnitude;
        pid.Compute();

        rb.AddForce(distanceErrorLongitude * pid.output);


        rb.transform.rotation = sample.rotation;
        //Quaternion rotationError = Quaternion.Inverse(sample.rotation) * rb.rotation;
        //rb.AddTorque(-0.00001f*rotationError.eulerAngles);

    }

    private void OnDrawGizmos()
    {
        if (path == null)
            return;

        if (debugPath)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector3 current = lap[i].position;
                Vector3 next = lap[i + 1].position;
                Gizmos.DrawLine(current, next);
                Gizmos.DrawSphere(current, 0.1f);
                Gizmos.DrawSphere(next, 0.1f);
            }
        }


        if (segmentSearcher != null)
        {

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(path[segmentSearcher.StartIndex], path[segmentSearcher.EndIndex]);
            Gizmos.DrawSphere(path[segmentSearcher.StartIndex], 0.1f);
            Gizmos.DrawSphere(path[segmentSearcher.EndIndex], 0.1f);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(path[segmentSearcher.StartIndex], Position);
            Gizmos.DrawSphere(Position, 0.1f);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(Position, segmentSearcher.ProjectedPosition);
            Gizmos.DrawSphere(segmentSearcher.ProjectedPosition, 0.1f);
        }
    }
}
