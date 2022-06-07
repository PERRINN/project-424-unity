using System;
using UnityEngine;

[Serializable]
public class AngularVelocityDifferentiation
{
    public Rigidbody rb;

    public Vector3 angularAcceleration;
    public Vector3 localAngularAcceleration;

    public float[] coefficients;

    public ShiftBuffer<Vector3> buffer;

    public AngularVelocityDifferentiation(Rigidbody rb, float [] coefficients)
    {
        this.rb = rb;
        this.coefficients = coefficients;
        buffer = new ShiftBuffer<Vector3>(coefficients.Length);
        buffer.Fill(rb.angularVelocity);


    }

    public void Compute(float dt)
    {
        buffer.Push(rb.angularVelocity);

        Vector3 accumulator = Vector3.zero;
        for (int i = 0; i < buffer.length; i++)
        {
            accumulator += buffer[i] * coefficients[i];
        }

        accumulator = accumulator / dt;
        angularAcceleration = accumulator;

        localAngularAcceleration = rb.transform.InverseTransformDirection(angularAcceleration);
    }
}
