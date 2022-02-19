using System;
using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    public struct Sample
    {
        public Vector3 positition;
        public Quaternion rotation;

        public int rawSteer;
        public int rawThrottle;
        public int rawBrake;

        public float steeringAngle;
        public float throttle;
        public float brakePressure;

        public int automaticGear;

        public static Sample Lerp(Sample a, Sample b, float t)
        {
            return new Sample
            {

                positition = Vector3.Lerp(a.positition, b.positition, t),
                rotation = Quaternion.Lerp(a.rotation, b.rotation, t),
                rawSteer = IntLerp(a.rawSteer, b.rawSteer, t),
                rawThrottle = IntLerp(a.rawThrottle, b.rawThrottle, t),
                rawBrake = IntLerp(a.rawBrake, b.rawBrake, t),
                steeringAngle = Mathf.Lerp(a.steeringAngle, b.steeringAngle, t),
                throttle = Mathf.Lerp(a.throttle, b.throttle, t),
                brakePressure = Mathf.Lerp(a.brakePressure, b.brakePressure, t),
                automaticGear = a.automaticGear
            };
        }

        private static int IntLerp(int a, int b, float t)
        {
            return (int)Mathf.Lerp(a, b, t);
        }
    } 
}
