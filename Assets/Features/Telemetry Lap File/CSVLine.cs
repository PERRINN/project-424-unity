using System.Dynamic;
using System.Linq;

namespace Perrinn424.TelemetryLapSystem
{
    public class CSVLine: DynamicObject
    {
        private float [] values;
        private readonly HeadersIndex headersIndex;
        public bool HasValues { get; private set; }
        public CSVLine(string[] headers)
        {
            values = new float[headers.Length];
            headersIndex = new HeadersIndex(headers);
        }

        public float[] Values => values;

        public float this[string header]
        {
            get => values[headersIndex[header]];
            set => values[headersIndex[header]] = value;
        }

        public void UpdateValues(CSVLine other)
        {
            UpdateValues(other.Values);
        }

        public void UpdateValues(string line)
        {
            float[] aux = line.Split(',').Select(v => float.Parse(v, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
            UpdateValues(aux);
        }

        public void UpdateValues(float[] newValues)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = newValues[i];
            }

            HasValues = true;
        }

        public bool Contains(string header)
        {
            return headersIndex.HasHeader(header);
        }

        public int Sector => (int)this["SECTOR"];
        public float Time
        {
            get => this["TIME"];
            set => this["TIME"] = value;
        }

        public float Distance
        {
            get => this["DISTANCE"];
            set => this["DISTANCE"] = value;
        }

        public float Speed
        {
            get => this["SPEED"];
        }


        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {

            string header = binder.Name.ToUpper();

            result = null;
            if (!Contains(header))
            {
                return false;
            }
            result = this[header];
            return true;
        }

        // If you try to set a value of a property that is
        // not defined in the class, this method is called.
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            string header = binder.Name.ToUpper();

            if (!Contains(header))
            {
                return false;
            }
            this[header] = (float)value;
            return true;
        }
    } 
}
