using NUnit.Framework;

namespace Perrinn424.Editor.Tests
{
    public class TimeReferenceTests
    {
        [Test]
        public void TimeReferenceTest()
        {
            TimeReference porsche = TimeReferenceHelper.CreatePorsche();
            float diff = porsche.LapDiff(0.1f, 12f);
            Assert.AreEqual(19f, diff);
        }

    } 
}
