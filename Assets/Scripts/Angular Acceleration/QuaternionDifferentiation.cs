using System;
using UnityEngine;

[Serializable]
public class QuaternionDifferentiation
{
    public Rigidbody rb;

    public Quaternion w;
    public Quaternion rotation;
    public Quaternion rotation_prev;

    public ShiftBuffer<Quaternion> buffer;

    public float[] coefficients;

    public void Init(Rigidbody rb)
    {
        this.rb = rb;

        rotation = rb.transform.rotation;
        rotation_prev = rotation;

        buffer = new ShiftBuffer<Quaternion>(2);
        buffer.Fill(rb.transform.rotation);

        coefficients = new[] { 1f, -1f };
    }

    public void Compute(float dt)
    {
        buffer.Push(rb.transform.rotation);

        Quaternion accumulator = new Quaternion(0f, 0f, 0f, 0f);
        for (int i = 0; i < buffer.length; i++)
        {
            Quaternion summand = ScalarMultiply(buffer[i], coefficients[i]);
            accumulator = Add(accumulator, summand);
        }


        Quaternion diff = Sub(rotation, rotation_prev);
        Quaternion rotation_dot = ScalarMultiply(accumulator, 1f / dt);
        w = rotation_dot * Quaternion.Inverse(rotation);
        w = ScalarMultiply(w, 2f);
    }

    public static Quaternion ScalarMultiply(Quaternion input, float scalar)
    {
        return new Quaternion(input.x * scalar, input.y * scalar, input.z * scalar, input.w * scalar);
    }

    public static Quaternion Add(Quaternion p, Quaternion q)
    {
        return new Quaternion(p.x + q.x, p.y + q.y, p.z + q.z, p.w + q.w);
    }

    public static Quaternion Sub(Quaternion p, Quaternion q)
    {
        return new Quaternion(p.x - q.x, p.y - q.y, p.z - q.z, p.w - q.w);
    }
}
