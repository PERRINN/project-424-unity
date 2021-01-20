using System;
using System.Collections;
using System.Linq;

namespace Perrinn424
{
    public class LapTime : IEnumerable
    {
        private readonly float[] sectors;
        public float Total { get; }

        public LapTime(float[] s)
        {
            this.sectors = new float[s.Length];
            Array.Copy(s, sectors, s.Length);
            Total = s.Sum();
        }

        public IEnumerator GetEnumerator()
        {
            return sectors.GetEnumerator();
        }

        public float this[int i] => sectors[i];
    } 
}
