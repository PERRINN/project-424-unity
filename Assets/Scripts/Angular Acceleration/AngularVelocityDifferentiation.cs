using System;
using UnityEngine;

[Serializable]
public class AngularVelocityDifferentiation
{
    public Rigidbody rb;

    public Vector3 angularVelocity;
    public Vector3 angularVelocity_prev;
    public Vector3 angularAcceleration;
    public Vector3 localAngularAcceleration;

    public AngularVelocityDifferentiation(Rigidbody rb)
    {
        this.rb = rb;
        angularVelocity = rb.angularVelocity;
        angularVelocity_prev = angularVelocity;

    }

    public void Compute(float dt)
    {
        angularVelocity = rb.angularVelocity;
        Vector3 diff = angularVelocity - angularVelocity_prev;
        angularAcceleration = diff / dt;
        localAngularAcceleration = rb.transform.InverseTransformDirection(angularAcceleration);

        angularVelocity_prev = angularVelocity;
    }
}
