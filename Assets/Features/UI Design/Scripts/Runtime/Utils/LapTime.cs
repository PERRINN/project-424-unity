using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Perrinn424
{
    public class LapTime : IEnumerable<float>
    {
        private int SumIndex => sectorCount;
        private readonly float[] times;
        public float Sum => times[SumIndex];
        public readonly int sectorCount;
        public int TimesCount { get; }
        public bool IsCompleted => pointer == sectorCount;

        private int pointer;
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
            pointer = i;
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

        public float this[int i] => times[i];


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
