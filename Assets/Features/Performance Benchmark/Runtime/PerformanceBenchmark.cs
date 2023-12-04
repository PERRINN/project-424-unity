using System;
using UnityEngine;

namespace Perrinn424.PerformanceBenchmarkSystem
{
    public class PerformanceBenchmark: IPerformanceBenchmark
    {
        public readonly float[] distance;
        private readonly int count;
        public int PreviousIndex { get; private set; }
        private readonly float period;

        public float Time { get; private set; } //[s]
        public float TimeDiff { get; private set; } //[s]
        public float Speed { get; private set; } //[m/s]
        public float TraveledDistance { get; private set; } //[m]

        public float Throttle => 0f;

        public float Brake => 0f;

        public PerformanceBenchmark(float[] reference, float frequency)
        {
            count = reference.Length;
            distance = new float[count];
            period = 1f / frequency;

            for (int i = 0; i < reference.Length; i++)
            {
                distance[i] = reference[i];
            }
        }

        public void Update(float currentTime, float currentDistance)
        {
            int index = FindIndex(currentDistance);

            if (index < 0 || index + 1 >= count)
            {
                TimeDiff = float.NaN;
                TraveledDistance = float.NaN;
                Speed = float.NaN;
                return;
            }
            PreviousIndex = index;
            
            float ratio = Mathf.InverseLerp(distance[index], distance[index + 1], currentDistance);
            
            //index represents time, so index == m means [m * period seconds]
            Time = Mathf.Lerp(index, index + 1, ratio)*period;
            TimeDiff = currentTime - Time;

            TraveledDistance = Mathf.Lerp(distance[index], distance[index + 1], ratio);


            Speed = CalculateSpeed(index, ratio);
        }

        private int FindIndex(float currentDistance)
        {
            // The most probably is that the last index is still the correct index
            if (IsCorrectIndex(PreviousIndex, currentDistance))
            {
                return PreviousIndex;
            }

            // If not, probably is the next one
            if(IsCorrectIndex(PreviousIndex + 1, currentDistance))
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
            int binaryIndex = Array.BinarySearch(distance, currentDistance);
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

        public float CalculateSpeed(int index, float ratio)
        {
            float speed = CalculateSpeed(index);
            float nextSpeed = CalculateSpeed(index + 1);
            return Mathf.Lerp(speed, nextSpeed, ratio);
        }

        internal float CalculateSpeed(int index)
        {
            float currentDistance = distance[index];
            float previousDistance = index == 0 ? 0 : distance[index - 1];
            return (currentDistance - previousDistance)/period;
        }

        public bool IsCorrectIndex(int index, float d)
        {
            if (index < 0 || index + 1 >= count)
                return false;

            return distance[index] <= d && distance[index + 1] > d;
        }
    } 
}