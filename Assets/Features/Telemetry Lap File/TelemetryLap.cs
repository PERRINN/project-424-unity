using System.Collections.Generic;

namespace Perrinn424.TelemetryLapSystem
{
    //TODO remove
    public class TelemetryLap
    {
        public Dataset data;
        public IReadOnlyList<string> Headers { get; private set; }

        public TelemetryLap(IReadOnlyList<string> headers, int cacheCount)
        {
            this.Headers = headers;
            data = new Dataset(cacheCount, headers.Count);
        }

        public void Write(IEnumerable<float> dataRow)
        {
            data.Write(dataRow);
        }

        public void Reset()
        {
            data.Reset();
        }
    } 
}
