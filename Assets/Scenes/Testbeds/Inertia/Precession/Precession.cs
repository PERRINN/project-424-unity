using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Collections;

public class Precession : MonoBehaviour
{
    public Rigidbody rb;
    public Transform com;
    public Vector3 torque;
    public Vector3 angularVelocity;
    public Vector3 precessionTorque;
    public Matrix4x4 inertiaTensor;
    public Vector3 angularMomentum;
    public Vector3 angularMomentum_derivative;
    public Vector3 angularAcceleration;

    public bool doInitialAngularVelocity;
    public bool doPrecession;

    private WindowBuffer angularVelocityList;

    private void Awake()
    {
        angularVelocityList = new WindowBuffer(15);
    }

    void Start()
    {
        rb.maxAngularVelocity = 20f;

        if(com != null)
            rb.centerOfMass = com.transform.localPosition;
        
        if(doInitialAngularVelocity)
            rb.AddTorque(torque, ForceMode.VelocityChange);
        
        inertiaTensor = InertiaTensorUtils.CalculateInertiaTensorMatrix(rb);
        print(inertiaTensor);

        StartCoroutine(DoAddAngularVelocity());
    }

    private IEnumerator DoAddAngularVelocity()
    {
        var wait = new WaitForSeconds(0.1f);
        while (true)
        {
            angularVelocityList.Add(rb.angularVelocity);
            yield return wait;
        }
    }

    private void FixedUpdate()
    {


        angularVelocity = rb.angularVelocity;
        angularMomentum = inertiaTensor * angularVelocity;
        angularMomentum_derivative = Vector3.Cross(angularVelocity, angularMomentum);
        angularAcceleration = inertiaTensor.inverse * angularMomentum_derivative;

        //angularVelocity = angularVelocity + angularAcceleration * Time.deltaTime;
        //rb.angularVelocity = angularVelocity;

        precessionTorque = inertiaTensor * angularAcceleration;

        if (!doPrecession)
            return;

        rb.AddTorque(angularAcceleration, ForceMode.Acceleration);
        //rb.AddTorque(precessionTorque);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;
        

        string msg = $"Precession {doPrecession}\nAngular velocity {rb.angularVelocity:F2}";
        Handles.Label(this.transform.position, msg, style);

        for (int i = 0; i < angularVelocityList.windowFillCount; i++)
        {
            Color c = Color.white;
            c.a = (float)i / angularVelocityList.windowFillCount;
            DrawRay(angularVelocityList.values[i], c);
        }
        //foreach (var previous in angularVelocityList)
        //{
        //    DrawRay(previous, Color.white);
        //}

        DrawRay(rb.angularVelocity, Color.red);
    }

    private void DrawRay(Vector3 dir, Color c)
    {
        Gizmos.color = c;

        Vector3 pos = this.com == null ? this.transform.position : this.com.position;
        Gizmos.DrawRay(pos, 3*dir);
    }
}
