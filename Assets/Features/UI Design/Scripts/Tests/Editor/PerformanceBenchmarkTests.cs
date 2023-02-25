using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;
namespace Perrinn424.Editor.Tests
{
    public class PerformanceBenchmarkTests
    {
        [TestCase(13.41127f, 657.8914f, 0.342376709f)]
        [TestCase(24.60585f, 1391.423f, -1.70401764f)]
        [TestCase(5.562164f, 544.5441f, -5.82819128f)]
        public void TimeReferenceTest(float time, float distance, float expectedDifference)
        {
            PerformanceBenchmark porsche919 = PerformanceBenchmarkHelper.CreatePorsche919();
            porsche919.Update(time, distance);
            Assert.That(expectedDifference, Is.EqualTo(porsche919.TimeDiff).Within(10e-3));
        }

        [Test]
        public void GCTest()
        {
            PerformanceBenchmark porsche919 = PerformanceBenchmarkHelper.CreatePorsche919();
            porsche919.Update(0f, 0f);

            Assert.That(() =>
            {
                porsche919.Update(0f, 0f);
            }, Is.Not.AllocatingGCMemory());
        }

        [Test]
        public void PerformanceTest()
        {
            PerformanceBenchmark porsche919 = PerformanceBenchmarkHelper.CreatePorsche919();
            TimeReferenceLegacy porsche919Legacy = new TimeReferenceLegacy(porsche919.distance);

            float t = 174.324f;
            float d = 11620.74f;

            porsche919.Update(t, d);
            Assert.That(porsche919Legacy.LapDiff(t, d), Is.EqualTo(porsche919.TimeDiff).Within(10e-3));

            int numTests = 10000;
            CustomTimer legacy = new CustomTimer("legacy", numTests);
            using (legacy)
            {
                for (int i = 0; i < numTests; i++)
                {
                    porsche919Legacy.LapDiff(t, d);
                }
            }

            CustomTimer newMethod = new CustomTimer("New", numTests);
            using (newMethod)
            {
                for (int i = 0; i < numTests; i++)
                {
                    porsche919.Update(t, d);
                }
            }

            Assert.That(newMethod.Milliseconds, Is.LessThan(legacy.Milliseconds));

        }

        [TestCase(0, 0.0f, ExpectedResult = 0f)]
        [TestCase(0, 1.0f, ExpectedResult = 41f)]
        [TestCase(1, 0.5f, ExpectedResult = 46.5f)]
        public float SpeedTest(int index, float ratio)
        {
            PerformanceBenchmark porsche919 = PerformanceBenchmarkHelper.CreatePorsche919();
            return porsche919.CalculateSpeed(index, ratio);
        }

        [Test]
        public void OutOfIndexTest()
        {
            PerformanceBenchmark porsche = PerformanceBenchmarkHelper.CreatePorsche919();
            Assert.DoesNotThrow(() => porsche.IsCorrectIndex(320, 0f));

            Assert.DoesNotThrow(() => porsche.Update(305.016f, 20737.32f));
        }

        private class TimeReferenceLegacy
        {
            public float[] time;
            public float[] distance;

            private readonly int count;

            public TimeReferenceLegacy(float[] reference)
            {
                count = reference.Length;
                time = new float[count];
                distance = new float[count];

                for (int i = 0; i < reference.Length; i++)
                {
                    time[i] = i;
                    distance[i] = reference[i];
                }
            }

            public float LapDiff(float currentTime, float currentDistance)
            {
                for (int i = 0; i < count - 1; i++)
                {
                    if (distance[i] < currentDistance && currentDistance < distance[i + 1])
                    {
                        float ration = (currentDistance - distance[i]) / (distance[i + 1] - distance[i]);
                        float referenceTime = Mathf.Lerp(time[i], time[i + 1], ration);
                        float diff = currentTime - referenceTime;
                        return diff;
                    }
                }

                return float.NaN;
            }
        }
    } 
}
