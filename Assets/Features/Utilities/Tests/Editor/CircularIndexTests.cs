using NUnit.Framework;
using System.Collections.Generic;

namespace Perrinn424.Utilities.Editor.Tests
{
    public class CircularIndexTests
    {

        [Test]
        public void BinaryOperationTest()
        {
            CircularIndex index = new CircularIndex(3);

            Assert.That((int)index, Is.EqualTo(0));
            index = index + 1;
            Assert.That((int)index, Is.EqualTo(1));
            index = index + 1;
            Assert.That((int)index, Is.EqualTo(2));
            index = index + 1;
            Assert.That((int)index, Is.EqualTo(0));
            index = index - 1;
            Assert.That((int)index, Is.EqualTo(2));
        }


        [Test]
        public void UnaryOperationTest()
        {
            CircularIndex index = new CircularIndex(3);

            Assert.That((int)index, Is.EqualTo(0));
            index++;
            Assert.That((int)index, Is.EqualTo(1));
            index++;
            Assert.That((int)index, Is.EqualTo(2));
            index++;
            Assert.That((int)index, Is.EqualTo(0));
            index--;
            Assert.That((int)index, Is.EqualTo(2));
        }

        [Test]
        public void IndexUseTest()
        {
            int[] array = new int[] { 0, 1, 2, 3, 4, 5 };
            CircularIndex index = new CircularIndex(array.Length);

            Assert.That(array[index], Is.EqualTo(array[0]));

            Assert.That(array[index+1], Is.EqualTo(array[1]));
            Assert.That(array[index+2], Is.EqualTo(array[2]));

            Assert.That(array[index-1], Is.EqualTo(array[array.Length -1]));
            Assert.That(array[index-2], Is.EqualTo(array[array.Length -2]));

            Assert.That(array[index + 6], Is.EqualTo(array[0]));
            Assert.That(array[index - 6], Is.EqualTo(array[0]));

            Assert.That(array[index + 7], Is.EqualTo(array[1]));
            Assert.That(array[index - 7], Is.EqualTo(array[array.Length - 1]));


        }

        [Test]
        public void AssignTest()
        {
            CircularIndex index = new CircularIndex(3);

            Assert.That((int)index, Is.EqualTo(0));
            index.Assign(2);
            Assert.That((int)index, Is.EqualTo(2));
            index.Assign(3);
            Assert.That((int)index, Is.EqualTo(0));
            index.Assign(-1);
            Assert.That((int)index, Is.EqualTo(2));
        }

        [Test]
        public void RangeTest()
        {
            CircularIndex index = new CircularIndex(3);
            IEnumerable<int> actual = null;
            IEnumerable<int> expected = null;


            actual = index.Range(4);
            expected = new int[] { 0, 1, 2, 0 };
            Assert.That(actual, Is.EquivalentTo(expected));

            actual = index.Range(4, 1);
            expected = new int[] { 1, 2, 0, 1 };
            Assert.That(actual, Is.EquivalentTo(expected));

            index++;
            index++; //index now is = 2
            actual = index.Range(6, -4); //current is 2, so 2 minus 4 positions in a circular index is 1
            expected = new int[] { 1, 2, 0, 1, 2, 0 };
            Assert.That(actual, Is.EquivalentTo(expected));
        }
    } 
}
