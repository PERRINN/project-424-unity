using Perrinn424.AutopilotSystem;
using UnityEngine;

public class CorrectorDebug : MonoBehaviour
{
    public Rigidbody rb;
    public Transform com;
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
        positionCorrector.Correct(this.transform.position);
        rotationCorrector.Correct(this.transform.rotation);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(rb.transform.position, positionCorrector.Force);
    }
}
