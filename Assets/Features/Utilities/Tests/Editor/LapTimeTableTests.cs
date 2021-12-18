using NUnit.Framework;

namespace Perrinn424.Utilities.Editor.Tests
{

    public class LapTimeTableTests
    {
        private LapTimeTable table;

        [SetUp]
        public void Setup()
        {
            table = new LapTimeTable(3);
            table.AddLap(new[] { 26.736f, 27.610f, 26.938f });//lap 1
            table.AddLap(new[] { 26.726f, 27.610f, 26.938f });//lap 2
            table.AddLap(new[] { 26.730f, 27.610f, 26.936f });//lap 3
            table.AddLap(new[] { 26.728f, 27.606f, 26.934f });//lap 4
            table.AddLap(new[] { 26.730f, 27.610f, 26.938f });//lap 5
            table.AddLap(new[] { 26.734f, 27.614f, 26.936f });//lap 6
            table.AddLap(new[] { 26.732f, 27.604f, 26.940f });//lap 7
            table.AddLap(new[] { 26.738f, 27.608f, 26.940f });//lap 8
            table.AddLap(new[] { 26.726f, 27.608f, 26.934f });//lap 9
            table.AddLap(new[] { 26.736f, 27.602f, 26.934f });//lap 10
            table.AddSector(30f);
        }

        [Test]
        public void BestSectorsTest()
        {
            int[] expectedBest = { 1, 9, 3, 3 };
            int[] bestIndex = table.GetBestLapForEachSector();
            Assert.That(bestIndex, Is.EquivalentTo(expectedBest));
        }

        [TestCase(6, 1, 2)]
        [TestCase(11, 2, 3)]
        public void IndexToLapSectorTest(int index, int expectedLap, int expectedSector)
        {
            table.IndexToLapSector(index, out int lap, out int sector);
            Assert.AreEqual(expectedLap, lap);
            Assert.AreEqual(expectedSector, sector);
        }

        [TestCase(2, 0, 8)]
        [TestCase(3, 2, 14)]
        public void LapSectorToIndexTest(int lap, int sector, int expectedIndex)
        {
            table.LapSectorToIndex(lap, sector, out int index);
            Assert.AreEqual(expectedIndex, index);
        }


        [Test]
        public void ImprovementTest()
        {
            int[] expectedImprovement = { 4, 7, 10, 13, 14, 15, 25, 37 };
            int[] improvementIndices = table.GetImprovedTimes();

            Assert.That(improvementIndices, Is.EquivalentTo(expectedImprovement));
        }

        [Test]
        public void BestLapTest()
        {
            int bestLap = table.GetBestLap();
            int expected = 3;
            Assert.AreEqual(expected, bestLap);
        }

        [Test]
        public void IncompleteLapTest()
        {
            table = new LapTimeTable(3);
            table.AddLap(new[] { 26.736f, 27.610f, 26.938f });//lap 1
            table.AddLap(new[] { 26.726f, 27.610f, 26.938f });//lap 2
            table.AddLap(new[] { 26.730f, 27.610f, 26.936f });//lap 3
            //table.AddLap(new[] { 26.728f, 27.606f, 26.934f });//lap 4
            table.AddSector(20f);

            int bestLap = table.GetBestLap();
            int expected = 1;
            Assert.AreEqual(expected, bestLap);

            int[] expectedBest = { 3, 0, 2, 1 };
            int[] bestIndex = table.GetBestLapForEachSector();
            Assert.That(bestIndex, Is.EquivalentTo(expectedBest));

            int[] improvementIndices = table.GetImprovedTimes();
            table.LapSectorToIndex(3, 0, out int testIndex);
            CollectionAssert.Contains(improvementIndices, testIndex);
        }

        [Test]
        public void IdealLapTest()
        {
            table = new LapTimeTable(5);
            table.AddLap(new[] { 100f, 100f, 100f, 60.310f, 100f });
            table.AddLap(new[] { 64.200f, 57.231f, 57.977f, 60.360f, 76.194f });
            table.AddLap(new[] { 64.181f, 57.240f, 57.958f, 60.363f, 76.209f });
            table.AddLap(new[] { 64.187f, 57.248f, 57.956f, 60.370f, 76.176f });
            table.AddSector(64.184f);
            table.AddSector(57.200f);

            LapTime idealLap = table.GetIdealLap();
            LapTime expectedIdealLap = new LapTime(new[] { 64.181f, 57.200f, 57.956f, 60.310f, 76.176f });

            Assert.That(idealLap, Is.EqualTo(expectedIdealLap).AsCollection);

        }
    }
} 
