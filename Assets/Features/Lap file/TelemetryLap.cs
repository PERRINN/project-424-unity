using System.Collections.Generic;

namespace Perrinn424.LapFileSystem
{
    public class TelemetryLap
    {
        public Dataset data;
        private IReadOnlyList<string> headers;

        public TelemetryLap(IReadOnlyList<string> headers, int cacheCount)
        {
            this.headers = headers;
            data = new Dataset(cacheCount, headers.Count);
        }

        public void Write(IEnumerable<float> dataRow)
        {
            data.Write(dataRow);
        }
    } 
}
