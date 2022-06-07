using System;
using UnityEngine;

//https://math.stackexchange.com/questions/1792826/estimate-angular-velocity-and-acceleration-from-a-sequence-of-rotations
//https://fgiesen.wordpress.com/2012/08/24/quaternion-differentiation/
//https://physics.stackexchange.com/questions/460311/derivation-for-angular-acceleration-from-quaternion-profile
[Serializable]
public class QuaternionDifferentiation
{
    public Rigidbody rb;

    public Quaternion w;

    public ShiftBuffer<Quaternion> buffer;

    public float[] coefficients;

    public Vector3 angularAcceleration;
    public Vector3 localAngularAcceleration;

    public void Init(Rigidbody rb)
    {
        this.rb = rb;
        
        coefficients = new[] { 1f, -2f, 1 };
        buffer = new ShiftBuffer<Quaternion>(coefficients.Length);
        buffer.Fill(rb.transform.rotation);
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

        Quaternion rotation_dot_dot = ScalarMultiply(accumulator, 1f / (dt*dt));
        w = rotation_dot_dot * Quaternion.Inverse(buffer[0]);
        w = ScalarMultiply(w, 2f);
        angularAcceleration = new Vector3(w.x, w.y, w.z);
        localAngularAcceleration = rb.transform.InverseTransformDirection(angularAcceleration);
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
