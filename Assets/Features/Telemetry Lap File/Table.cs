using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Perrinn424.TelemetryLapSystem
{
    [Serializable]
    public class Table : ISerializationCallbackReceiver
    {
        [SerializeField]
        private SerializedDenseArray matrix;
        private HeadersIndex headerIndex;

        public int RowCount => rowCount;
        public int ColumnCount => columnCount;

        public string[] Headers => headers;
        public string[] Units => units;

        [SerializeField]
        private int rowCount;
        [SerializeField]
        private int columnCount;
        [SerializeField]
        private string [] headers;
        [SerializeField]
        private string [] units;

        public static Table FromCSV(string csv)
        {
            StringReader reader = new StringReader(csv);
            var table = FromTextReader(reader);
            reader.Dispose();
            return table;
        }

        public static Table FromStream(StreamReader stream)
        {
            return FromTextReader(stream);
        }

        private static Table FromTextReader(TextReader textReader)
        {
            Table table = new Table();
            CSVLine csvLine = new CSVLine(new string[0]);
            List<float> tempValues = new List<float>();
            int lineCount = 0;
            string stringLine;
            while ((stringLine = textReader.ReadLine()) != null)
            {
                if (lineCount == 0)
                {

                    table.headers = stringLine.Split(',');
                    table.headerIndex = new HeadersIndex(table.Headers);
                    csvLine = new CSVLine(table.Headers);

                }
                else if (lineCount == 1)
                {
                    table.units = stringLine.Split(',');
                }
                else
                {
                    csvLine.UpdateValues(stringLine);
                    tempValues.AddRange(csvLine.Values);
                }

                lineCount++;
            }

            table.rowCount = lineCount - 2; //header and units
            table.columnCount = csvLine.Values.Length;
            table.matrix = new SerializedDenseArray(table.RowCount, table.ColumnCount);
            table.matrix.collection = tempValues.ToArray();
            return table;
        }


        public IEnumerable<float> GetRow(int rowIndex)
        {
            for (int column = 0; column < ColumnCount; column++)
            {
                yield return this[rowIndex, column];
            }
        }

        public IEnumerable<IEnumerable<float>> GetRows()
        {
            for (int row = 0; row < rowCount; row++)
            {
                yield return GetRow(row);
            }
        }

        public IEnumerable<float> GetColumn(int columnIndex)
        {
            for (int row = 0; row < RowCount; row++)
            {
                yield return this[row, columnIndex];
            }
        }

        public IEnumerable<float> GetColumn(string header)
        {
            return GetColumn(headerIndex[header]);
        }

        public void OnBeforeSerialize(){}

        public void OnAfterDeserialize()
        {
            headerIndex = new HeadersIndex(headers);
        }

        public float this[int rowIndex, int columnIndex]
        {
            get
            {
                //row-major
                int index = rowIndex * ColumnCount + columnIndex;
                return matrix[index];
            }
        }

        public float this[int rowIndex, string header] => this[rowIndex, headerIndex[header]];

        public float GetValue(int rowIndex, string header)
        {
            return this[rowIndex, headerIndex[header]];
        }

        public bool TryGetValue(int rowIndex, string header, out float value)
        {
            if (headerIndex.HasHeader(header))
            {
                value = this[rowIndex, headerIndex[header]];
                return true;
            }

            value = float.NaN;
            return false;
        }

        public IEnumerable<float> this[string header]
        {
            get => this.GetColumn(header);
        }
    } 
}
