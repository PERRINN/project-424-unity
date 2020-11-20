using System;
using System.Collections.Generic;
using UnityEngine;
using VehiclePhysics;

[ExecuteInEditMode]
public class MapGenerator : MonoBehaviour
{
    public VPReplayAsset replay;
    public float timeStep = 0.05f;

    public Vector3 [] points = new Vector3[0];
    public Rect rect;


    private void Awake()
    {
        rect = new Rect(float.PositiveInfinity, float.PositiveInfinity, float.NegativeInfinity, float.NegativeInfinity);
        Create();
    }

    [ContextMenu("Force")]
    private void Create()
    {
        timeStep = Mathf.Max(timeStep, replay.timeStep);
        int step = (int)(timeStep / replay.timeStep);
        int newLength = replay.recordedData.Count / step;
        points = new Vector3[newLength];

        float minX, minZ, maxX, maxZ;
        minX = minZ = float.PositiveInfinity;
        maxX = maxZ = float.NegativeInfinity;

        for (int i = 0; i < newLength; i++)
        {
            points[i] = replay.recordedData[i * step].position;

            Vector3 test = points[i];

            if (test.x < minX)
                minX = test.x;

            if (test.z < minZ)
                minZ = test.z;

            if (test.x > maxX)
                maxX = test.x;

            if (test.z > maxZ)
                maxZ = test.z;
        }

        rect = new Rect(minX, minZ, maxX-minX, maxZ-minZ);

        float origin = Mathf.Min(rect.x, rect.y);
        float size = Mathf.Max(rect.width, rect.height);
        float augmentedSize = size * 0.1f;

        size += augmentedSize;
        origin -= augmentedSize*0.5f;
        rect = new Rect(origin, origin, size, size);
    }

    private void OnDrawGizmos()
    {
        if(replay == null)
            return;

        for (int i = 0; i < points.Length - 1; i++)
        {
            Gizmos.DrawLine(points[i], points[i+1]);
        }
        //foreach (Vector3 frame in points)
        //{
        //    Gizmos.DrawSphere(frame, 0.1f);
        //}
    }
}
