using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

public class PrecessionTest : MonoBehaviour
{
    [Header("Setup")]
    public Rigidbody rb;
    [FormerlySerializedAs("torque")]
    public Vector3 initialAngularVelocity;
    public bool doInitialAngularVelocity;
    public bool doPrecession;

    [Header("Debug config")]
    public int rayNumber = 15;
    public float rayDelay = 0.1f;
    public float rayMult = 3.0f;

    [Header("Debug")]
    public Vector3 angularVelocity;
    public Vector3 precessionTorque;
    public Matrix4x4 inertiaTensor;
    public Vector3 angularMomentum;
    public Vector3 angularMomentum_derivative;
    public Vector3 angularAcceleration;


    private WindowBuffer angularVelocityList;

    public PrecessionEffect precessionEffect;

    private void Awake()
    {
        angularVelocityList = new WindowBuffer(rayNumber);
    }

    private void OnEnable()
    {
        precessionEffect = new PrecessionEffect(rb);

        rb.maxAngularVelocity = 20f;


        if (doInitialAngularVelocity)
        {
            rb.AddTorque(initialAngularVelocity, ForceMode.VelocityChange);
        }

        inertiaTensor = precessionEffect.InertiaTensor;
        //print(inertiaTensor);

        StartCoroutine(DoGetAngularVelocity());
    }


    private IEnumerator DoGetAngularVelocity()
    {
        var wait = new WaitForSeconds(rayDelay);
        while (true)
        {
            angularVelocityList.Add(rb.angularVelocity);
            yield return wait;
        }
    }

    private void FixedUpdate()
    {
        if (doPrecession)
        {
            precessionEffect.Apply();
        }

        DebugValues();
    }

    private void DebugValues()
    {
        angularVelocity = rb.angularVelocity;
        angularMomentum = precessionEffect.AngularMomentum;
        angularMomentum_derivative = precessionEffect.AngularMomentum_derivative;
        angularAcceleration = precessionEffect.AngularAcceleration;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        DrawLabel();
        DrawAngularVelocity();
    }

    private void DrawLabel()
    {
#if UNITY_EDITOR
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;
        string msg = $"Precession {doPrecession}\nAngular velocity {rb.angularVelocity:F2}";
        UnityEditor.Handles.Label(this.transform.position, msg, style);
#endif
    }

    private void DrawAngularVelocity()
    {
        for (int i = 0; i < angularVelocityList.windowFillCount; i++)
        {
            Color c = Color.white;
            c.a = (float)i / angularVelocityList.windowFillCount;
            DrawRay(angularVelocityList.values[i], c);
        }

        DrawRay(rb.angularVelocity, Color.red);
    }

    private void DrawRay(Vector3 dir, Color c)
    {
        Gizmos.color = c;

        Vector3 pos = rb.transform.TransformPoint(rb.centerOfMass);
        Gizmos.DrawRay(pos, rayMult * dir);
    }
}
