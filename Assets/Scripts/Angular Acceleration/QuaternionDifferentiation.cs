using System;
using UnityEngine;

[Serializable]
public class QuaternionDifferentiation
{
    public Rigidbody rb;

    public Quaternion w;
    public Quaternion rotation;
    public Quaternion rotation_prev;

    public void Init(Rigidbody rb)
    {
        this.rb = rb;

        rotation = rb.transform.rotation;
        rotation_prev = rotation;
    }

    public void Compute(float dt)
    {
        rotation_prev = rotation;
        rotation = rb.transform.rotation;
        Quaternion diff = Sub(rotation, rotation_prev);
        Quaternion rotation_dot = ScalarMultiply(diff, 1f / dt);
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
