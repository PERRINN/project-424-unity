using Perrinn424;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VehiclePhysics;

public class FrameSearcher
{

    //private Transform vehicle;
    //private AutopilotProvider autopilotProvider;

    public FrameSearcher(Transform vehicle, AutopilotProvider provider)
    {
        //this.vehicle = vehicle;
        //this.autopilotProvider = provider;
    }
    public FrameSearcher() { }

    public int closestFrame1;
    public int closestFrame2;
    public float closestDisFrame1;
    public float closestDisFrame2;
    int sectionSize;

    public void Search(IList<VPReplay.Frame> frames, Vector3 position)
    {
        //Vector3 position = vehicle.transform.position;
        float currentPosX = position.x;
        float currentPosZ = position.z;

        int sectionClosestFrame1 = 0;
        int sectionClosestFrame2 = 0;
        closestFrame1 = 0;
        closestFrame2 = 0;

        closestDisFrame1 = float.MaxValue;
        closestDisFrame2 = float.MaxValue;

        sectionSize = (int)Mathf.Sqrt(frames.Count);
        // Search two closest section frames
        for (int i = 0; i <= sectionSize; i++)
        {
            //int recordedFrameNum = (i == sectionSize) ? frames.Count - sectionSize : sectionSize * i;
            int recordedFrameNum = (i == sectionSize) ? frames.Count - sectionSize : sectionSize * i;

            if (i == sectionSize)
            {
                recordedFrameNum = frames.Count - sectionSize;
            }
            else
            {
                recordedFrameNum = sectionSize * i;
            }

            float x = frames[recordedFrameNum].position.x - currentPosX;
            float z = frames[recordedFrameNum].position.z - currentPosZ;

            float distanceCalculation = (float)Mathf.Sqrt((x * x) + (z * z));

            if (distanceCalculation < closestDisFrame1)
            {
                sectionClosestFrame2 = sectionClosestFrame1;
                sectionClosestFrame1 = recordedFrameNum;
                closestDisFrame2 = closestDisFrame1;
                closestDisFrame1 = distanceCalculation;
            }
            else if (distanceCalculation < closestDisFrame2)
            {
                sectionClosestFrame1 = recordedFrameNum;
                closestDisFrame2 = distanceCalculation;
            }
        }

        (sectionClosestFrame1, sectionClosestFrame2) = GetAsMinMax(sectionClosestFrame1, sectionClosestFrame2);

        //Boundary search conditions
        if (sectionClosestFrame1 == 0 && sectionClosestFrame2 > frames.Count / 2)
        {
            sectionClosestFrame1 = sectionClosestFrame2;
            sectionClosestFrame2 = frames.Count - 1;
        }
        else
        if (sectionClosestFrame1 == sectionSize && sectionClosestFrame2 == frames.Count - sectionSize)
        {
            sectionClosestFrame1 = sectionSize * sectionSize;
        }

        // Reset Distance value
        closestDisFrame1 = float.MaxValue;
        closestDisFrame2 = float.MaxValue;

        // Boundary search conditions
        sectionClosestFrame1 = (sectionClosestFrame1 - sectionSize / 2 <= 0) ? 0 : sectionClosestFrame1 -= sectionSize / 2;
        sectionClosestFrame2 = (sectionClosestFrame2 + sectionSize / 2 >= frames.Count) ? frames.Count - 1 : sectionClosestFrame2 += sectionSize / 2;

        // Search two closest frames
        for (int i = sectionClosestFrame1; i <= sectionClosestFrame2; i++)
        {
            float x = frames[i].position.x - currentPosX;
            float z = frames[i].position.z - currentPosZ;

            float distanceCalculation = (float)Mathf.Sqrt((x * x) + (z * z));

            if (distanceCalculation < closestDisFrame1)
            {
                closestFrame2 = closestFrame1;
                closestFrame1 = i;
                closestDisFrame2 = closestDisFrame1;
                closestDisFrame1 = distanceCalculation;
            }
            else if (distanceCalculation < closestDisFrame2)
            {
                closestFrame2 = i;
                closestDisFrame2 = distanceCalculation;
            }
        }

        //new code
        for (int i = 0; i < 60; i++)
        {
            float x = frames[i].position.x - currentPosX;
            float z = frames[i].position.z - currentPosZ;

            float distanceCalculation = (float)Mathf.Sqrt((x * x) + (z * z));

            if (distanceCalculation < closestDisFrame1)
            {
                closestFrame2 = closestFrame1;
                closestFrame1 = i;
                closestDisFrame2 = closestDisFrame1;
                closestDisFrame1 = distanceCalculation;
            }
            else if (distanceCalculation < closestDisFrame2)
            {
                closestFrame2 = i;
                closestDisFrame2 = distanceCalculation;
            }
        }

        if (closestFrame1 == closestFrame2)
        {
            Debug.LogError($"closest are equal {closestFrame1}");
            closestFrame2 = closestFrame1 + 1;

            float x = frames[closestFrame2].position.x - currentPosX;
            float z = frames[closestFrame2].position.z - currentPosZ;
            closestDisFrame2 = (float)Mathf.Sqrt((x * x) + (z * z));
        }

        (closestFrame1, closestFrame2) = GetAsMinMax(closestFrame1, closestFrame2);
        closestDisFrame1 = Distance2D(frames[closestFrame1].position, position);
        closestDisFrame2 = Distance2D(frames[closestFrame2].position, position);

    }

    private float Distance2D(Vector3 a, Vector3 b)
    {
        float x = a.x - b.x;
        float z = a.z - b.z;
        return (float)Mathf.Sqrt((x * x) + (z * z));
    }

    (int, int) GetAsMinMax(int valueA, int valueB)
    {
        if (valueA > valueB)
        {
            int tmp = valueA;
            valueA = valueB;
            valueB = tmp;
        }

        return (valueA, valueB);
    }
}
