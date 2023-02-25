using System.Collections.Generic;
using System.Linq;

namespace Perrinn424.TelemetryLapSystem
{
    public class HeadersIndex
    {
        private readonly Dictionary<string, int> headersIndex;

        public HeadersIndex(string[] headers)
        {
            headersIndex =
            headers
                .Select((header, index) => new { header, index })
                .ToDictionary(x => x.header, x => x.index);
        }

        public int this[string header] => headersIndex[header];

        public bool HasHeader(string header)
        {
            return headersIndex.ContainsKey(header);
        }
    } 
}
