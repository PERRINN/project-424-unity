
using EdyCommonTools;
using Project424;
using System;
using System.Collections.Generic;
using VehiclePhysics.UI;
using UnityEngine;
using VehiclePhysics;


public class Autopilot : VehicleBehaviour
{
    // Public component parameters

    public float kp = 600000.0f;
    public float ki = 0.0f;
    public float kd = 20000.0f;
    public float maxForceP = 10000.0f;
    public float maxForceD = 25000.0f;
    public int startUpThrottleSpeedRatio = 60;
    public int startUpThrottle = 70;
    public int startUpBrakeSpeedRatio = 80;

    public float offsetValue = 0.0f;
    public BoxCollider startLine;

    public bool debugGizmo = false;

    // Exposed for Telemetry

    public float Error => height; //[m]
    public float P => edyPID.proportional; //[N]
    public float I => edyPID.integral; //[N]
    public float D => edyPID.derivative; //[N]
    public float PID => edyPID.output; //[N]

    // Private members

    Rigidbody rigidBody424;
    VPReplay replaySystem;
    VPReplayController replayController;
    List<VPReplay.Frame> recordedReplay = new List<VPReplay.Frame>();
    readonly PidController edyPID = new PidController();

    int sectionSize;
    float height = 0, previousHeight = 0;
    Vector3 appliedForceV3;

    Vector3 m_lastPosition;
    float m_totalDistance, m_lastTime;

    int showSteer, showBrake, showThrottle;
    bool autopilotON;
    bool lostControl = false;

    VPDeviceInput m_deviceInput;
    float m_ffbForceIntensity;
    float m_ffbDamperCoefficient;


    public override int GetUpdateOrder ()
    {
        // Execute after input components (0) to override their input
        return 10;
    }


    public override void OnEnableVehicle ()
    {
        rigidBody424 = GetComponent<Rigidbody>();
        replaySystem = GetComponentInChildren<VPReplay>();
        replayController = GetComponentInChildren<VPReplayController>();

        m_lastPosition = rigidBody424.position;
        m_totalDistance = 0;
        m_lastTime = 0;

        // Disable autopilot when no replay data is available
        if (replayController == null || replayController.predefinedReplay == null)
        {
            enabled = false;
            return;
        }

        SteeringScreen.autopilotState = false;
        recordedReplay = replayController.predefinedReplay.recordedData;
        sectionSize = (int)Math.Sqrt(recordedReplay.Count); // Breakdown recorded replay into even sections

        m_deviceInput = vehicle.GetComponentInChildren<VPDeviceInput>();
        if (m_deviceInput != null)
        {
            m_ffbForceIntensity = m_deviceInput.forceIntensity;
            m_ffbDamperCoefficient = m_deviceInput.damperCoefficient;
        }

        // (Edy) ReferenceLapDuplicated makes no sense to me
        /*
        if (ReferenceLapDuplicated())
        {
            enabled = false;
            return;
        }
        */
    }


