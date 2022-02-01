using Perrinn424;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VehiclePhysics;

//https://bl.ocks.org/mbostock/8027637
//https://github.com/viliwonka/KDTree

[ExecuteInEditMode]
public class FrameSearcherTest : MonoBehaviour, IComparer<Vector3>
{
    //public VPReplayAsset asset;
    public VPReplayAsset asset => provider.GetReplayAsset();
    public Vector3[] positions;
    public int index;
    public int nextIndex;

    public Transform a;
    public Transform b;
    public Vector3 projection;
    public Vector3 AB;

    public bool belong;
    IFrameSearcher frameSearcher;

    public AutopilotProvider provider;

    void Start()
    {
        //positions = asset.recordedData.Select(d => d.position).ToArray();
    }

    // Update is called once per frame
    void Update()
    {
        //if (frameSearcher == null) frameSearcher = new FrameSearcher(provider.GetReplayAsset().recordedData);
        if (frameSearcher == null) frameSearcher = new HeuristicFrameSearcher(provider.GetReplayAsset().recordedData, 5, -10, 100);
        //AB = b.position - a.position;
        //projection = Vector3.Project(this.transform.position - a.position, b.position - a.position);

        ////belong = projection.sqrMagnitude < AB.sqrMagnitude && Vector3.Dot(AB, projection) >= 0f;

        //belong = Check(a.position, b.position);

        //SearchLinear();
        //a.position = asset.recordedData[index].position;
        //b.position = asset.recordedData[index+1].position;

        frameSearcher.Search(this.transform);
        index = frameSearcher.ClosestFrame1;
        nextIndex = frameSearcher.ClosestFrame2;
        this.transform.position = new Vector3(transform.position.x, provider.GetReplayAsset().recordedData[index].position.y, transform.position.z);

        a.transform.position = provider.GetReplayAsset().recordedData[index].position;
        b.transform.position = provider.GetReplayAsset().recordedData[nextIndex].position;
    }

    private void SearchLinear()
    {

        float FastDistance(Vector3 a, Vector3 b)
        {

            float xDiff = a.x - b.x;
            float zDiff = a.z - b.z;
            return xDiff * xDiff + zDiff * zDiff;
        }


        float maxDistance = float.PositiveInfinity;
        for (int i = 0; i < asset.recordedData.Count; i++)
        {
            Vector3 current = asset.recordedData[i].position;
            float d = FastDistance(current, this.transform.position);
            if (d < maxDistance)
            {
                maxDistance = d;
                index = i;
            }
            //Vector3 next = asset.recordedData[i + 1].position;
            //if (Check(current, next))
            //{
            //    index = i;
            //    break;
            //}
        }
    }

    private bool Check(Vector3 origin, Vector3 destination)
    {
        //Vector3 segment = destination - origin;
        //Vector3 vectorToPoint = point - origin;
        //Vector3 proj = Vector3.Project(vectorToPoint, segment);
        //return proj.sqrMagnitude < segment.sqrMagnitude && Vector3.Dot(segment, proj) >= 0f;

        Vector3 localCurrent = this.transform.InverseTransformPoint(origin);
        Vector3 localNext = this.transform.InverseTransformPoint(destination);
        return localNext.z > 0 && localCurrent.z < 0;
    }

    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.white;
        //Gizmos.DrawSphere(a.position, 0.1f);
        //Gizmos.color = Color.red;
        //Gizmos.DrawSphere(b.position, 0.1f);
        //Gizmos.DrawRay(a.position, AB);
        //Gizmos.color = Color.yellow;
        //Gizmos.DrawSphere(this.transform.position, 0.1f);
        //Gizmos.DrawLine(a.position, this.transform.position);


        //Gizmos.color = Color.green;
        //Gizmos.DrawRay(a.position, projection);

        for (int i = 0; i < asset.recordedData.Count - 1; i++)
        {
            Vector3 current = asset.recordedData[i].position;
            Vector3 next = asset.recordedData[i + 1].position;
            Gizmos.DrawLine(current, next);
        }


        Gizmos.color = Color.white;

        for (int i = -2; i < 4; i++)
        {
            Gizmos.DrawSphere(asset.recordedData[i+index].position, 0.1f);
        }
        //a.position = asset.recordedData[index].position;

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(asset.recordedData[index].position, 0.1f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(asset.recordedData[nextIndex].position, 0.1f);

    }

    private void SearchBinary()
    {
        index = Array.BinarySearch<Vector3>(positions, this.transform.position, this);
        if (index >= 0)
        {
            //exists
            //...
        }
        else
        {
            // doesn't exist
            int indexOfBiggerNeighbour = ~index; //bitwise complement of the return value

            if (indexOfBiggerNeighbour == positions.Length)
            {
                // bigger than all elements
                //...
                index = -1;
            }

            else if (indexOfBiggerNeighbour == 0)
            {
                // smaller than all elements
                //...
                index = -1;
            }

            else
            {
                // Between 2 elements
                int indexOfSmallerNeighbour = indexOfBiggerNeighbour - 1;
                //...
                index = indexOfSmallerNeighbour;
            }
        }
    }

    public int Compare(Vector3 a, Vector3 b)
    {
        float sqrtDistA = FastDistance(a, this.transform.position);
        float sqrtDistB = FastDistance(b, this.transform.position);
        return sqrtDistA.CompareTo(sqrtDistB);
        //float localZofPositionA = this.transform.InverseTransformPoint(a).z;
        //float localZofPositionB = this.transform.InverseTransformPoint(b).z;
        //return localZofPositionA.CompareTo(localZofPositionB);
    }

    public float FastDistance(Vector3 a, Vector3 b)
    {

        float xDiff = a.x - b.x;
        float zDiff = a.z - b.z;
        return xDiff * xDiff + zDiff * zDiff;
    }
}
