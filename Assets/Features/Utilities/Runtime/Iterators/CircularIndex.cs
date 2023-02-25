using System;
using System.Collections.Generic;
using System.Linq;

namespace Perrinn424.Utilities
{
    public struct CircularIndex : IEquatable<CircularIndex>
    {
        private int index;
        private readonly int length;
        public CircularIndex(int index, int length)
        {
            this.length = length;
            this.index = FitCircular(index,length);
        }

        public CircularIndex(int length) : this(0, length){}

        public void Assign(int value)
        {
            index = FitCircular(value, length);
        }

        public void Increase(int increment)
        {
            index = Increment(index, increment, length);
        }

        //https://web.archive.org/web/20170210025920/http://javascript.about.com/od/problemsolving/a/modulobug.htm
        private static int Increment(int @base, int increment, int length)
        {
            return FitCircular(@base + increment, length);
        }

        public static int FitCircular(int @base, int length)
        {
            return (@base % length + length) % length;
        }

        public IEnumerable<int> Range(int count)
        {
            return Range(index, count, this.length);
        }

        public IEnumerable<int> Range(int count, int offset)
        {
            return Range(Increment(index, offset, this.length), count, this.length);
        }

        private static IEnumerable<int> Range(int start, int count, int length)
        {
            foreach (int v in Enumerable.Range(start, count))
            {
                yield return FitCircular(v, length);
            }
        }

        public static implicit operator int(CircularIndex d) => d.index;

        public static CircularIndex operator +(CircularIndex t1, int t2)
        {
            t1.Increase(t2);
            return t1;
        }
        public static CircularIndex operator -(CircularIndex t1, int t2)
        {
            t1.Increase(-t2);
            return t1;
        }

        public static CircularIndex operator ++(CircularIndex a)
        {
            a.Increase(1);
            return a;
        }

        public static CircularIndex operator --(CircularIndex a)
        {
            a.Increase(-1);
            return a;
        }

        public static bool operator ==(CircularIndex t1, int t2)
        {
            return t1.index == t2;
        }

        public static bool operator !=(CircularIndex t1, int t2)
        {
            return t1.index != t2;
        }

        public override int GetHashCode()
        {
            return index;
        }

        public override bool Equals(object obj)
        {
            if (obj is int i)
            {
                return this.index == i;
            }
            else if (obj is CircularIndex ci)
            {
                return Equals(ci);
            }

            return false;
        }

        public bool Equals(CircularIndex other)
        {
            return this.index == other.index;
        }

        public override string ToString()
        {
            return index.ToString();
        }
    } 
}
