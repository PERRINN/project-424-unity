using System.Collections;
using System.Collections.Generic;

namespace Perrinn424
{
    public class LapTime : IEnumerable<float>
    {
        private int SumIndex => sectorCount;
        private readonly float[] times;
        public float Sum => times[SumIndex];
        public readonly int sectorCount;

        public LapTime(float[] s)
        {
            sectorCount = s.Length;
            this.times = new float[sectorCount + 1];

            int i;
            float sum = 0f;
            for (i = 0; i < sectorCount; i++)
            {
                sum += s[i];
                times[i] = s[i];
            }
            times[SumIndex] = sum;
        }


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