    public override void UpdateVehicle ()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (autopilotON)
            {
                autopilotON = false;
                SteeringScreen.autopilotState = false;
                if (m_deviceInput != null)
                {
                    m_deviceInput.forceIntensity = m_ffbForceIntensity;
                    m_deviceInput.damperCoefficient = m_ffbDamperCoefficient;
                }
            }
            else
            {
                autopilotON = true;
                SteeringScreen.autopilotState = true;
                if (m_deviceInput != null)
                {
                    m_deviceInput.forceIntensity = 0.0f;
                    m_deviceInput.damperCoefficient = 0.0f;
                }
            }
        }
    }


    public override void FixedUpdateVehicle ()
    {
        // Current Vehicle Position
        int currentFrame = replaySystem.currentFrame;
        float currentPosX = replaySystem.recordedData[currentFrame].position.x;
        float currentPosZ = replaySystem.recordedData[currentFrame].position.z;

        int sectionClosestFrame1 = 0;
        int sectionClosestFrame2 = 0;
        int closestFrame1 = 0;
        int closestFrame2 = 0;

        float closestDisFrame1 = float.MaxValue;
        float closestDisFrame2 = float.MaxValue;

        // Search two closest section frames
        for (int i = 0; i <= sectionSize; i++)
        {
            int recordedFrameNum = (i == sectionSize) ? recordedReplay.Count - sectionSize : sectionSize * i;

            float x = recordedReplay[recordedFrameNum].position.x - currentPosX;
            float z = recordedReplay[recordedFrameNum].position.z - currentPosZ;

            float distanceCalculation = (float)Math.Sqrt((x * x) + (z * z));

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

        // Boundary search conditions
        if (sectionClosestFrame1 == 0 && sectionClosestFrame2 > recordedReplay.Count / 2)
        {
            sectionClosestFrame1 = sectionClosestFrame2;
            sectionClosestFrame2 = recordedReplay.Count - 1;
        }
        else
        if (sectionClosestFrame1 == sectionSize && sectionClosestFrame2 == recordedReplay.Count - sectionSize)
        {
            sectionClosestFrame1 = sectionSize * sectionSize;
        }

        // Reset Distance value
        closestDisFrame1 = float.MaxValue;
        closestDisFrame2 = float.MaxValue;

        // Boundary search conditions
        sectionClosestFrame1 = (sectionClosestFrame1 - sectionSize / 2 <= 0) ? 0 : sectionClosestFrame1 -= sectionSize / 2;
        sectionClosestFrame2 = (sectionClosestFrame2 + sectionSize / 2 >= recordedReplay.Count) ? recordedReplay.Count - 1 : sectionClosestFrame2 += sectionSize / 2;

        // Search two closest frames
        for (int i = sectionClosestFrame1; i <= sectionClosestFrame2; i++)
        {
            float x = recordedReplay[i].position.x - currentPosX;
            float z = recordedReplay[i].position.z - currentPosZ;

            float distanceCalculation = (float)Math.Sqrt((x * x) + (z * z));

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

        // Reference point offset: Recorded vehicle
        Vector3 offsetFromClosestFrame1 = GetOffsetPosition(offsetValue, recordedReplay[closestFrame1]);
        Vector3 offsetFromClosestFrame2 = GetOffsetPosition(offsetValue, recordedReplay[closestFrame2]);
        Vector3 offsetFromCurrentVehiclePos = GetOffsetPosition(offsetValue, replaySystem.recordedData[currentFrame]);

        // get height
        float valueDiffPosX = offsetFromClosestFrame1.x - offsetFromClosestFrame2.x; //recordedReplay[frame3].position.x - recordedReplay[frame4].position.x;
        float valueDiffPosZ = offsetFromClosestFrame1.z - offsetFromClosestFrame2.z; //recordedReplay[frame3].position.z - recordedReplay[frame4].position.z;
        float distanceBetweenTwoFrames = (float)Math.Sqrt((valueDiffPosX * valueDiffPosX) + (valueDiffPosZ * valueDiffPosZ));
        float semiPerimeter = (closestDisFrame1 + closestDisFrame2 + distanceBetweenTwoFrames) / 2;
        float tryCatchArea = semiPerimeter * (semiPerimeter - closestDisFrame1) * (semiPerimeter - closestDisFrame2) * (semiPerimeter - distanceBetweenTwoFrames);

        tryCatchArea = tryCatchArea < 0 ? 0 : tryCatchArea;

        float area = (float)Math.Sqrt(tryCatchArea);
        float checkHeight = area * 2 / distanceBetweenTwoFrames;

        float nextFrameX = recordedReplay[closestFrame2].position.x - currentPosX;
        float nextFrameZ = recordedReplay[closestFrame2].position.z - currentPosZ;
        float nextFrameDistance = (float)Math.Sqrt((nextFrameX * nextFrameX) + (nextFrameZ * nextFrameZ));
        float prograssiveCalculation = (float)Math.Sqrt((nextFrameDistance * nextFrameDistance) - (checkHeight * checkHeight));
        int progressive = (int)((distanceBetweenTwoFrames - prograssiveCalculation) / distanceBetweenTwoFrames * 100);

        float errX = offsetFromClosestFrame1.x - offsetFromCurrentVehiclePos.x; //recordedReplay[frame3].position.x - currentPosX;
        float errXBAL = (offsetFromClosestFrame2.x - offsetFromCurrentVehiclePos.x) - errX;
        float errZ = offsetFromClosestFrame1.z - offsetFromCurrentVehiclePos.z; //recordedReplay[frame3].position.z - currentPosZ;
        float errZBAL = (offsetFromClosestFrame2.z - offsetFromCurrentVehiclePos.z) - errZ;
        float degree = -(float)(Math.PI * recordedReplay[closestFrame1].rotation.eulerAngles.y / 180);
        float degreeERR = -(float)(Math.PI * recordedReplay[closestFrame2].rotation.eulerAngles.y / 180) - degree;
        float cosD = (float)Math.Cos(degree + degreeERR * progressive / 100);
        float sinD = (float)Math.Sin(degree + degreeERR * progressive / 100);
        float carPosX = ((errX + errXBAL * progressive / 100) * cosD) + ((errZ + errZBAL * progressive / 100) * sinD);
        height = (carPosX > 0) ? -checkHeight : checkHeight;

        // Steering screen display
        SteeringScreen.bestTime = replaySystem.FramesToTime(closestFrame1);

        //get error force
        edyPID.SetParameters(Mathf.Min(kp, maxForceP / checkHeight), ki, Mathf.Min(kd, maxForceD * Time.deltaTime / Mathf.Abs(height - previousHeight)));
        edyPID.input = height;
        edyPID.Compute();

        previousHeight = height;

        //errorLimit [m/s]
        appliedForceV3.x = edyPID.output * cosD * 1.000f;
        appliedForceV3.y = 0;
        appliedForceV3.z = edyPID.output * sinD * 1.000f;

        //get recorded driver input
        (closestFrame1, closestFrame2) = GetAsMinMax(closestFrame1, closestFrame2);

        //Car Control System
        float frameAngle = recordedReplay[closestFrame1].rotation.eulerAngles.y;
        float carAngle = rigidBody424.rotation.eulerAngles.y;

        if ((frameAngle - carAngle) < -350) { frameAngle += 360; }
        else if ((frameAngle - carAngle) > 350) { frameAngle -= 360; }
        float carAngleErr = frameAngle - carAngle;
        carAngleErr = carAngleErr == 0 ? 0 : (float)Math.Sqrt(carAngleErr * carAngleErr);

        if (carAngleErr > 30 && carAngleErr < 90) { lostControl = true; }
        else if (carAngleErr >= 90)
        {
            autopilotON = false;
            SteeringScreen.autopilotState = false;
        }
        else { lostControl = false; }

        if (autopilotON)
        {
            rigidBody424.AddForceAtPosition(appliedForceV3, offsetFromCurrentVehiclePos); // transform.position rigidBody424.centerOfMass

            if (debugGizmo)
                {
                DebugUtility.DrawCrossMark(offsetFromCurrentVehiclePos, vehicle.cachedTransform, GColor.pink);
                Debug.DrawLine(offsetFromCurrentVehiclePos, offsetFromCurrentVehiclePos + appliedForceV3 / 1000.0f, GColor.orange);
                }

            if (!lostControl)
            {
                // Steer angle
                int steerERR = recordedReplay[closestFrame2].inputData[InputData.Steer] - recordedReplay[closestFrame1].inputData[InputData.Steer];
                showSteer = (steerERR * progressive / 100) + recordedReplay[closestFrame1].inputData[InputData.Steer];
                vehicle.data.Set(Channel.Input, InputData.Steer, showSteer);

                // Speed check
                float replayTravelingDistance = (recordedReplay[closestFrame2].position - recordedReplay[closestFrame1].position).magnitude;
                float SecondsPerFrame = Time.time - m_lastTime;
                m_lastPosition = rigidBody424.position;
                m_totalDistance += replayTravelingDistance;

                // Brake Control
                int brakeERR = recordedReplay[closestFrame2].inputData[InputData.Brake] - recordedReplay[closestFrame1].inputData[InputData.Brake];
                showBrake = (brakeERR * progressive / 100) + recordedReplay[closestFrame1].inputData[InputData.Brake];

                if (vehicle.data.Get(Channel.Vehicle, VehicleData.Speed) / 1000 < replayTravelingDistance / SecondsPerFrame * startUpBrakeSpeedRatio / 100)   //startup
                {
                    showBrake = 0;
                }
                vehicle.data.Set(Channel.Input, InputData.Brake, showBrake);
                m_lastTime += SecondsPerFrame;

                // Throttle
                int throttleERR = recordedReplay[closestFrame2].inputData[InputData.Throttle] - recordedReplay[closestFrame1].inputData[InputData.Throttle];
                showThrottle = (throttleERR * progressive / 100) + recordedReplay[closestFrame1].inputData[InputData.Throttle];

                if (vehicle.data.Get(Channel.Vehicle, VehicleData.Speed) / 1000 < replayTravelingDistance / SecondsPerFrame * startUpThrottleSpeedRatio / 100)   //startup
                {
                    vehicle.data.Set(Channel.Input, InputData.Throttle, startUpThrottle * 100);
                }
                else
                {
                    vehicle.data.Set(Channel.Input, InputData.Throttle, showThrottle);
                }

                // AutomaticGear
                vehicle.data.Set(Channel.Input, InputData.AutomaticGear, recordedReplay[closestFrame1].inputData[InputData.AutomaticGear]);
            }
        }
    }


    Vector3 GetOffsetPosition(float offsetValue, VPReplay.Frame offsetTransform)
    {
        Vector3 positionOffset;

        float degreeOFFSET = (float)(Math.PI * offsetTransform.rotation.eulerAngles.y / 180);
        float errOffsetZ = offsetValue;
        float cosDOffset = (float)Math.Cos(degreeOFFSET);
        float sinDOffset = (float)Math.Sin(degreeOFFSET);
        float carPosXoffset = errOffsetZ * sinDOffset;
        float carPosZoffset = errOffsetZ * cosDOffset;

        positionOffset.x = carPosXoffset + offsetTransform.position.x;
        positionOffset.y = offsetTransform.position.y;
        positionOffset.z = carPosZoffset + offsetTransform.position.z;

        return positionOffset;
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


    // (Edy) This makes no sense at all to me.
    // It overrides the size of the box collider of the start line to an arbitrary size for
    // checking if more than one position of the replay are inside it.
    /*
    bool ReferenceLapDuplicated()
    {
        if (startLine == null) return false;

        bool duplicated = false;
        int count = 0;

        for (int i = 0; i < recordedReplay.Count; i++)
        {
            startLine.size = new Vector3(1, 1, 0.09f);
            if (startLine.bounds.Contains(recordedReplay[i].position))
            {
                count++;
                if (count > 1) { duplicated = true; }
            }
        }
        startLine.size = new Vector3(1, 1, 1);
        return duplicated;
    }
    */
}
