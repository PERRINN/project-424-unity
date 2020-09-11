using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VehiclePhysics;

public class PID : MonoBehaviour
{
    public VPReplay target;
    public GameObject cubeOne;
    public GameObject cubeTwo;
    public GameObject cubeCar;
    public VPReplayController replayController;
    public bool showPosition = true;

    List<VPReplay.Frame> recordedReplay = new List<VPReplay.Frame>();

    float height;

    void Start()
    {
        recordedReplay = replayController.predefinedReplay.recordedData;
    }

    void Update()
    {
        getDistance();

    }

    void OnGUI()
    {
        string errorDistance = "";
        errorDistance += height;
        GUI.Box(new Rect(185, Screen.height - 90, 100, 50), "Error");
        GUI.Label(new Rect(200, Screen.height - 65, 100, 50), errorDistance);
    }

    void getDistance()
    {
        int currentFrame = target.recordedData.Count - 1;
        float currentPosX = target.recordedData[currentFrame].position.x;
        float currentPosZ = target.recordedData[currentFrame].position.z;


        int sectionSize = recordedReplay.Count / 100;

        float minDistance = float.MaxValue;
        float minDistance2 = float.MaxValue;

        int frame1 = 0;
        int frame2 = 0;
        int frame3 = 0;
        int frame4 = 0;

        for (int i = 0; i < 100; i++)
        {
            float x = recordedReplay[sectionSize * i].position.x - currentPosX;
            float z = recordedReplay[sectionSize * i].position.z - currentPosZ;

            float distance = (float)Math.Sqrt((x * x) + (z * z));

            if (distance < minDistance)
            {
                frame2 = frame1;
                frame1 = sectionSize * i;
                minDistance2 = minDistance;
                minDistance = distance;
            }
            else if (distance < minDistance2)
            {
                frame2 = sectionSize * i;
                minDistance2 = distance;
            }
        }

        if (frame1 > frame2)
        {
            int temp;
            temp = frame1;
            frame1 = frame2;
            frame2 = temp;
        }

        for (int i = frame1; i < frame2; i++)
        {
            float x = recordedReplay[i].position.x - currentPosX;
            float z = recordedReplay[i].position.z - currentPosZ;

            float distance = (float)Math.Sqrt((x * x) + (z * z));

            if (distance < minDistance)
            {
                frame4 = frame3;
                frame3 = i;
                minDistance2 = minDistance;
                minDistance = distance;
            }
            else if (distance < minDistance2)
            {
                frame4 = i;
                minDistance2 = distance;
            }
        }

        float a = recordedReplay[frame3].position.x - recordedReplay[frame4].position.x;
        float b = recordedReplay[frame3].position.z - recordedReplay[frame4].position.z;
        float minDistance3 = (float)Math.Sqrt((a * a) + (b * b));

        float s = (minDistance + minDistance2 + minDistance3) / 2;

        float area = (float)Math.Sqrt(s * (s - minDistance) * (s - minDistance2) * (s - minDistance3));
        height = area * 2 / minDistance3;

        if (showPosition)
        {
            cubeOne.transform.position = recordedReplay[frame3].position;
            cubeTwo.transform.position = recordedReplay[frame4].position;
            cubeCar.transform.position = target.recordedData[currentFrame].position;
        }


        //print("Frame 1: " + frame3 + "   Frame 2: " + frame4);
        //print("Car position: " + target.recordedData[currentFrame].position);
        ////print("Dummy1 position: " + recordedReplay[frame3].position);
        ////print("Dummy2 position: " + recordedReplay[frame4].position);
        //print("Car - Dummy1 distance: " + minDistance);
        //print("Car - Dummy2 distance: " + minDistance2);
        //print("Dummy1 - Dummy2 distance: " + minDistance3);
        //print("Height: " + height);
    }
}
