using NUnit.Framework;

namespace Perrinn424.Editor.Tests
{
    public class LapTests
    {
        [Test]
        public void IEnumerableTest()
        {
            float[] sectors = {0f, 1f, 2f};
            LapTime lapTime = new LapTime(sectors);

            CollectionAssert.AreEquivalent(sectors, lapTime);

            sectors[0] = 5f;
            CollectionAssert.AreNotEquivalent(sectors, lapTime);
        }

        [Test]
        public void TotalTest()
        {
            float[] sectors = { 0f, 1f, 2f ,3f, 4f};
            LapTime lapTime = new LapTime(sectors);

            Assert.AreEqual(10f, lapTime.Total);
        }

        [Test]
        public void IndexerTest()
        {
            float[] sectors = { 10f, 1f, 2f, 3f, 4f };
            LapTime lapTime = new LapTime(sectors);

            Assert.AreEqual(10f, lapTime[0]);
            Assert.AreEqual(2f, lapTime[2]);
        }
    } 
}
