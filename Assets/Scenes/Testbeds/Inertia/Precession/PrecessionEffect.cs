using UnityEngine;

public class PrecessionEffect
{
    public Rigidbody rb;
    public Matrix4x4 InertiaTensor { get; private set; }
    public Vector3 AngularVelocity { get; private set; }
    public Vector3 AngularMomentum { get; private set; }
    public Vector3 AngularMomentum_derivative { get; private set; }
    public Vector3 AngularAcceleration { get; private set; }

    public PrecessionEffect(Rigidbody rb)
    {
        this.rb = rb;
        InertiaTensor = InertiaTensorUtils.CalculateInertiaTensorMatrix(rb);
    }

    public void Apply()
    {
        AngularVelocity = rb.angularVelocity;
        AngularMomentum = InertiaTensor * AngularVelocity;
        AngularMomentum_derivative = Vector3.Cross(AngularVelocity, AngularMomentum);
        AngularAcceleration = InertiaTensor.inverse * AngularMomentum_derivative;
        rb.AddTorque(AngularAcceleration, ForceMode.Acceleration);
    }
}
