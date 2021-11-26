using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InertiaTensor : MonoBehaviour
{
    public Rigidbody rb;
    public Vector3 inertiaTensor;
    public Quaternion inertiaTensorRotation;
    public Matrix4x4 m;
    // Start is called before the first frame update
    void Start()
    {
        inertiaTensor = rb.inertiaTensor;
        inertiaTensorRotation = rb.inertiaTensorRotation;
        //m = RotationMatrix(inertiaTensor, inertiaTensorRotation);
        //m = RotationMatrix(inertiaTensor, inertiaTensorRotation);
        //m = InertiaTensorUtils.CalculateInertiaTensorMatrix(inertiaTensor, inertiaTensorRotation);
        m = InertiaTensorUtils.CalculateInertiaTensorMatrix(rb);
        print(m);
    }

    public Matrix4x4 RotationMatrix(Vector3 d, Quaternion q)
    {
        Matrix4x4 R = Matrix4x4.Rotate(q);
        Matrix4x4 S = Matrix4x4.Scale(d);
        return R * S * R.inverse;
    }
}
