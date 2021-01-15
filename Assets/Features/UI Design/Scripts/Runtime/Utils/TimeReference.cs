using System;
using UnityEngine;

namespace Perrinn424
{
    public class TimeReference
    {
        private readonly float[] time;
        private readonly float[] distance;
        private readonly int count;

        public TimeReference(int[] reference)
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

            if(index == -1)
                return float.NaN;

            float ration = (currentDistance - distance[index]) / (distance[index + 1] - distance[index]);
            float referenceTime = Mathf.Lerp(time[index], time[index + 1], ration);
            float diff = currentTime - referenceTime;
            return diff;
        }

        private int FindIndex(float currentDistance)
        {
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
    } 
}