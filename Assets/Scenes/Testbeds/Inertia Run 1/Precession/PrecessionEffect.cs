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
        Matrix4x4 rotationMatrix = Matrix4x4.Rotate(rb.transform.rotation);
        Matrix4x4 inertiaTensorLocal = rotationMatrix * InertiaTensor * rotationMatrix.transpose;
        AngularVelocity = rb.angularVelocity;
        AngularMomentum = inertiaTensorLocal * AngularVelocity;
        AngularMomentum_derivative = Vector3.Cross(AngularVelocity, AngularMomentum);
        rb.AddTorque(-AngularMomentum_derivative);
    }
}
