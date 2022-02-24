using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    public class Path : IPath
    {
        private readonly IReadOnlyList<Sample> lap;

        public Path(IReadOnlyList<Sample> lap)
        {
            this.lap = lap;
        }

        public Vector3 this[int index] => lap[index].position;

        public int Count => lap.Count;

        public IEnumerator<Vector3> GetEnumerator()
        {
            foreach (Sample s in lap)
            {
                yield return s.position;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    } 
}
