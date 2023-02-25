using System.IO;

namespace Perrinn424.TelemetryLapSystem
{
    public class CSVFileWriter : StreamWriter
    {
        public CSVFileWriter(Stream stream): base(stream)
        {
        }

        public CSVFileWriter(string filename): base(filename)
        {
        }
    } 
}
