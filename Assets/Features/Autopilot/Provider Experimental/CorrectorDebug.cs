using Perrinn424.AutopilotSystem;
using UnityEngine;

public class CorrectorDebug : MonoBehaviour
{
    public Rigidbody rb;
    public Transform com;

    public bool position;
    public bool rotation;
    public PositionCorrector positionCorrector;
    public RotationCorrector rotationCorrector;

    private void OnEnable()
    {
        rb.centerOfMass = rb.transform.InverseTransformPoint(com.transform.position);
        positionCorrector.Init(rb);
        rotationCorrector.Init(rb);
    }

    private void FixedUpdate()
    {
        if (position)
        {
            positionCorrector.Correct(this.transform.position);
        }
        if (rotation)
        {
            rotationCorrector.Correct(this.transform.rotation);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(positionCorrector.ApplicationPosition, positionCorrector.Force);
    }
}
