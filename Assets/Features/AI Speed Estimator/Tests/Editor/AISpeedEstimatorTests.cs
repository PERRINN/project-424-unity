using NUnit.Framework;
using UnityEditor;
using Perrinn424.TelemetryLapSystem;
using UnityEngine;
using System.Diagnostics;
using Unity.Sentis;

namespace Perrinn424.AISpeedEstimatorSystem.Editor.Tests
{
    [TestFixture]
    public class AISpeedEstimatorTests
    {
        [Test]
        public void LapTest()
        {
            (ModelAsset model, TelemetryLapAsset lap) = GetAssets();

            Assert.That(model, Is.Not.Null);
            Assert.That(lap, Is.Not.Null);

            Table table = lap.table;

            using AISpeedEstimator aISpeedEstimator = new AISpeedEstimator(model);
            AISpeedEstimatorInput input = new AISpeedEstimatorInput();

            Stopwatch sw = Stopwatch.StartNew();
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
            sw.Stop();

            error /= table.RowCount;

            UnityEngine.Debug.Log($"Elapsed time to process {table.RowCount} frames: {sw.ElapsedMilliseconds} ms");
            UnityEngine.Debug.Log($"Avg Error: {error} m/s. {error * 3.6f} km/h");


            Assert.That(error, Is.LessThan(7f / 3.6f)); //avg error less than 7 km/h
        }

        private static (ModelAsset model, TelemetryLapAsset lap) GetAssets()
        {
            var path = GetPluginPath();

            TelemetryLapAsset telemetry = GetAssetFromPath<TelemetryLapAsset>(path);
            ModelAsset model = GetAssetFromPath<ModelAsset>(path);

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
