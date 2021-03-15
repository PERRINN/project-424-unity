using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;
namespace Perrinn424.Editor.Tests
{
    public class TimeReferenceTests
    {
        [TestCase(13.41127f, 657.8914f, 0.370546341f)]
        [TestCase(24.60585f, 1391.423f, -1.62420464f)]
        [TestCase(5.562164f, 544.5441f, -5.81348467f)]
        public void TimeReferenceTest(float time, float distance, float expectedDifference)
        {
            TimeReference porsche = TimeReferenceHelper.CreatePorsche();
            float difference = porsche.LapDiff(time, distance);
            Assert.That(expectedDifference, Is.EqualTo(difference).Within(10e-3));
        }

        [Test]
        public void GCTest()
        {
            TimeReference porsche = TimeReferenceHelper.CreatePorsche();
            porsche.LapDiff(0f, 0f);

            Assert.That(() =>
            {
                porsche.LapDiff(0f, 0f);
            }, Is.Not.AllocatingGCMemory());
        }

        [Test]
        public void PerformanceTest()
        {
            TimeReference porsche = TimeReferenceHelper.CreatePorsche();
            TimeReferenceLegacy porscheLegacy = new TimeReferenceLegacy(porsche.distance);

            float t = 5.562164f;
            float d = 544.5441f;

            Assert.That(porscheLegacy.LapDiff(t, d), Is.EqualTo(porsche.LapDiff(t, d)).Within(10e-3));

            int numTests = 10000;
            CustomTimer legacy = new CustomTimer("legacy", numTests);
            using (legacy)
            {
                for (int i = 0; i < numTests; i++)
                {
                    porscheLegacy.LapDiff(t, d);
                }
            }

            CustomTimer newMethod = new CustomTimer("New", numTests);
            using (newMethod)
            {
                for (int i = 0; i < numTests; i++)
                {
                    porsche.LapDiff(t, d);
                }
            }

            Assert.That(newMethod.Milliseconds, Is.LessThan(legacy.Milliseconds));

        }

        [Test]
        public void OutOfIndexTest()
        {
            TimeReference porsche = TimeReferenceHelper.CreatePorsche();
            Assert.DoesNotThrow(() => porsche.IsCorrectIndex(320, 0f));

            Assert.DoesNotThrow(() => porsche.LapDiff(305.016f, 20737.32f));
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
