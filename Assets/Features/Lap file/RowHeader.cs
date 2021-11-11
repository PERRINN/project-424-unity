namespace Perrinn424.LapFileSystem
{
    public struct RowHeader
    {
        public int frame;
        public double time;
        public double distance;
        public double totalTime;
        public double totalDistance;
        public int segmentNum;
        public int markers;
        public float markerTime;
        public bool markerFlag;

        public static int ParamCount => 9;
        public static string Headers => "FRAME,TIME,DISTANCE,TOTALTIME, TOTALDISTANCE,SEGMENTNUM,MARKERS,MARKERTIME,MARKERFLAG";
    } 
}
