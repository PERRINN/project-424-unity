using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    public class AutopilotProviderExperimental : IAutopilotProvider, IReadOnlyList<Vector3>
    {
        private readonly IReadOnlyList<Sample> samples;
        private NearestSegmentSearcher segmentSearcher;

        public AutopilotProviderExperimental(IReadOnlyList<Sample> samples)
        {
            this.samples = samples;
            segmentSearcher = new NearestSegmentSearcher(this);
        }

        Vector3 IReadOnlyList<Vector3>.this[int index] => this[index].position;

        public Sample this[int index] => samples[index];


        public int Count => samples.Count;

        public Sample this[Vector3 position]
        {
            get
            {
                segmentSearcher.Search(position);
                Sample interpolatedSample = Sample.Lerp(this[segmentSearcher.StartIndex], this[segmentSearcher.EndIndex], segmentSearcher.Ratio);
                return interpolatedSample;
            }
        }

        public IEnumerator<Sample> GetEnumerator()
        {
            return samples.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        IEnumerator<Vector3> IEnumerable<Vector3>.GetEnumerator()
        {
            foreach (Sample s in samples)
            {
                yield return s.position;
            }
        }
    }
}
