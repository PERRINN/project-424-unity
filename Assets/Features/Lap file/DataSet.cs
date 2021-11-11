using System.Collections.Generic;

namespace Perrinn424.LapFileSystem
{
    [System.Serializable]
    public struct Dataset
    {
        public int rows;
        public int width;
        public int writtenRows;
        //https://answers.unity.com/questions/1485842/serialize-custom-multidimensional-array-from-inspe.html
        //https://en.wikipedia.org/wiki/Row-_and_column-major_order
        //https://stackoverflow.com/questions/2151084/map-a-2d-array-onto-a-1d-array
        public List<float> data;

        public Dataset(int rows, int width) : this()
        {
            this.rows = rows;
            this.width = width;
            this.writtenRows = 0;
            data = new List<float>(rows * width);
        }

        public void Write(IEnumerable<float> dataRow)
        {
            ////major-row access  
            //for (int columnIndex = 0; columnIndex < newRow.Length; columnIndex++)
            //{
            //    data.Add(newRow[columnIndex]);
            //}

            foreach (float value in dataRow)
            {
                data.Add(value);
            }

            writtenRows++;
        }
    } 
}
