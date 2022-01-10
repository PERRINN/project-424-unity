namespace Perrinn424.TelemetryLapSystem
{
    public struct DataRow
    {
        public int frame;
        public double time;
        public double distance;
        public double totalTime;
        public double totalDistance;
        public int segmentNum;
        public int sector;
        public int markers;
        public float markerTime;
        public bool markerFlag;

        public static int ParamCount => 10;
        public static string[] Headers = new string[]{ "FRAME", "TIME", "DISTANCE", "TOTALTIME", "TOTALDISTANCE", "SEGMENTNUM", "SECTOR", "MARKERS", "MARKERTIME", "MARKERFLAG" };
        public static string[] Units = new string[]{ "n/a", "s", "m", "s", "m", "n/a", "n/a", "n/a", "n/a", "n/a" };
    } 
}
