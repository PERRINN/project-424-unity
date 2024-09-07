using NUnit.Framework;
using Unity.Barracuda;
using UnityEditor;
using Perrinn424.TelemetryLapSystem;
using static VehiclePhysics.EnergyProvider;
using Mirror;
using UnityEngine;

namespace Perrinn424.AISpeedEstimatorSystem.Editor.Tests
{
    [TestFixture]
    public class AISpeedEstimatorTests
    {
        [Test]
        public void BasicTest()
        {
            (NNModel model, TelemetryLapAsset lap) = GetAssets();

            Assert.That(model, Is.Not.Null);
            Assert.That(lap, Is.Not.Null);

            Table table = lap.table;

            AISpeedEstimator aISpeedEstimator = new AISpeedEstimator(model);
            AISpeedEstimatorInput input = new AISpeedEstimatorInput();

            float error = 0;
            for (int rowIndex = 0; rowIndex < table.RowCount; rowIndex++)
            {

                float speed = table[rowIndex, "Speed"]/3.6f; //kmh to m/s

                input.throttle = table[rowIndex, "Throttle"];
                input.brake = table[rowIndex, "Brake"];
                input.accelerationLateral = table[rowIndex, "AccelerationLat"];
                input.accelerationLongitudinal = table[rowIndex, "AccelerationLong"];
                input.accelerationVertical = table[rowIndex, "AccelerationVert"];
                input.nWheelFL = table[rowIndex, "nWheelFL"];
                input.nWheelFR = table[rowIndex, "nWheelFR"];
                input.nWheelRL = table[rowIndex, "nWheelRL"];
                input.nWheelRR = table[rowIndex, "nWheelRR"];
                input.steeringAngle = table[rowIndex, "SteeringAngle"];

                float estimation = aISpeedEstimator.Estimate(ref input);
                error += Mathf.Abs(estimation - speed);
            }

            error /= table.RowCount;

            Assert.That(error, Is.LessThan(5f / 3.6f)); //avg error less than 5 km/h
        }

        private static (NNModel model, TelemetryLapAsset lap) GetAssets()
        {
            var path = GetPluginPath();

            TelemetryLapAsset telemetry = GetAssetFromPath<TelemetryLapAsset>(path);
            NNModel model = GetAssetFromPath<NNModel>(path);

            return (model, telemetry);
        }

        private static T GetAssetFromPath<T>(string path) where T : UnityEngine.Object
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { path });
            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        private static string GetPluginPath()
        {
            string fileName = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
            string rootPath = System.IO.Path.GetDirectoryName(fileName);
            return rootPath.Substring(rootPath.IndexOf("Assets"));
        }
    } 
}
