using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Utils;

namespace Perrinn424.AutopilotSystem.Editor.Tests
{
    public class ProjectorTests
    {
        [Test]
        public void LocalCoordinatesProjectorTest()
        {
            TestProjector(new LocalCoordinatesProjector());
        }

        [Test]
        public void TestRayProjectorTest()
        {
            TestProjector(new RayProjector());
        }

        [Test]
        public void CrossProductProjectorTest()
        {
            TestProjector(new CrossProductProjector());
        }

        [Test]
        public void AreaProjectorTest()
        {
            TestProjector(new AreaProjector());
        }

        private void TestProjector(IProjector projector)
        {
            GameObject go = new GameObject("test");
            go.transform.position = Vector3.zero;
            Vector3 start = new Vector3(1f, 0f, -1f);
            Vector3 end = new Vector3(1f, 0f, 1f);

            (Vector3 projectedPosition, float ratio) = projector.Project(go.transform, start, end);
            Vector3 expectedProjectedPosition = new Vector3(1f, 0f, 0f);
            Assert.That(projectedPosition, Is.EqualTo(expectedProjectedPosition).Using(Vector3EqualityComparer.Instance));
            Assert.That(ratio, Is.EqualTo(0.5f).Within(1e-2));

            Object.DestroyImmediate(go);
        }
    } 
}
