using NUnit.Framework;
using Perrinn424.AutopilotSystem;
using System;
using Unity.Barracuda;
using UnityEditor.Sprites;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using VehiclePhysics;
using UnityEditor.SceneManagement;

namespace Perrinn424.AISpeedEstimatorSystem.Editor.Tests
{
    [TestFixture]
    public class AISpeedEstimatorTests
    {
        [Test]
        public void BasicTest()
        {
            (NNModel model, RecordedLap lap) = GetAssets();

            Assert.That(model, Is.Not.Null);
            Assert.That(lap, Is.Not.Null);
        }

        private static (NNModel model, RecordedLap lap) GetAssets()
        {
            Scene mainScene = EditorSceneManager.OpenScene("Assets/Scenes/424 Nordschleife Scene.unity");
            //SceneAsset mainScene = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/424 Nordschleife Scene.unity");
            //mainScene.
            //SceneAsset.FindAnyObjectByType<AISpeedEstimator>(mainScene)
            //mainScene.FindAnyObjectByType<AISpeedEstimator>();

            foreach (GameObject go in mainScene.GetRootGameObjects())
            {
                if (go.GetComponent<VehicleBase>() is VehicleBase vehicle)
                {
                    AISpeedEstimatorContainer speedEstimator = vehicle.GetComponentInChildren<AISpeedEstimatorContainer>();
                    Autopilot autopilot = vehicle.GetComponentInChildren<Autopilot>();
                    return (speedEstimator.modelAsset, autopilot.recordedLap);
                }
            }

            throw new InvalidOperationException("Assets not found in scene");
        }
    } 
}
