﻿using System;
using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    [Serializable]
    public struct Sample
    {
        public float speed;
        public Vector3 position;
        public Quaternion rotation;

        public int gear;
        public float steeringAngle;
        public float throttle;
        public float brakePressure;

        public float drsPosition;


        public static Sample Lerp(Sample a, Sample b, float t)
        {
            return new Sample
            {
                speed = Mathf.Lerp(a.speed, b.speed, t),
                position = Vector3.Lerp(a.position, b.position, t),
                rotation = Quaternion.Lerp(a.rotation, b.rotation, t),
                               
                steeringAngle = Mathf.Lerp(a.steeringAngle, b.steeringAngle, t),
                throttle = Mathf.Lerp(a.throttle, b.throttle, t),
                brakePressure = Mathf.Lerp(a.brakePressure, b.brakePressure, t),
                gear = a.gear,

                drsPosition = Mathf.Lerp(a.drsPosition, b.drsPosition, t),
            };
        }

        public static Sample LerpUncampled(Sample a, Sample b, float t)
        {
            return new Sample
            {
                speed = Mathf.LerpUnclamped(a.speed, b.speed, t),
                position = Vector3.LerpUnclamped(a.position, b.position, t),
                rotation = Quaternion.LerpUnclamped(a.rotation, b.rotation, t),
                               
                steeringAngle = Mathf.LerpUnclamped(a.steeringAngle, b.steeringAngle, t),
                throttle = Mathf.LerpUnclamped(a.throttle, b.throttle, t),
                brakePressure = Mathf.LerpUnclamped(a.brakePressure, b.brakePressure, t),
                gear = a.gear,
                
                drsPosition = Mathf.LerpUnclamped(a.drsPosition, b.drsPosition, t),
            };
        }
    } 
}
