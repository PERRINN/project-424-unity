using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Perrinn424.Utils
{
    public class CircularBuffer<T>
    {
        private T[] array;
        private int index;
        private int length;
        public T Current 
        {
            get => array[index];
        }
        public CircularBuffer(IEnumerable<T> elements)
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
