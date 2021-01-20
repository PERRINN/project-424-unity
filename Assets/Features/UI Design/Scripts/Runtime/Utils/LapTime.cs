using System.Collections;
using System.Collections.Generic;

namespace Perrinn424
{
    public class LapTime : IEnumerable<float>, IEnumerator<float>
    {
        private int SumIndex => sectorCount;
        private readonly float[] sectors;
        public float Sum => sectors[SumIndex];
        private readonly int sectorCount;

        public LapTime(float[] s)
        {
            sectorCount = s.Length;
            this.sectors = new float[sectorCount + 1];

            int i;
            float sum = 0f;
            for (i = 0; i < sectorCount; i++)
            {
                sum += s[i];
                sectors[i] = s[i];
            }
            sectors[SumIndex] = sum;
        }


        public float this[int i] => sectors[i];

        public IEnumerator<float> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        int position = -1;

        public bool MoveNext()
        {
            position++;
            return (position < sectorCount);
        }

        public void Reset()
        {
            position = -1;

        }

        public float Current => sectors[position];

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            position = -1;
        }
    }
}
