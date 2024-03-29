using VehiclePhysics;
using UnityEngine;
using System;


public class Perrinn424SpeedLimiter : VehicleBehaviour
{

    [System.Serializable]
    public class SpeedLimiterArray
    {
        public float segmentStart = 0.0f;
        public float segmentEnd = 500.0f;
        // set range for minimum limiter -- 0 to 1
        [Range(0, 1)]
        public float minimumLimiter = 1.0f;
    }

    // Emit telemetry checkbox
    public bool emitTelemetry = true;

    // Speed limiter segments configuration
    [Space(5)]
    [SerializeField] private SpeedLimiterArray[] speedLimiterSegments;

    [HideInInspector] public float limiterValue = 1.0f;
    [HideInInspector] public float limiterEnabled = 0.0f;

    // Private members
    Perrinn424CarController m_vehicle;


    float getLimiterValue(float lapDistance)
    {
        for (int i = 0; i < speedLimiterSegments.Length; i++)
        {
            if (lapDistance > speedLimiterSegments[i].segmentStart && lapDistance < speedLimiterSegments[i].segmentEnd)
                return speedLimiterSegments[i].minimumLimiter;
        }
        return 1.0f;
    }


    public override void OnEnableVehicle()
        {
        m_vehicle = vehicle as Perrinn424CarController;
        }


    public override void FixedUpdateVehicle()
    {
        // Getting traveled distance in current lap
        Telemetry.DataRow telemetryDataRow = vehicle.telemetry.latest;
        float distance = (float)telemetryDataRow.distance;

        // check if car is in limited power segment and return the limiter value
        limiterValue = getLimiterValue(distance);

        // check if limiter is enabled
        limiterEnabled = 0.0f;
        if (limiterValue < 1.0f)
            limiterEnabled = 1.0f;

        // Apply limiter to the vehicle
        m_vehicle.mguLimiter = limiterValue;
    }


    // Telemetry
    public override bool EmitTelemetry()
    {
        return emitTelemetry;
    }


    public override void RegisterTelemetry()
    {
        vehicle.telemetry.Register<SpeedLimiterTelemetry>(this);
    }


    public override void UnregisterTelemetry()
    {
        vehicle.telemetry.Unregister<SpeedLimiterTelemetry>(this);
    }


    public class SpeedLimiterTelemetry : Telemetry.ChannelGroup
    {
        public override int GetChannelCount()
        {
            return 2;
        }


        public override Telemetry.PollFrequency GetPollFrequency()
        {
            return Telemetry.PollFrequency.Normal;
        }


        public override void GetChannelInfo (Telemetry.ChannelInfo[] channelInfo, UnityEngine.Object instance)
        {
            // Fill-in channel information

            channelInfo[0].SetNameAndSemantic("SpeedLimiterActive", Telemetry.Semantic.Ratio);
            channelInfo[1].SetNameAndSemantic("SpeedLimiter", Telemetry.Semantic.Ratio);
        }


        public override void PollValues(float[] values, int index, UnityEngine.Object instance)
        {
            Perrinn424SpeedLimiter speedLimiter = instance as Perrinn424SpeedLimiter;

            values[index + 0] = speedLimiter.limiterEnabled;
            values[index + 1] = speedLimiter.limiterValue;

        }
    }

}
