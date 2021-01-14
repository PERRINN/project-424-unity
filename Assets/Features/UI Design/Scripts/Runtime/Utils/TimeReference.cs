using UnityEngine;

namespace Perrinn424
{
    public class TimeReference
    {
        public float[] time;
        public float[] distance;

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
            for (int i = 0; i < count - 1; i++)
            {
                if (distance[i] < currentDistance && currentDistance < distance[i + 1])
                {
                    float ration = (currentDistance - distance[i]) / (distance[i + 1] - distance[i]);
                    float referenceTime = Mathf.Lerp(time[i], time[i + 1], ration);
                    float diff = currentTime - referenceTime;
                    return diff;
                }
            }

            return float.NaN;
        }
    } 
}