using Perrinn424.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        public string TempFullRelativePath { get; private set; }

        private string separator = ",";
        private IFormatProvider invariantCulture;

        public string MetadataFullRelativePath { get; private set; }

        StringBuilder builder = new StringBuilder();

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

        private IReadOnlyList<string> headers;
        public LapFileWriter(IReadOnlyList<string> headers)
        {
            this.headers = headers;

        }

        public bool IsRecordingReady { get; private set; }
        public void StartRecording()
        {
            string root = "Telemetry";
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }

            TempFullRelativePath = Path.Combine(root, "temp.csv");

            FileStream fs = new FileStream(TempFullRelativePath, FileMode.Create, FileAccess.Write);
            fileWriter = new CSVFileWriter(fs);
            HeadersWritten = false;
            WriteHeaders(headers);
            IsRecordingReady = true;
        }

        public void StopRecordingAndSaveFile(LapFileMetadata meta)
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

            fileWriter.Dispose();
            File.Move(TempFullRelativePath, FullRelativePath); // Rename the oldFileName into newFileName

            meta.csvFile = FullRelativePath;
            string json = JsonUtility.ToJson(meta, true);
            MetadataFullRelativePath = $"{FullRelativePath}.metadata";
            File.WriteAllText(MetadataFullRelativePath, json);

            IsRecordingReady = false;
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
            builder.Length = 0;

            foreach (float v in row)
            {
                builder.AppendFormat(invariantCulture,"{0:F2},", v);
            }

            builder.Length = builder.Length - 1;

            string line = builder.ToString();
            //string line = string.Format(invariantCulture, "{0:F5},{1:F5}", row);
            //string line = String.Join(separator, row.Select(v => v.ToString("F5", invariantCulture)));
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
