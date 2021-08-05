using System;
using System.Collections.Generic;
using System.Linq;

namespace Perrinn424.Utilities
{
    public class CircularIterator<T> : IIterator<T>
    {
        private readonly T[] array;
        private int index;
        private readonly int length;
        public T Current 
        {
            get => array[index];
            set
            {
                index = Array.IndexOf(array, value);
            }
        }
        public CircularIterator(IEnumerable<T> elements)
        {
            index = 0;
            this.array = elements.ToArray();
            length = array.Length;
        }

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


        public void Reset()
        {
            index = 0;
        }

        private void IncreaseIndex(int delta)
        {
            index = index + delta;

            if (index > length - 1)
            {
                index = index % length;
            }
            else if (index < 0)
            {
                index = length + index; //index here is negative
            }
        }
    } 
}
