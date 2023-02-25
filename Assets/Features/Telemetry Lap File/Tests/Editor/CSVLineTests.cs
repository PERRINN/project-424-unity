using NUnit.Framework;

namespace Perrinn424.TelemetryLapSystem.Editor.Tests
{
    public class CSVLineTests
    {
        [Test]
        public void LineTest()
        {
            string[] headers = 
                {        
                "FRAME",
                "TIME",
                "DISTANCE",
                "TOTALTIME",
                "TOTALDISTANCE",
                "SEGMENTNUM",
                "SECTOR",
                "MARKERS",
                "MARKERTIME",
                "MARKERFLAG",
                "POSITIONX",
                "POSITIONZ",
                "POSITIONY",
                "SPEED",
                "STEERINGANGLE" 
            };

            CSVLine line = new CSVLine(headers);
            line.UpdateValues("0,1,2,3,4,5,6,7,8,9,10,11,12,13,14");
            Assert.That(line.Time, Is.EqualTo(1f));
            Assert.That(line.Speed, Is.EqualTo(13f));
        }

        [Test]
        public void DynamicTest()
        {
            string[] headers = { "HEADER1", "HEADER2" };
            CSVLine line = new CSVLine(headers);
            line.UpdateValues("0.55, 17.13");
            dynamic dynamicLine = line;
            Assert.That(dynamicLine.HEADER1, Is.EqualTo(0.55f));

            dynamicLine.HEADER2 = 15f;
            Assert.That(dynamicLine.HEADER2, Is.EqualTo(15f));
        }
    } 
}
