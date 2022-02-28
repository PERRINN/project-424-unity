using EdyCommonTools;
using UnityEngine;

public class PathErrorCorrector
{
    private Rigidbody rb;

    private PidController pid;

    public PathErrorCorrector(Rigidbody rb)
    {
        this.rb = rb;
        pid = new PidController();
    }

    public void SetPIDParameters(float kp, float ki, float kd)
    {
        pid.SetParameters(kp, ki, kd);
    }

    public void Correct(Vector3 targetPosition)
    {
        Vector3 distanceError = rb.transform.position - targetPosition;
        distanceError = Vector3.ProjectOnPlane(distanceError, rb.transform.up);

        float distanceErrorMagnitude = distanceError.magnitude;
        Vector3 distanceErrorDir = distanceError.normalized;

        pid.input = distanceErrorMagnitude;
        pid.Compute();

        rb.AddForceAtPosition(distanceErrorDir * pid.output, rb.transform.position);
        //rb.AddForce(distanceErrorDir * pid.output);
    }
}
