using NUnit.Framework;
using System.IO;

namespace Perrinn424.TelemetryLapSystem.Editor.Tests
{
    public class TelemetryLapFileWriterTests
    {
        private TelemetryLapFileWriter lapFile;

        [SetUp]
        public void SetUp()
        {
            lapFile = new TelemetryLapFileWriter(new string[0], new string[0]);
        }

        [TearDown]
        public void TearDown()
        {
            lapFile.Delete();
        }

        [Test]
        public void CreateFileTest()
        {
            lapFile.StartRecording();
            Assert.That(File.Exists(lapFile.TempFullRelativePath), Is.True);
            Assert.That(File.Exists(lapFile.FullRelativePath), Is.False);
            Assert.That(File.Exists(lapFile.MetadataFullRelativePath), Is.False);
            lapFile.StopRecordingAndSaveFile(false, false, 0f);
            lapFile.WriteMetadata(new TelemetryLapMetadata());
            Assert.That(File.Exists(lapFile.FullRelativePath), Is.True);
            Assert.That(File.Exists(lapFile.MetadataFullRelativePath), Is.True);
        }
    } 
}
