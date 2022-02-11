using System.Collections.Generic;

namespace Perrinn424.TelemetryLapSystem
{
    //TODO remove

    [System.Serializable]
    public struct Dataset
    {
        public int rows;
        public int width;
        public int rowCount;
        //https://answers.unity.com/questions/1485842/serialize-custom-multidimensional-array-from-inspe.html
        //https://en.wikipedia.org/wiki/Row-_and_column-major_order
        //https://stackoverflow.com/questions/2151084/map-a-2d-array-onto-a-1d-array
        public List<float> data;

        public Dataset(int expectedRowCount, int width)
        {
            this.rows = expectedRowCount;
            this.width = width;
            this.rowCount = 0;
            data = new List<float>(expectedRowCount * width);
        }

        public float this[int rowIndex, int columnIndex]
        {
            get => data[rowIndex*width + columnIndex];
        }

        public void Write(IEnumerable<float> dataRow)
        {
            foreach (float value in dataRow)
            {
                data.Add(value);
            }

            rowCount++;
        }

        public void Reset()
        {
            data.Clear();
            rowCount = 0;
        }
    } 
}
