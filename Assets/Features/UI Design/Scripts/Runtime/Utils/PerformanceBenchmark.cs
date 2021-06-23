using System;
using UnityEngine;

namespace Perrinn424
{
    public class PerformanceBenchmark
    {
        internal readonly float[] time;
        internal readonly float[] distance;
        private readonly int count;
        private int previousIndex = -1;

        public float Speed { get; private set; }

        public PerformanceBenchmark(int[] reference)
        {
            count = reference.Length;
            time = new float[count];
            distance = new float[count];

            for (int i = 0; i < reference.Length; i++)
            {
                time[i] = i;
                distance[i] = reference[i];
            }
        }

        public float LapDiff(float currentTime, float currentDistance)
        {
            int index = FindIndex(currentDistance);

            if (index < 0 || index + 1 >= count)
                return float.NaN;

            previousIndex = index;
            float ratio = (currentDistance - distance[index]) / (distance[index + 1] - distance[index]);
            float referenceTime = Mathf.Lerp(time[index], time[index + 1], ratio);
            float diff = currentTime - referenceTime;

            Speed = CalculateSpeed(index, ratio);
            return diff;
        }

        private int FindIndex(float currentDistance)
        {
            // The most probably is that the last index is still the correct index
            if (IsCorrectIndex(previousIndex, currentDistance))
            {
                return previousIndex;
            }

            // If not, probably is the next one
            if(IsCorrectIndex(previousIndex + 1, currentDistance))
            {
                return previousIndex + 1;
            }

            // We can use binary search because distance is sorted and it is much faster
            int binaryIndex = Array.BinarySearch(distance, currentDistance);
            if (binaryIndex < 0)
            {
                //The return index is complement of the first element larger than currentDistance
                binaryIndex = ~binaryIndex;
                if (binaryIndex > count)
                    binaryIndex = - 1; //not found

                return binaryIndex -1; //the return index is the first element larger, but we need the previous one
            }

            return binaryIndex;
        }

        private float CalculateSpeed(int index, float ratio)
        {
            float speed = CalculateSpeed(index);
            float nextSpeed = CalculateSpeed(index + 1);
            return Mathf.Lerp(speed, nextSpeed, ratio);
        }

        private float CalculateSpeed(int index)
        {
            float currentDistance = distance[index];
            float previousDistance = index == 0 ? 0 : distance[index - 1];
            return currentDistance - previousDistance;
        }

        internal bool IsCorrectIndex(int index, float d)
        {
            if (index < 0 || index + 1 >= count)
                return false;

            return distance[index] < d && distance[index + 1] > d;
        }
    } 
}