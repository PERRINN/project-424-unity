using System.Linq;
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
    } 
}
