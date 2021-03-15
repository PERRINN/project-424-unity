using System;
using NUnit.Framework;
using UnityEngine;

namespace Perrinn424.Utils.Editor.Tests
{
    public class UtilsTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void CircularBufferTest()
        {
            int[] array = { 0, 1, 2, 3, 4 };
            CircularBuffer<int> buffer = new CircularBuffer<int>(array);

            void Next()
            {
                for (int i = 0; i < 100; i++)
                {
                    buffer.MoveNext();
                }
            }

            void Previous()
            {
                for (int i = 0; i < 100; i++)
                {
                    buffer.MovePrevious();
                }
            }

            Assert.DoesNotThrow(Next);
            Assert.DoesNotThrow(Previous);

            buffer.Reset();

            for (int i = 1; i < array.Length; i++)
            {
                int previous = array[i-1];
                int current = array[i];
                Assert.AreEqual(previous, buffer.Current);
                Assert.AreEqual(current, buffer.MoveNext());
                Assert.AreEqual(current, buffer.Current);
            }

            //Circular to the right
            Assert.AreEqual(array[0], buffer.MoveNext());



            buffer.Reset();
            //Circular to the left
            buffer.MovePrevious();
            for (int i = array.Length - 1; i >= 0; i--)
            {
                int current = array[i];
                Assert.AreEqual(current, buffer.Current);
                buffer.MovePrevious();
            }
        }

        [TestCase(Mathf.Infinity, TimeFormatter.Mode.MinutesAndSeconds, @"m\:ss\:fff", @"m\:ss\:fff", "")]
        [TestCase(62.123f, TimeFormatter.Mode.MinutesAndSeconds, @"m\:ss\.fff", @"m\:ss\.fff", "1:02.123")]
        [TestCase(52.123f, TimeFormatter.Mode.MinutesAndSeconds, @"m\:ss\.fff", @"ss\.fff", "52.123")]
        [TestCase(62.123f, TimeFormatter.Mode.TotalSeconds, @"m\:ss\.fff", @"m\:ss\.fff", "62.123")]
        [TestCase(62.123f, TimeFormatter.Mode.MinutesAndSeconds, @"mm\:ss\.fff", @"ss\.fff", "01:02.123")]
        public void TimeFormartterTests(
            float seconds, TimeFormatter.Mode mode, string formatWithMinutes,string formatWithoutMinutes, string expectedResult)
        {
            TimeFormatter formartter = new TimeFormatter(mode, formatWithMinutes, formatWithoutMinutes);
            Assert.That(formartter.ToString(seconds), Is.EqualTo(expectedResult));
        }
    }
}
