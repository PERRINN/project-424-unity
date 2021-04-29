using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Perrinn424.Utilities
{
    public class ClampedIterator<T> : IIterator<T>
    {
        private readonly T[] array;
        public int CurrentIndex { get; private set; }
        private readonly int length;
        public T Current => array[CurrentIndex];
        private readonly int startIndex;


        public ClampedIterator(IEnumerable<T> elements, int startIndex)
        {
            this.startIndex = startIndex;
            Reset();
            this.array = elements.ToArray();
            length = array.Length;
        }

        public ClampedIterator(IEnumerable<T> elements):this(elements, 0){}

        public T MoveNext()
        {
            IncreaseIndex(1);
            return Current;
        }

        public T MovePrevious()
        {
            IncreaseIndex(-1);
            return Current;
        }

        private void IncreaseIndex(int delta)
        {
            CurrentIndex = Mathf.Clamp(CurrentIndex + delta, 0, length - 1);
        }

        public void Reset()
        {
            CurrentIndex = startIndex;
        }
    }
}
