using NUnit.Framework;
using Perrinn424.Utilities;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Perrinn424.AutopilotSystem.Editor.Tests
{
    public class NearestNeighbourTests
    {

        private Path path;
        private Bounds bounds;
        [SetUp]
        public void Setup()
        {
            path = new Path(LoadRecordedLap());

            foreach (Vector3 wayPoint in path)
            {
                bounds.Encapsulate(wayPoint);
            }
        }

        [Test]
        public void BruteForceNN()
        {
            BruteForceSearcher bruteForce = new BruteForceSearcher(path);
            FullTest(bruteForce);
        }

        [Test]
        public void SectorSearcherTest()
        {
            SectorSearcherNearestNeighbor sectorSearcher = new SectorSearcherNearestNeighbor(path, 2, 4);
            FullTest(sectorSearcher);
            RandomTest(new SectorSearcherNearestNeighbor(path, 2, Mathf.Infinity));
        }


        [Test]
        public void HeuristicNNTest()
        {
            HeuristicNearestNeighbor heuristic = new HeuristicNearestNeighbor(path, 10, 10, 4);
            FullTest(heuristic);
        }

        [Test]
        public void AutopilotOffModeSearcherTest()
        {
            AutopilotNearestNeighbourSearcher autopilotOffMode = new AutopilotNearestNeighbourSearcher(path);
            FullTest(autopilotOffMode);
        }

        private void FullTest(INearestNeighbourSearcher searcher)
        {
            CircularIndex circularIndex = new CircularIndex(0, path.Count);

            for (int i = 0; i < path.Count; i++)
            {
                circularIndex.Assign(i);
                Vector3 targetPos = path[circularIndex];
                Vector3 prev = path[circularIndex - 1];
                Vector3 next = path[circularIndex + 1];

                float distPrev = Vector3.Distance(targetPos, prev);
                float distNext = Vector3.Distance(targetPos, next);
                float testDistance = Mathf.Min(distPrev, distNext) * 0.4f;

                targetPos = targetPos + Random.insideUnitSphere * testDistance;
                searcher.Search(targetPos);
                string errorMsg = $"The nearest neighbor of {targetPos} is {path[i]} (dist: {Vector3.Distance(targetPos, path[i])}), not {searcher.Position} (dist: {searcher.Distance})";
                Assert.That(i, Is.EqualTo(searcher.Index), errorMsg);
                float tolerance = 1.0e-2f;
                Assert.That(searcher.Distance, Is.LessThanOrEqualTo(testDistance + tolerance));
            }
        }

        private void RandomTest(INearestNeighbourSearcher searcher)
        {
            int testCount = 100;
            BruteForceSearcher bruteForce = new BruteForceSearcher(path);

            for (int i = 0; i < testCount; i++)
            {
                Vector3 randomPosition = Vector3.Scale(Random.insideUnitSphere, bounds.size * 1.2f) + bounds.center;
                bruteForce.Search(randomPosition);
                searcher.Search(randomPosition);
                Assert.That(searcher.Index, Is.EqualTo(bruteForce.Index));
            }

        }


        private RecordedLap LoadRecordedLap()
        {
            var path = GetPluginPath();
            var guids = AssetDatabase.FindAssets("t:recordedlap", new[] { path });
            return AssetDatabase.LoadAssetAtPath<RecordedLap>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        private static string GetPluginPath()
        {
            string fileName = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
            string rootPath = System.IO.Path.GetDirectoryName(fileName);
            return rootPath.Substring(rootPath.IndexOf("Assets"));
        }
    }

}