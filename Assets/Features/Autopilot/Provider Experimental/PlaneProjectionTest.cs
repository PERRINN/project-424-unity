using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PlaneProjectionTest : MonoBehaviour
{
    public Vector3 up;
    public Vector3 vector;
    public Vector3 planeProjection;

    private void Update()
    {
        up = this.transform.up;
        planeProjection = Vector3.ProjectOnPlane(vector, up);
    }

    private void OnDrawGizmos()
    {
        Gizmos.matrix = this.transform.localToWorldMatrix;

        Gizmos.DrawCube(Vector3.zero, new Vector3(10f, 0.01f, 10f));

        Gizmos.matrix = Matrix4x4.identity;

        Gizmos.color = Color.green;
        Gizmos.DrawRay(this.transform.position, vector);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(this.transform.position, planeProjection);
    }
}
