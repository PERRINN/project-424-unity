using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Perrinn424.Utilities
{
    public class LapTime : IEnumerable<float>
    {
        private int SumIndex => sectorCount;
        private readonly float[] times;
        public float Sum => times[SumIndex];
        public readonly int sectorCount;
        public int TimesCount { get; }
        public int SectorsCompletedIndex { get; private set; }
        public bool IsCompleted => SectorsCompletedIndex == sectorCount;

        public LapTime(int sectorCount, float[] sectorTimes)
        {
            if (sectorTimes.Length > sectorCount)
            {
                string msg = $"The provider array lenght is bigger than sector count. {sectorTimes.Length} > {sectorCount}";
                throw new ArgumentException(msg);
            }

            this.sectorCount = sectorCount;
            this.times = new float[sectorCount + 1];

            int i;
            float sum = 0f;

            for (i = 0; i < sectorTimes.Length; i++)
            {
                sum += sectorTimes[i];
                times[i] = sectorTimes[i];
            }
            SectorsCompletedIndex = i;
            for (i = sectorTimes.Length; i < sectorCount; i++)
            {
                times[i] = Mathf.Infinity;
                sum = Mathf.Infinity;
            }


            times[SumIndex] = sum;
            TimesCount = times.Length;
        }

        public LapTime(float[] s) : this(s.Length, s){}
        public LapTime(int sectorCount) : this(sectorCount, new float[0]){}

        public float this[int index] => times[index];

        public void AddSector(float newTime)
        {
            if(IsCompleted)
                throw new ArgumentException("The lap is completed");

            times[SectorsCompletedIndex++] = newTime;

            RefreshSum();
        }

        public void UpdateSector(int index, float newValue)
        {
            if (times[index] == Mathf.Infinity && newValue != Mathf.Infinity)
            {
                SectorsCompletedIndex++;
            }

            times[index] = newValue;
            RefreshSum();
        }

        private void RefreshSum()
        {
            if (IsCompleted)
            {
                float sum = 0;
                for (int i = 0; i < sectorCount; i++)
                {
                    sum += times[i];
                }

                times[SumIndex] = sum;
            }
        }

        public IEnumerator<float> GetEnumerator()
        {
            IList<float> list = times;
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
