using NUnit.Framework;

namespace Perrinn424.Editor.Tests
{
    public class TimeReferenceTests
    {
        [TestCase(13.41127f, 657.8914f, 0.4128628f)]
        [TestCase(24.60585f, 1391.423f, -1.513094f)]
        [TestCase(5.562164f, 544.5441f, -5.779054f)]
        public void TimeReferenceTest(float time, float distance, float expectedDifference)
        {
            TimeReference porsche = TimeReferenceHelper.CreatePorsche();
            float difference = porsche.LapDiff(time, distance);
            Assert.That(expectedDifference, Is.EqualTo(difference).Within(10e-3));
        }
    } 
}
