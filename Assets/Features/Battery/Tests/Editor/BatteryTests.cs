using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Perrinn424.Editor.Tests
{
    public class BatteryTests
    {
        [Test]
        public void ModelTest()
        {
            BatteryTemperatureModel battery = new BatteryTemperatureModel();

            int lineCount = 0;
            foreach (float[] testLine in GetTestLine())
            {
                //float time = testLine[0];
                float speed = testLine[1];
                float power = testLine[2];

                if (lineCount == 0)
                {
                    battery.InitModel(0.5f, speed, power);
                }
                else
                {
                    battery.UpdateModel(0.5f, speed, power);
                }

                AssertBattery(battery, testLine, lineCount);
                lineCount++;
            }

        }

        private void AssertBattery(BatteryTemperatureModel battery, float[] expectedValues, int lineCount)
        {
            float expectedTotalHeat = expectedValues[3];
            float expectedAirMassFlow = expectedValues[4];
            float expectedHeatDissipation = expectedValues[5];
            float expectedHeatDissipated = expectedValues[6];
            float expectedTemperatureModule = expectedValues[7];
            float expectedQInteral = expectedValues[8];

            float tolerance = 1;
            Assert.That(battery.TotalHeat, Is.EqualTo(expectedTotalHeat).Within(tolerance).Percent, $"Failed in TotalHeat line: {lineCount}");
            Assert.That(battery.AirMassFlow, Is.EqualTo(expectedAirMassFlow).Within(tolerance).Percent, $"Failed in AirMassFlow line: {lineCount}");
            Assert.That(battery.HeatDissipation, Is.EqualTo(expectedHeatDissipation).Within(tolerance).Percent, $"Failed in HeatDissipation line: {lineCount}");
            Assert.That(battery.HeatDissipated, Is.EqualTo(expectedHeatDissipated).Within(tolerance).Percent, $"Failed in HeatDissipated line: {lineCount}");
            Assert.That(battery.TemperatureModule, Is.EqualTo(expectedTemperatureModule).Within(tolerance).Percent, $"Failed in TemperatureModule line: {lineCount}");
            Assert.That(battery.HeatInternal, Is.EqualTo(expectedQInteral).Within(tolerance).Percent, $"Failed in QInteral line: {lineCount}");
        }

        private static IEnumerable<float[]> GetTestLine()
        {
            var root = GetTestPath();
            var path = Path.Combine(root, "ModelTest.csv");


            foreach (var line in File.ReadLines(path).Skip(2)) //headers and units
            {
                yield return line.Split(";").Select(v => float.Parse(v, CultureInfo.InvariantCulture)).ToArray();

                //UnityEngine.Debug.Log(line);
            }
        }

        private static string GetTestPath()
        {
            string fileName = new StackTrace(true).GetFrame(0).GetFileName();
            string rootPath = Path.GetDirectoryName(fileName);
            return rootPath.Substring(rootPath.IndexOf("Assets"));
        }

    } 
}
