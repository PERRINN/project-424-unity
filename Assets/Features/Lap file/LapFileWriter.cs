using Perrinn424.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Perrinn424.LapFileSystem
{
    public class LapFileWriter : IDisposable
    {
        private CSVFileWriter fileWriter;
        public string Filename { get; private set; }
        public string FullRelativePath { get; private set; }
        public string FullPath { get; private set; }
        public bool HeadersWritten { get; private set; }
        public int ColumnCount { get; private set; }
        public int LineCount { get; private set; }

        private string separator = ",";
        private IFormatProvider invariantCulture;

        public string MetadataFullRelativePath { get; private set; }

        public LapFileWriter(LapFileMetadata meta)
        {
            TimeFormatter formater = new TimeFormatter(TimeFormatter.Mode.MinutesAndSeconds, @"mm\.ss\.fff", @"ss\.fff");
            string lapTimeStr = formater.ToString(meta.lapTime);
            invariantCulture = System.Globalization.CultureInfo.InvariantCulture;
            string dateStr = DateTime.UtcNow.ToString("yyyy-MM-dd HH.mm.ss UTC", invariantCulture);
            Filename = $"{lapTimeStr} {dateStr}.csv";

            string root = "Telemetry";
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }

            FullRelativePath = Path.Combine(root, Filename);
            FullPath = Path.Combine(Application.dataPath, FullRelativePath);


            fileWriter = new CSVFileWriter(FullRelativePath);

            meta.csvFile = FullRelativePath;
            string json = JsonUtility.ToJson(meta, true);
            MetadataFullRelativePath = $"{FullRelativePath}.metadata";
            File.WriteAllText(MetadataFullRelativePath, json);
        }

        public void Delete()
        {
            if (File.Exists(FullRelativePath))
            {
                File.Delete(FullRelativePath);
            }

            if (File.Exists(MetadataFullRelativePath))
            {
                File.Delete(MetadataFullRelativePath);
            }
        }

        public void Dispose()
        {
            fileWriter.Dispose();
        }

        public void WriteHeaders(IEnumerable<string> headers)
        {
            if (HeadersWritten)
            {
                throw new InvalidOperationException("Headers already written");
            }

            if (headers.Any(h => ValidateHeader(h)))
            {
                throw new FormatException($"string:{separator} is not allowed in headers");
            }
            ColumnCount = headers.Count();
            string line = String.Join(separator, headers);
            fileWriter.WriteLine(line);
            HeadersWritten = true;
        }

        public void WriteRow(IEnumerable<float> row)
        {
            string line = String.Join(separator, row.Select(v => v.ToString("F5", invariantCulture)));
            fileWriter.WriteLine(line);
            LineCount++;
        }

        public void WriteRowSafe(IEnumerable<float> row)
        {
            if (!HeadersWritten)
            {
                throw new InvalidOperationException("No headers found");
            }

            if (row.Count() != ColumnCount)
            {
                throw new InvalidOperationException("The column count of the row doesn't match with the header column count");
            }

            WriteRow(row);
        }

        private bool ValidateHeader(string h)
        {
            return h.Contains(separator);
        }

        //public static void Save(IEnumerable<string> headers, IEnumerable<IEnumerable<float>> values)
        //{
        //    using (LapFileWriter lapFileWriter = new LapFileWriter())
        //    {
        //        lapFileWriter.WriteHeaders(headers);
        //        foreach (var row in values)
        //        {
        //            lapFileWriter.WriteRow(row);
        //        }
        //    }
        //}
    } 
}
