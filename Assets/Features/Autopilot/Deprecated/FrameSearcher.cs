using System.Collections.Generic;
using UnityEngine;
using VehiclePhysics;

namespace Perrinn424.AutopilotSystem
{
 
    /// <summary>
    /// Frame searcher used in the previous implementation. It returns buggy results, so it will be removed
    /// </summary>
    public class FrameSearcher : IFrameSearcher
    {

        private readonly IReadOnlyList<VPReplay.Frame> frames;
        public FrameSearcher(IReadOnlyList<VPReplay.Frame> frames)
        {
            this.frames = frames;
        }

        public int ClosestFrame1 { get; private set; }
        public int ClosestFrame2 { get; private set; }
        public float ClosestDisFrame1 { get; private set; }
        public float ClosestDisFrame2 { get; private set; }
        int sectionSize;
        public int count;
        public void Search(Transform t)
        {
            count = 0;
            Vector3 position = t.position;
            float currentPosX = position.x;
            float currentPosZ = position.z;

            int sectionClosestFrame1 = 0;
            int sectionClosestFrame2 = 0;
            ClosestFrame1 = 0;
            ClosestFrame2 = 0;

            ClosestDisFrame1 = float.MaxValue;
            ClosestDisFrame2 = float.MaxValue;

            sectionSize = (int)Mathf.Sqrt(frames.Count);
            // Search two closest section frames
            for (int i = 0; i <= sectionSize; i++)
            {
                count++;
                //int recordedFrameNum = (i == sectionSize) ? frames.Count - sectionSize : sectionSize * i;
                int recordedFrameNum = (i == sectionSize) ? frames.Count - sectionSize : sectionSize * i;

                if (i == sectionSize)
                {
                    recordedFrameNum = frames.Count - sectionSize;
                }
                else
                {
                    recordedFrameNum = sectionSize * i;
                }

                float x = frames[recordedFrameNum].position.x - currentPosX;
                float z = frames[recordedFrameNum].position.z - currentPosZ;

                float distanceCalculation = (float)Mathf.Sqrt((x * x) + (z * z));

                if (distanceCalculation < ClosestDisFrame1)
                {
                    sectionClosestFrame2 = sectionClosestFrame1;
                    sectionClosestFrame1 = recordedFrameNum;
                    ClosestDisFrame2 = ClosestDisFrame1;
                    ClosestDisFrame1 = distanceCalculation;
                }
                else if (distanceCalculation < ClosestDisFrame2)
                {
                    sectionClosestFrame1 = recordedFrameNum;
                    ClosestDisFrame2 = distanceCalculation;
                }
            }

            (sectionClosestFrame1, sectionClosestFrame2) = GetAsMinMax(sectionClosestFrame1, sectionClosestFrame2);

            //Boundary search conditions
            if (sectionClosestFrame1 == 0 && sectionClosestFrame2 > frames.Count / 2)
            {
                sectionClosestFrame1 = sectionClosestFrame2;
                sectionClosestFrame2 = frames.Count - 1;
            }
            else
            if (sectionClosestFrame1 == sectionSize && sectionClosestFrame2 == frames.Count - sectionSize)
            {
                sectionClosestFrame1 = sectionSize * sectionSize;
            }

            // Reset Distance value
            ClosestDisFrame1 = float.MaxValue;
            ClosestDisFrame2 = float.MaxValue;

            // Boundary search conditions
            sectionClosestFrame1 = (sectionClosestFrame1 - sectionSize / 2 <= 0) ? 0 : sectionClosestFrame1 -= sectionSize / 2;
            sectionClosestFrame2 = (sectionClosestFrame2 + sectionSize / 2 >= frames.Count) ? frames.Count - 1 : sectionClosestFrame2 += sectionSize / 2;

            // Search two closest frames
            for (int i = sectionClosestFrame1; i <= sectionClosestFrame2; i++)
            {
                count++;
                float x = frames[i].position.x - currentPosX;
                float z = frames[i].position.z - currentPosZ;

                float distanceCalculation = (float)Mathf.Sqrt((x * x) + (z * z));

                if (distanceCalculation < ClosestDisFrame1)
                {
                    ClosestFrame2 = ClosestFrame1;
                    ClosestFrame1 = i;
                    ClosestDisFrame2 = ClosestDisFrame1;
                    ClosestDisFrame1 = distanceCalculation;
                }
                else if (distanceCalculation < ClosestDisFrame2)
                {
                    ClosestFrame2 = i;
                    ClosestDisFrame2 = distanceCalculation;
                }
            }

            //new code
            for (int i = 0; i < 60; i++)
            {
                float x = frames[i].position.x - currentPosX;
                float z = frames[i].position.z - currentPosZ;

                float distanceCalculation = (float)Mathf.Sqrt((x * x) + (z * z));

                if (distanceCalculation < ClosestDisFrame1)
                {
                    ClosestFrame2 = ClosestFrame1;
                    ClosestFrame1 = i;
                    ClosestDisFrame2 = ClosestDisFrame1;
                    ClosestDisFrame1 = distanceCalculation;
                }
                else if (distanceCalculation < ClosestDisFrame2)
                {
                    ClosestFrame2 = i;
                    ClosestDisFrame2 = distanceCalculation;
                }
            }

            if (ClosestFrame1 == ClosestFrame2)
            {
                Debug.LogError($"closest are equal {ClosestFrame1}");
                ClosestFrame2 = ClosestFrame1 + 1;

                float x = frames[ClosestFrame2].position.x - currentPosX;
                float z = frames[ClosestFrame2].position.z - currentPosZ;
                ClosestDisFrame2 = (float)Mathf.Sqrt((x * x) + (z * z));
            }

            (ClosestFrame1, ClosestFrame2) = GetAsMinMax(ClosestFrame1, ClosestFrame2);
            ClosestDisFrame1 = Distance2D(frames[ClosestFrame1].position, position);
            ClosestDisFrame2 = Distance2D(frames[ClosestFrame2].position, position);
            //UnityEngine.Debug.Log($"Count: {count}");

        }

        private float Distance2D(Vector3 a, Vector3 b)
        {
            float x = a.x - b.x;
            float z = a.z - b.z;
            return (float)Mathf.Sqrt((x * x) + (z * z));
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
    } 
}
