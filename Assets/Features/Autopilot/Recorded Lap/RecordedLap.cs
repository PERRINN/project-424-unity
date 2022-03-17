using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    public class RecordedLap : ScriptableObject, IReadOnlyList<Sample>
    {
        public float lapTime;
        public float frequency;
        public List<Sample> samples;

        public Sample this[int index] => samples[index];

        public int Count => samples.Count;

        public IEnumerator<Sample> GetEnumerator()
        {
            return samples.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    } 
}
