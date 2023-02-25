using NUnit.Framework;
using UnityEngine;

namespace Perrinn424.Utilities.Editor.Tests
{
    public class UtilitiesTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void CircularIteratorTest()
        {
            int[] array = { 0, 1, 2, 3, 4 };
            CircularIterator<int> circularIterator = new CircularIterator<int>(array);

            void Next()
            {
                for (int i = 0; i < 100; i++)
                {
                    circularIterator.MoveNext();
                }
            }

            void Previous()
            {
                for (int i = 0; i < 100; i++)
                {
                    circularIterator.MovePrevious();
                }
            }

            Assert.DoesNotThrow(Next);
            Assert.DoesNotThrow(Previous);

            circularIterator.Reset();

            for (int i = 1; i < array.Length; i++)
            {
                int previous = array[i-1];
                int current = array[i];
                Assert.AreEqual(previous, circularIterator.Current);
                Assert.AreEqual(current, circularIterator.MoveNext());
                Assert.AreEqual(current, circularIterator.Current);
            }

            //Circular to the right
            Assert.AreEqual(array[0], circularIterator.MoveNext());



            circularIterator.Reset();
            //Circular to the left
            circularIterator.MovePrevious();
            for (int i = array.Length - 1; i >= 0; i--)
            {
                int current = array[i];
                Assert.AreEqual(current, circularIterator.Current);
                circularIterator.MovePrevious();
            }

            circularIterator.Current = 4;
            Assert.That(circularIterator.Current, Is.EqualTo(4));
            circularIterator.MoveNext();
            Assert.That(circularIterator.Current, Is.EqualTo(0));
        }

        [Test]
        public void ClampedIteratorTest()
        {
            ClampedIterator<float> clampedIterator = new ClampedIterator<float>(new []{0f, 0.1f, 0.3f, 1f, 2f, 5f, 10f}, 3);
            Assert.That(clampedIterator.Current, Is.EqualTo(1f));

            //Next
            Assert.That(clampedIterator.MoveNext(), Is.EqualTo(2f));
            Assert.That(clampedIterator.MoveNext(), Is.EqualTo(5f));
            Assert.That(clampedIterator.MoveNext(), Is.EqualTo(10f));
            Assert.That(clampedIterator.MoveNext(), Is.EqualTo(10f));
            Assert.That(clampedIterator.MoveNext(), Is.EqualTo(10f));

            //Previous
            Assert.That(clampedIterator.MovePrevious(), Is.EqualTo(5f));
            Assert.That(clampedIterator.MovePrevious(), Is.EqualTo(2f));
            Assert.That(clampedIterator.MovePrevious(), Is.EqualTo(1f));
            Assert.That(clampedIterator.MovePrevious(), Is.EqualTo(0.3f));
            Assert.That(clampedIterator.MovePrevious(), Is.EqualTo(0.1f));
            Assert.That(clampedIterator.MovePrevious(), Is.EqualTo(0.0f));
            Assert.That(clampedIterator.MovePrevious(), Is.EqualTo(0.0f));

            Assert.That(clampedIterator.MoveNext(), Is.EqualTo(0.1f));

            clampedIterator.Reset();
            Assert.That(clampedIterator.Current, Is.EqualTo(1f));

        }

        [TestCase(Mathf.Infinity, TimeFormatter.Mode.MinutesAndSeconds, @"m\:ss\:fff", @"m\:ss\:fff", "")]
        [TestCase(62.123f, TimeFormatter.Mode.MinutesAndSeconds, @"m\:ss\.fff", @"m\:ss\.fff", "1:02.123")]
        [TestCase(52.123f, TimeFormatter.Mode.MinutesAndSeconds, @"m\:ss\.fff", @"ss\.fff", "52.123")]
        [TestCase(62.123f, TimeFormatter.Mode.TotalSeconds, @"m\:ss\.fff", @"m\:ss\.fff", "62.123")]
        [TestCase(62.123f, TimeFormatter.Mode.MinutesAndSeconds, @"mm\:ss\.fff", @"ss\.fff", "01:02.123")]
        [TestCase(62.123f, TimeFormatter.Mode.MinutesAndSeconds, @"mm\.ss\.fff", @"ss\.fff", "01.02.123")]
        public void TimeFormartterTests(
            float seconds, TimeFormatter.Mode mode, string formatWithMinutes,string formatWithoutMinutes, string expectedResult)
        {
            TimeFormatter formatter = new TimeFormatter(mode, formatWithMinutes, formatWithoutMinutes);
            Assert.That(formatter.ToString(seconds), Is.EqualTo(expectedResult));
        }
    }
}
