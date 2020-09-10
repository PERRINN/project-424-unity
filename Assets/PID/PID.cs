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
    public bool autoUpdate = false;

    List<VPReplay.Frame> recordedReplay = new List<VPReplay.Frame>();
    int count;

    // Start is called before the first frame update
    void Start()
    {
        recordedReplay = replayController.predefinedReplay.recordedData;
        //count = recordedReplay.Count;
    }

    // Update is called once per frame
    void Update()
    {
        if (autoUpdate) { getDistance(); }
        
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(216, Screen.height - 50, 150, 20), "GET DISTANCE"))
        {
            getDistance();
        }
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
            //print(sectionSize * i);

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
        float height = area * 2 / minDistance3;

        cubeOne.transform.position = recordedReplay[frame3].position;
        cubeTwo.transform.position = recordedReplay[frame4].position;
        cubeCar.transform.position = target.recordedData[currentFrame].position;



        print("Frame 1: " + frame3 + "   Frame 2: " + frame4);
        print("Car position: " + target.recordedData[currentFrame].position);
        //print("Dummy1 position: " + recordedReplay[frame3].position);
        //print("Dummy2 position: " + recordedReplay[frame4].position);
        print("Car - Dummy1 distance: " + minDistance);
        print("Car - Dummy2 distance: " + minDistance2);
        print("Dummy1 - Dummy2 distance: " + minDistance3);
        print("Height: " + height);
    }
}
