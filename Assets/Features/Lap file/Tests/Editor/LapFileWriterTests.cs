using NUnit.Framework;
using System.IO;

namespace Perrinn424.LapFileSystem
{
    public class LapFileWriterTests
    {
        private LapFileWriter lapFile;

        [SetUp]
        public void SetUp()
        {
            LapFileMetadata metadata = new LapFileMetadata();
            lapFile = new LapFileWriter(metadata);
        }

        [TearDown]
        public void TearDown()
        {
            lapFile.Dispose();
            lapFile.Delete();
        }

        [Test]
        public void CreateFileTest()
        {
            Assert.That(File.Exists(lapFile.FullRelativePath), Is.True);
            Assert.That(File.Exists(lapFile.MetadataFullRelativePath), Is.True);
        }

        [Test]
        public void CreateHeadersTest()
        {
            Assert.Catch(()=>lapFile.WriteHeaders(new[] { "h,1", "h2", "h3" })); //h,1 is invalid because it contains ,

            lapFile.WriteHeaders(new[] { "h1", "h2", "h3" });
            Assert.That(lapFile.HeadersWritten, Is.True);
            Assert.That(lapFile.ColumnCount, Is.EqualTo(3));

            Assert.Catch(() => lapFile.WriteHeaders(new[] { "h1", "h2", "h3" })); //headers already added
        }

        [Test]
        public void WriteLineTest()
        {
            float[] values = { 1.23f, 2.345f, 6.789f };
            Assert.Catch(() => lapFile.WriteRowSafe(values)); //no headers

            values = new[]{ 1.23f, 2.345f, 6.789f, 5.678f };
            lapFile.WriteHeaders(new[] { "h1", "h2", "h3" });
            Assert.Catch(() => lapFile.WriteRowSafe(values)); //headers.Count != line.count

            values = new []{ 1.23f, 2.345f, 6.789f };
            Assert.That(lapFile.LineCount, Is.EqualTo(0));
            lapFile.WriteRow(values);
            Assert.That(lapFile.LineCount, Is.EqualTo(1));
            lapFile.WriteRow(values);
            Assert.That(lapFile.LineCount, Is.EqualTo(2));
        }
    } 
}
