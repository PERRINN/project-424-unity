using System;
using UnityEngine;

namespace Perrinn424.PerformanceBenchmarkSystem
{
    [Serializable]
    public struct PerformanceBenchmarkSample
    {
        public float time; //s
        public float distance; //m
        public float throttle; //[0,1]
        public float brake; //[0,1]
        public float speed; //m/s

        public static PerformanceBenchmarkSample Lerp(PerformanceBenchmarkSample a, PerformanceBenchmarkSample b, float t)
        {
            return new PerformanceBenchmarkSample
            {
                time = Mathf.Lerp(a.time, b.time, t),
                distance = Mathf.Lerp(a.distance, b.distance, t),
                throttle = Mathf.Lerp(a.throttle, b.throttle, t),
                brake = Mathf.Lerp(a.brake, b.brake, t),
                speed = Mathf.Lerp(a.speed, b.speed, t),
            };
        }

        public static PerformanceBenchmarkSample LerpUncampled(PerformanceBenchmarkSample a, PerformanceBenchmarkSample b, float t)
        {
            return new PerformanceBenchmarkSample
            {
                time = Mathf.LerpUnclamped(a.time, b.time, t),
                distance = Mathf.LerpUnclamped(a.distance, b.distance, t),
                throttle = Mathf.LerpUnclamped(a.throttle, b.throttle, t),
                brake = Mathf.LerpUnclamped(a.brake, b.brake, t),
                speed = Mathf.LerpUnclamped(a.speed, b.speed, t),
            };
        }
    } 
}
