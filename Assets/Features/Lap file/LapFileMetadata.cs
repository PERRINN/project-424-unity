namespace Perrinn424.LapFileSystem
{
    [System.Serializable]
    public class LapFileMetadata
    {
        public int frequency;
        public int lapIndex;
        public float lapTime;
        public float[] sectorsTime;
        public string[] headers;
        public int count;
        public string csvFile;
    } 
}
