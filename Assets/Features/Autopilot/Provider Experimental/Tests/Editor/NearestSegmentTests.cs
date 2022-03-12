using NUnit.Framework;
using Perrinn424.AutopilotSystem;
using Perrinn424.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NearestSegmentTests
{

    private Path path;
    [SetUp]
    public void Setup()
    {
        path = new Path(LoadRecordedLap());
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
