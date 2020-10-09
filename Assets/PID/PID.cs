using EdyCommonTools;
using Project424;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VehiclePhysics;

public class PID : MonoBehaviour
{
    public Rigidbody rigidBody424;
    public VehicleBase vehicleBase;
    public VPReplay target;
    public GameObject cubeOne;
    public GameObject cubeTwo;
    public GameObject cubeCar;
    public VPReplayController replayController;
    public bool showPosition = true;

    List<VPReplay.Frame> recordedReplay = new List<VPReplay.Frame>();
    readonly PidController edyPID = new PidController();
    public float kp, ki, kd;
    public float maxForce;
    public bool autopilotON = false;
    public int throttleControl = 100;
    public int brakeControl = 100;

    int cuts;
    float height = 0;
    Vector3 appliedForceV3;

    int showSteer, showBrake, showThrottle;
    int frame1;
    int frame2;
    bool runOnce = false;

    VPDeviceInput m_deviceInput;
    float m_ffbForceIntensity;
    float m_ffbDamperCoefficient;

    void OnEnable()
    {
        recordedReplay = replayController.predefinedReplay.recordedData;
        cuts = recordedReplay.Count / 500;

        m_deviceInput = vehicleBase.GetComponentInChildren<VPDeviceInput>();
        if (m_deviceInput != null)
        {
            m_ffbForceIntensity = m_deviceInput.forceIntensity;
            m_ffbDamperCoefficient = m_deviceInput.damperCoefficient;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (autopilotON)
            {
                autopilotON = false;
                if (m_deviceInput != null)
                {
                    m_deviceInput.forceIntensity = m_ffbForceIntensity;
                    m_deviceInput.damperCoefficient = m_ffbDamperCoefficient;
                }
            }
            else
            {
                autopilotON = true;
                edyPID.Reset();

                if (m_deviceInput != null)
                {
                    m_deviceInput.forceIntensity = 0.0f;
                    m_deviceInput.damperCoefficient = 0.0f;
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (runOnce == false)
        {
            frame1 = AutopilotOnStart().Item1;
            frame2 = AutopilotOnStart().Item2;
            runOnce = true;
        }

        GetDistance();
    }

    void OnGUI()
    {
        GUIStyle styleON = new GUIStyle(GUI.skin.button);

        if (autopilotON) { styleON.normal.textColor = Color.green; }
        else { styleON.normal.textColor = Color.red; }

        string errorDistance = "";
        string forceX = "";
        string forceZ = "";
        errorDistance += height;
        forceX += appliedForceV3.x;
        forceZ += appliedForceV3.z;

        GUI.Box(new Rect(185, Screen.height - 90, 300, 80), "");
        GUI.Button(new Rect(200, Screen.height - 85, 40, 20), "ON", styleON);
        GUI.Label(new Rect(200, Screen.height - 65, 200, 50), "Error    : " + errorDistance);
        GUI.Label(new Rect(200, Screen.height - 50, 200, 50), "Force X: " + forceX);
        GUI.Label(new Rect(200, Screen.height - 35, 200, 50), "Force Z: " + forceZ);

        GUI.Label(new Rect(350, Screen.height - 65, 200, 50), "Steer    : " + showSteer);
        GUI.Label(new Rect(350, Screen.height - 50, 200, 50), "Brake    : " + showBrake * brakeControl / 100);
        GUI.Label(new Rect(350, Screen.height - 35, 200, 50), "Throttle : " + showThrottle * throttleControl / 100);
    }

    (int, int) AutopilotOnStart()
    {
        int currentFrame = target.recordedData.Count - 1;
        float currentPosX = target.recordedData[currentFrame].position.x;
        float currentPosZ = target.recordedData[currentFrame].position.z;

        int sectionSize = recordedReplay.Count / cuts;

        int frame1 = 0;
        int frame2 = 0;
        int frame3 = 0;
        int frame4 = 0;

        float minDistance1 = float.MaxValue;
        float minDistance2 = float.MaxValue;

        for (int i = 0; i < cuts; i++)
        {
            float x = recordedReplay[sectionSize * i].position.x - currentPosX;
            float z = recordedReplay[sectionSize * i].position.z - currentPosZ;

            float distance = (float)Math.Sqrt((x * x) + (z * z));

            if (distance < minDistance1)
            {
                frame2 = frame1;
                frame1 = sectionSize * i;
                minDistance2 = minDistance1;
                minDistance1 = distance;
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

        minDistance1 = float.MaxValue;
        minDistance2 = float.MaxValue;

        frame1 = (frame1 - 50 <= 0) ? 0 : frame1 -= 50;
        frame2 = (frame2 + 100 >= recordedReplay.Count - 1) ? recordedReplay.Count - 1 : frame2 += 100;

        for (int i = frame1; i <= frame2; i++)
        {
            float x = recordedReplay[i].position.x - currentPosX;
            float z = recordedReplay[i].position.z - currentPosZ;

            float distance = (float)Math.Sqrt((x * x) + (z * z));

            if (distance < minDistance1)
            {
                frame4 = frame3;
                frame3 = i;
                minDistance2 = minDistance1;
                minDistance1 = distance;
            }
            else if (distance < minDistance2)
            {
                frame4 = i;
                minDistance2 = distance;
            }
        }

        return (frame3, frame4);
    }

    void GetDistance()
    {
        int currentFrame = target.recordedData.Count - 1;
        float currentPosX = target.recordedData[currentFrame].position.x;
        float currentPosZ = target.recordedData[currentFrame].position.z;

        float minDistance1 = float.MaxValue;
        float minDistance2 = float.MaxValue;

        int frame3 = 0;
        int frame4 = 0;

        if (frame2 + 2 > recordedReplay.Count - 1)
        {
            frame2 = recordedReplay.Count - 1;
            if (frame2 == recordedReplay.Count - 1)
            {
                frame1 = AutopilotOnStart().Item1;
                frame2 = AutopilotOnStart().Item2;
            }
        }

        for (int i = frame1 - 2; i <= frame2 + 2; i++)
        {
            float x = recordedReplay[i].position.x - currentPosX;
            float z = recordedReplay[i].position.z - currentPosZ;

            float distance = (float)Math.Sqrt((x * x) + (z * z));

            if (distance < minDistance1)
            {
                frame4 = frame3;
                frame3 = i;
                minDistance2 = minDistance1;
                minDistance1 = distance;
            }
            else if (distance < minDistance2)
            {
                frame4 = i;
                minDistance2 = distance;
            }
        }

        // get height
        float a = recordedReplay[frame3].position.x - recordedReplay[frame4].position.x;
        float b = recordedReplay[frame3].position.z - recordedReplay[frame4].position.z;
        float minDistance3 = (float)Math.Sqrt((a * a) + (b * b));
        float s = (minDistance1 + minDistance2 + minDistance3) / 2;
        float chkcal = s * (s - minDistance1) * (s - minDistance2) * (s - minDistance3);

        chkcal = chkcal < 0 ? 0 : chkcal;

        float area = (float)Math.Sqrt(chkcal);
        float errX = recordedReplay[frame3].position.x - currentPosX;
        float errZ = recordedReplay[frame3].position.z - currentPosZ;
        float degree = -(float)(Math.PI * recordedReplay[frame3].rotation.eulerAngles.y / 180);
        float cosD = (float)Math.Cos(degree);
        float sinD = (float)Math.Sin(degree);
        float carPosX = (errX * cosD) + (errZ * sinD);

        float checkHeight = area * 2 / minDistance3;
        height = (carPosX > 0) ? -checkHeight : checkHeight;

        PIDChart.errorDistance = height;
        PIDChart.proportional = ClampByOutput(edyPID.proportional);
        PIDChart.integral = ClampByOutput(edyPID.integral);
        PIDChart.derivative = ClampByOutput(edyPID.derivative);
        PIDChart.output = ClampByOutput(edyPID.output);

        if (showPosition)
        {
            cubeOne.transform.position = recordedReplay[frame3].position;
            cubeTwo.transform.position = recordedReplay[frame4].position;
            cubeCar.transform.position = target.recordedData[currentFrame].position;
        }


        //get error force
        edyPID.minOutput = maxForce * -1.0f;
        edyPID.maxOutput = maxForce * 1.0f;
        edyPID.SetParameters(kp, ki, kd);
        edyPID.input = height;
        edyPID.Compute();


        appliedForceV3.x = ClampByOutput(edyPID.output * cosD * 1.000f);
        appliedForceV3.y = 0;
        appliedForceV3.z = ClampByOutput(edyPID.output * sinD * 1.000f);


        //get recorded driver input
        if (frame3 > frame4)
        {
            int temp;
            temp = frame3;
            frame3 = frame4;
            frame4 = temp;
        }

        if (autopilotON)
        {
            rigidBody424.AddForceAtPosition(appliedForceV3, transform.position);

            float nextFrameX = recordedReplay[frame4].position.x - currentPosX;
            float nextFrameZ = recordedReplay[frame4].position.z - currentPosZ;
            float nextFrameDistance = (float)Math.Sqrt((nextFrameX * nextFrameX) + (nextFrameZ * nextFrameZ));
            float abc = (float)Math.Sqrt((nextFrameDistance * nextFrameDistance) - (checkHeight * checkHeight));
            int progressive = (int)((minDistance3 - abc) / minDistance3 * 100);

            // Steer angle
            int steerERR = recordedReplay[frame4].inputData[InputData.Steer] - recordedReplay[frame3].inputData[InputData.Steer];
            showSteer = (steerERR * progressive / 100) + recordedReplay[frame3].inputData[InputData.Steer];
            vehicleBase.data.Set(Channel.Input, InputData.Steer, showSteer);

            // Brake
            int brakeERR = recordedReplay[frame4].inputData[InputData.Brake] - recordedReplay[frame3].inputData[InputData.Brake];
            showBrake = (brakeERR * progressive / 100) + recordedReplay[frame3].inputData[InputData.Brake];
            vehicleBase.data.Set(Channel.Input, InputData.Brake, showBrake * brakeControl / 100);

            // Throttle
            int throttleERR = recordedReplay[frame4].inputData[InputData.Throttle] - recordedReplay[frame3].inputData[InputData.Throttle];
            showThrottle = (throttleERR * progressive / 100) + recordedReplay[frame3].inputData[InputData.Throttle];
            vehicleBase.data.Set(Channel.Input, InputData.Throttle, showThrottle * throttleControl / 100);

            // AutomaticGear
            vehicleBase.data.Set(Channel.Input, InputData.AutomaticGear, recordedReplay[frame3].inputData[InputData.AutomaticGear]);

        }

        if (frame4 > frame2)
        {
            frame1 += 1;
            frame2 += 1;
        }
        else if (frame3 < frame1)
        {
            frame1 -= 1;
            frame2 -= 1;
        }
    }

    float ClampByOutput(float value)
    {
        float clampedForce = Mathf.Clamp(value, -maxForce, maxForce);
        return clampedForce;
    }
}