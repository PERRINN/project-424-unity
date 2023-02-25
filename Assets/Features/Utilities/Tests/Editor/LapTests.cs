using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Perrinn424.Utilities.Editor.Tests
{
    public class LapTests
    {
        [Test]
        public void IEnumerableTest()
        {
            float[] sectors = {0f, 1f, 2f};
            LapTime lapTime = new LapTime(sectors);

            float []expected = { 0f, 1f, 2f , 0f+1f+2f};
            CollectionAssert.AreEquivalent(expected, lapTime.ToArray());

            expected[0] = 5f;
            CollectionAssert.AreNotEquivalent(expected, lapTime.ToArray());
        }

        [Test]
        public void TotalTest()
        {
            float[] sectors = { 0f, 1f, 2f ,3f, 4f};
            LapTime lapTime = new LapTime(sectors);

            Assert.AreEqual(10f, lapTime.Sum);
        }

        [Test]
        public void IndexerTest()
        {
            float[] sectors = { 10f, 1f, 2f, 3f, 4f };
            LapTime lapTime = new LapTime(sectors);

            Assert.AreEqual(10f, lapTime[0]);
            Assert.AreEqual(2f, lapTime[2]);
        }

        [Test]
        public void IncompletedLapTest()
        {
            float[] sectors = { 10f, 1f, 2f, 3f, 4f };
            LapTime lapTime = new LapTime(sectors);
            Assert.IsTrue(lapTime.IsCompleted);

            lapTime = new LapTime(3);
            Assert.IsFalse(lapTime.IsCompleted);
            Assert.That(lapTime.Sum, Is.EqualTo(Mathf.Infinity));
            Assert.That(lapTime[1], Is.EqualTo(Mathf.Infinity));

            lapTime = new LapTime(3, new[]{10f, 1f});
            Assert.IsFalse(lapTime.IsCompleted);
            Assert.That(lapTime[0], Is.EqualTo(10f));
            Assert.That(lapTime[1], Is.EqualTo(1f));
            Assert.That(lapTime[2], Is.EqualTo(Mathf.Infinity));

            Assert.Throws<ArgumentException>(() => new LapTime(2, new[] {1f, 2f, 3f}));
        }

        [Test]
        public void AddSectorTest()
        {
            LapTime lap = new LapTime(4, new []{1f,2f});
            Assert.IsFalse(lap.IsCompleted);
            Assert.That(lap.Sum, Is.EqualTo(Mathf.Infinity));
            Assert.That(lap.SectorsCompletedIndex, Is.EqualTo(2));

            lap.AddSector(3f);
            Assert.IsFalse(lap.IsCompleted);
            Assert.That(lap.Sum, Is.EqualTo(Mathf.Infinity));
            lap.AddSector(4f);
            Assert.IsTrue(lap.IsCompleted);
            Assert.That(lap.Sum, Is.EqualTo(10f));

            float[] expected = { 1f, 2f, 3f, 4f, 10f};
            CollectionAssert.AreEquivalent(expected, lap.ToArray());

            Assert.Throws<ArgumentException>(() => lap.AddSector(1234567f));

        }
    } 
}
