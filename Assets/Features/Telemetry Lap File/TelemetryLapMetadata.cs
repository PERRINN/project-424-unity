using UnityEngine;

namespace Perrinn424.TelemetryLapSystem
{
    [System.Serializable]
    public class TelemetryLapMetadata
    {
        public string trackName;
        public int fileFormatVersion;
        public int frequency;
        public int lapIndex;
        public float lapTime;
        public bool completed;
        public float completedSectors;
        public float[] sectorsTime;
        public string[] headers;
        public string[] headerUnits;
        public string [] channels;
        public float [] channelsFrequency;
        public int count;
        public string csvFile;
        public long timeStamp;
        public bool ideal;
        public string[] idealSectorOrigin;


        public TelemetryLapMetadata Copy()
        {
            return JsonUtility.FromJson<TelemetryLapMetadata>(JsonUtility.ToJson(this));
        }
    } 
}
