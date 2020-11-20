using System;
using System.Collections.Generic;
using Perrinn424.UI;
using UnityEngine;
using VehiclePhysics;

[ExecuteInEditMode]
public class MapGenerator : MonoBehaviour
{
    public VPReplayAsset replay;
    public float timeStep = 0.05f;

    public Vector3 [] points = new Vector3[0];
    public Rect rect;

    public MapGeneratorXXX mapGeneratorXxx;

    private void Awake()
    {
        rect = new Rect(float.PositiveInfinity, float.PositiveInfinity, float.NegativeInfinity, float.NegativeInfinity);
        Create();
    }

    [ContextMenu("Force")]
    private void Create()
    {
        mapGeneratorXxx = new MapGeneratorXXX(400, replay, timeStep);

        //replay.GetPositions(timeStep, out points, out rect);

        //float origin = Mathf.Min(rect.x, rect.y);
        //float size = Mathf.Max(rect.width, rect.height);
        //float augmentedSize = size * 0.1f;

        //size += augmentedSize;
        //origin -= augmentedSize*0.5f;
        //rect = new Rect(origin, origin, size, size);
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
