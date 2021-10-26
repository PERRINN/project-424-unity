using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyForce : MonoBehaviour
{
    public Rigidbody rb;
    public Vector3 localPosition;
    public Vector3 force;
    public bool apply;

    private void FixedUpdate()
    {
        if (apply)
        {
            Vector3 pos = rb.transform.TransformPoint(localPosition);
            rb.AddForceAtPosition(force, pos, ForceMode.Impulse);
            apply = false;
        }
    }
}
