using System.Dynamic;
using System.Linq;

namespace Perrinn424.TelemetryLapSystem
{
    public class CSVLine: DynamicObject
    {
        private float [] values;
        private readonly HeadersIndex headersIndex;
        private readonly char separatorCharacter;
        public bool HasValues { get; private set; }
        public CSVLine(string[] headers, char separatorCharacter)
        {
            values = new float[headers.Length];
            headersIndex = new HeadersIndex(headers);
            this.separatorCharacter = separatorCharacter;
        }

        public CSVLine(string[] headers): this(headers, ','){}

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
            string[] stringValues = line.Split(separatorCharacter);
            float [] newValues = new float[stringValues.Length];

            for (int i = 0; i < stringValues.Length; i++)
            {
                float newValue;
                if (float.TryParse(stringValues[i], out newValue))
                {
                    newValues[i] = newValue;
                }
                else
                {
                    newValues[i] = float.NaN;
                }
            }


            //float[] aux = line.Split(separatorCharacter).Select(v => float.Parse(v, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
            UpdateValues(newValues);
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

        public int Sector => (int)this["Sector"];
        public float Time
        {
            get => this["Time"];
            set => this["Time"] = value;
        }

        public float Distance
        {
            get => this["Distance"];
            set => this["Distance"] = value;
        }

        public float Speed
        {
            get => this["Speed"];
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
