using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using VehiclePhysics;

namespace Perrinn424.PerformanceBenchmarkSystem
{
    public class PerformanceBenchmark : IPerformanceBenchmark
    {
        public float Time { get; private set; } //[s]
        public float TimeDiff { get; private set; } //[s]
        public float Speed { get; private set; } //[m/s]
        public float TraveledDistance { get; private set; } //[m]
        public float Throttle { get; private set; } //[0,1]
        public float Brake { get; private set; } //[0,1]

        public int PreviousIndex { get; private set; }

        private readonly int count;
        private readonly List<PerformanceBenchmarkSample> samples;

        private SampleComparer sampleComparer;

        private PerformanceBenchmarkSample currentSample;
        private PerformanceBenchmarkSample nextSample;

        public PerformanceBenchmark(List<PerformanceBenchmarkSample> samples)
        { 
            this.count = samples.Count;
            this.samples = samples;
            this.sampleComparer = new SampleComparer();
        }

        public void Update(float currentTime, float currentDistance)
        {
            if(!IsBetween(currentSample.distance, nextSample.distance, currentDistance)) //do samples need to be refreshed?
            {
                int index = FindIndex(currentDistance);

                if (index < 0 || index + 1 >= count)
                {
                    SetErrorValues();
                    return;
                }


                //Refresh samples
                PreviousIndex = index;
                currentSample = samples[index];
                nextSample = samples[index + 1];
            }

            SetValues(currentSample, nextSample, currentTime, currentDistance);
        }

        public void SetValues(PerformanceBenchmarkSample currentSample, PerformanceBenchmarkSample nextSample, float currentTime, float currentDistance)
        {
            float ratio = Mathf.InverseLerp(currentSample.distance, nextSample.distance, currentDistance);

            PerformanceBenchmarkSample interpolatedSample = PerformanceBenchmarkSample.Lerp(currentSample, nextSample, ratio);
            Time = interpolatedSample.time;
            TimeDiff = currentTime - Time;
            TraveledDistance = interpolatedSample.distance;
            Speed = interpolatedSample.speed;
            Throttle = interpolatedSample.throttle;
            Brake = interpolatedSample.brake;
        }

        private int FindIndex(float currentDistance)
        {
            // The most probably is that the last index is still the correct index
            if (IsCorrectIndex(PreviousIndex, currentDistance))
            {
                return PreviousIndex;
            }

            // If not, probably is the next one
            if (IsCorrectIndex(PreviousIndex + 1, currentDistance))
            {
                return PreviousIndex + 1;
            }

            if (IsCorrectIndex(0, currentDistance)) //maybe new lap
            {
                return 0;
            }


            // We can use binary search because distance is sorted and it is much faster
            int binaryIndex = BinarySearch(currentDistance);

            //Debug.LogError($"Index not found! Previous was {PreviousIndex} and the real one was {binaryIndex}");

            return binaryIndex;
        }

        public int BinarySearch(float currentDistance)
        {
            int binaryIndex = samples.BinarySearch(new PerformanceBenchmarkSample { distance = currentDistance }, sampleComparer);
            if (binaryIndex < 0)
            {
                //The return index is complement of the first element larger than currentDistance
                binaryIndex = ~binaryIndex;
                if (binaryIndex > count)
                    binaryIndex = -1; //not found

                return binaryIndex - 1; //the return index is the first element larger, but we need the previous one
            }

            return binaryIndex;
        }

        public bool IsCorrectIndex(int index, float distance)
        {
            if (index < 0 || index + 1 >= count)
                return false;

            return IsBetween(samples[index].distance, samples[index + 1].distance, distance);
        }

        private bool IsBetween(float lower, float upper, float value) // [lower, upper)
        {
            return lower <= value && upper > value;
        }
        private void SetErrorValues()
        {
            Time = TimeDiff = Speed = TraveledDistance = Throttle = Brake = float.NaN;
        }

        private class SampleComparer : IComparer<PerformanceBenchmarkSample>
        {
            public int Compare(PerformanceBenchmarkSample x, PerformanceBenchmarkSample y)
            {
                return x.distance.CompareTo(y.distance);
            }
        }
    }
}
