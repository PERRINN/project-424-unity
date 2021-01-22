using NUnit.Framework;

namespace Perrinn424.Editor.Tests
{

    public class LapTimeTableTests
    {
        private LapTimeTable table;

        [SetUp]
        public void Setup()
        {
            table = new LapTimeTable(3);
            table.AddLap(new []{26.736f, 27.610f, 26.938f});//lap 1
            table.AddLap(new []{26.726f, 27.610f, 26.938f});//lap 2
            table.AddLap(new []{26.730f, 27.610f, 26.936f});//lap 3
            table.AddLap(new []{26.728f, 27.606f, 26.934f});//lap 4
            table.AddLap(new []{26.730f, 27.610f, 26.938f});//lap 5
            table.AddLap(new []{26.734f, 27.614f, 26.936f});//lap 6
            table.AddLap(new []{26.732f, 27.604f, 26.940f});//lap 7
            table.AddLap(new []{26.738f, 27.608f, 26.940f});//lap 8
            table.AddLap(new []{26.726f, 27.608f, 26.934f});//lap 9
            table.AddLap(new []{26.736f, 27.602f, 26.934f});//lap 10
        }

        [Test]
        public void BestSectorsTest()
        {
            int[] expectedBest = new[] {1, 9, 3};
            int[] bestIndex = table.GetBest();
            Assert.That(bestIndex, Is.EquivalentTo(expectedBest));
        }
    } 
}
