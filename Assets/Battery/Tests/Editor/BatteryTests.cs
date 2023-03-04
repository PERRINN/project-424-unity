using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

public class BatteryTests
{
    [Test]
    public void ModelTest()
    {
        Battery battery = new Battery();

        int lineCount = 0;
        foreach (float[] testLine in GetTestLine())
        {
            //float time = testLine[0];
            float speed = testLine[1];
            float power = testLine[2];

            battery.UpdateModel(0.5f, speed, power);

            float expectedTotalHeat = testLine[3];
            float expectedAirMassFlow = testLine[4];
            float expectedHeatDissipation = testLine[5];
            float expectedHeatDissipated = testLine[6];
            float expectedTemperatureModule = testLine[7];
            float expectedQInteral = testLine[8];

            Assert.That(battery.TotalHeat, Is.EqualTo(expectedTotalHeat).Within(1).Percent, $"Failed in TotalHeat line: {lineCount}");
            Assert.That(battery.AirMassFlow, Is.EqualTo(expectedAirMassFlow).Within(1).Percent, $"Failed in AirMassFlow line: {lineCount}");
            Assert.That(battery.HeatDissipation, Is.EqualTo(expectedHeatDissipation).Within(1).Percent, $"Failed in HeatDissipation line: {lineCount}");
            Assert.That(battery.HeatDissipated, Is.EqualTo(expectedHeatDissipated).Within(1).Percent, $"Failed in HeatDissipated line: {lineCount}");
            Assert.That(battery.TemperatureModule, Is.EqualTo(expectedTemperatureModule).Within(1).Percent, $"Failed in TemperatureModule line: {lineCount}");
            Assert.That(battery.QInteral, Is.EqualTo(expectedQInteral).Within(1).Percent, $"Failed in QInteral line: {lineCount}");
            lineCount++;
        }

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
