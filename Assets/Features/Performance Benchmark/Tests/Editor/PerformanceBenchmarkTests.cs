using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;
namespace Perrinn424.PerformanceBenchmarkSystem.Editor.Tests
{
    public class PerformanceBenchmarkTests
    {
        private PerformanceBenchmarkData data;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            data = LoadData();
        }

        private PerformanceBenchmarkData LoadData()
        {
            var guids = AssetDatabase.FindAssets("t:PerformanceBenchmarkData");
            return AssetDatabase.LoadAssetAtPath<PerformanceBenchmarkData>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }


        [TestCase(13.41127f, 657.8914f, 0.044112506f)]
        [TestCase(24.60585f, 1391.423f, -1.929898503f)]
        [TestCase(5.562164f, 544.5441f, -6.129224123f)]
        public void TimeReferenceTest(float time, float distance, float expectedDifference)
        {
            IPerformanceBenchmark porsche919 = new PerformanceBenchmark(data.samples);
            porsche919.Update(time, distance);
            Assert.That(expectedDifference, Is.EqualTo(porsche919.TimeDiff).Within(10e-3));
        }

        [Test]
        public void GCTest()
        {
            IPerformanceBenchmark porsche919 = new PerformanceBenchmark(data.samples);
            porsche919.Update(0f, 0f);


            Assert.That(() =>
            {
                porsche919.Update(0f, 0f);
            }, Is.Not.AllocatingGCMemory());
        }


        [Test]
        public void BoundaryTest()
        {

            List<PerformanceBenchmarkSample> samples = new List<PerformanceBenchmarkSample>()
            {
                new PerformanceBenchmarkSample(){ distance = 0f },
                new PerformanceBenchmarkSample(){ distance = 3f },
                new PerformanceBenchmarkSample(){ distance = 5f },
                new PerformanceBenchmarkSample(){ distance = 6f },
            };

            //IPerformanceBenchmark performanceBenchmark = new PerformanceBenchmark(new float[] { 0f, 3f, 5f, 6f }, 0f);
            PerformanceBenchmark performanceBenchmark = new PerformanceBenchmark (samples);

            //float[] testValues = { -80f, 0f, 1.5f, 3f, 3.5f, 5f, 5.5f, 6f, 80f };

            //foreach (float value in testValues)
            //{
            //    UnityEngine.Debug.Log($"With {value} the index is: {performanceBenchmark.BinarySearch(value)}");

            //}

            float currentDistance = 1.5f;
            performanceBenchmark.Update(0f, currentDistance);

            Assert.That(performanceBenchmark.PreviousIndex, Is.EqualTo(0));
            Assert.That(performanceBenchmark.PreviousIndex, Is.EqualTo(performanceBenchmark.BinarySearch(currentDistance)));

            currentDistance = 3f; //boundary
            performanceBenchmark.Update(0f, currentDistance);
            Assert.That(performanceBenchmark.PreviousIndex, Is.EqualTo(1));
            Assert.That(performanceBenchmark.PreviousIndex, Is.EqualTo(performanceBenchmark.BinarySearch(currentDistance)));
        }

        [Test]
        public void OutOfIndexTest()
        {
            IPerformanceBenchmark porsche = new PerformanceBenchmark(data.samples);
            Assert.DoesNotThrow(() => porsche.Update(305.016f, 20737.32f));
        }
    } 
}
