using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Perrinn424.PerformanceBenchmarkSystem
{
    public class PerformanceBenchmarkData : ScriptableObject, IReadOnlyList<PerformanceBenchmarkSample>
    {
        public float frenquency;
        public List<PerformanceBenchmarkSample> samples;

        public PerformanceBenchmarkSample this[int index] => samples[index];

        public int Count => samples.Count;

        public IEnumerator<PerformanceBenchmarkSample> GetEnumerator()
        {
            return samples.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
