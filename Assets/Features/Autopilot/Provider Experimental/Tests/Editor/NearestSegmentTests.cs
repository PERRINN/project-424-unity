using NUnit.Framework;
using Perrinn424.AutopilotSystem;
using Perrinn424.Utilities;
using System;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class NearestSegmentTests
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
    public void DummyTest()
    {
        BruteForceSegmentSearcher bruteForce = new BruteForceSegmentSearcher(path);
        CircularIndex circularIndex = new CircularIndex(0, path.Count);

        for (int i = 0; i < path.Count; i++)
        {
            circularIndex.Assign(i);
            Vector3 targetPos = path[circularIndex];
            Vector3 prev = path[circularIndex - 1];
            Vector3 next = path[circularIndex + 1];

            float distPrev = Vector3.Distance(targetPos, prev);
            float distNext = Vector3.Distance(targetPos, next);
            float distance = Mathf.Min(distPrev, distNext) * 0.8f;

            targetPos = targetPos + Random.insideUnitSphere * distance;
            bruteForce.Search(targetPos);
            Assert.That(i, Is.EqualTo(bruteForce.StartIndex));

        }
    }

    [Test]
    public void BruteForceNN()
    {
        BruteForceSearcher bruteForce = new BruteForceSearcher(path);
        FullTest(bruteForce.Search);
    }

    [Test]
    public void SectorSearcherTest()
    {
        SectorSearcher sectorSearcher = new SectorSearcher(path);
        Debug.Log($"Sector Size: {sectorSearcher.SectorSize}");
        FullTest(sectorSearcher.Search);
        RandomTest(sectorSearcher.Search);
    }


    [Test]
    public void HeuristicNNTest()
    {
        HeuristicNN heuristic = new HeuristicNN(path, 10, 10);
        FullTest(heuristic.Search);
    }

    private void FullTest(Func<Vector3, (int, float)> nnSearcher)
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
            (int nnIndex, float distance) = nnSearcher(targetPos);
            string errorMsg = $"The nearest neighbor of {targetPos} is {path[i]} (dist: {Vector3.Distance(targetPos, path[i])}), not {path[nnIndex]} (dist: {distance})";
            Assert.That(i, Is.EqualTo(nnIndex), errorMsg);
            float tolerance = 1.0e-2f;
            Assert.That(distance, Is.LessThanOrEqualTo(testDistance + tolerance));
        }
    }

    private void RandomTest(Func<Vector3, (int, float)> nnSearcher)
    {
        int testCount = 100;
        BruteForceSearcher bruteForce = new BruteForceSearcher(path);

        for (int i = 0; i < testCount; i++)
        {
            Vector3 randomPosition = Vector3.Scale(Random.insideUnitSphere, bounds.size*1.2f) + bounds.center;
            (int nnBruteForceResult, _) = bruteForce.Search(randomPosition);
            (int nnSearcherResult, _) = nnSearcher(randomPosition);
            Assert.That(nnSearcherResult, Is.EqualTo(nnSearcherResult));
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
