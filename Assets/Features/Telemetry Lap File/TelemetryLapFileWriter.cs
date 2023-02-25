﻿using Perrinn424.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;

namespace Perrinn424.TelemetryLapSystem
{
    public class TelemetryLapFileWriter
    {
        private CSVFileWriter fileWriter;
        public string Filename { get; private set; }
        public string FullPath { get; private set; }
        public string TempFullPath { get; private set; }
        public string MetadataFullPath { get; private set; }
        public bool HeadersWritten { get; private set; }
        public int ColumnCount { get; private set; }
        public int LineCount { get; private set; }

        private const string separator = ",";
        private readonly IFormatProvider invariantCulture;
        private readonly StringBuilder builder;
        private readonly TimeFormatter timeFormatter;


        public IReadOnlyList<string> Headers { get; private set; }
        public IReadOnlyList<string> Units { get; private set; }
        public bool IsRecordingReady { get; private set; }

        public TelemetryLapFileWriter(IReadOnlyList<string> headers, IReadOnlyList<string> units)
        {
            this.Headers = headers;
            this.Units = units;

            invariantCulture = System.Globalization.CultureInfo.InvariantCulture;
            builder = new StringBuilder();
            timeFormatter = new TimeFormatter(TimeFormatter.Mode.MinutesAndSeconds, @"mm\.ss\.fff", @"ss\.fff");
        }

        public void StartRecording()
        {
            TempFullPath = FolderManager.Combine("temp.csv");

            FileStream fs = new FileStream(TempFullPath, FileMode.Create, FileAccess.Write);
            fileWriter = new CSVFileWriter(fs);
            
            HeadersWritten = false;
            WriteHeaders(Headers, Units);

            LineCount = 0;
            IsRecordingReady = true;
        }

        public void StopRecordingAndSaveFile(bool isCompleted, bool isIdeal, float lapTime)
        {
            SetFileNames(isCompleted, isIdeal, lapTime);
            DisposeFileAndRename();
            IsRecordingReady = false;
        }

        private void SetFileNames(bool isCompleted, bool isIdeal, float lapTime)
        {
            string dateStr = DateTime.UtcNow.ToString("yyyy-MM-dd HH.mm.ss UTC", invariantCulture);
            string lapTimeStr = isCompleted ? timeFormatter.ToString(lapTime) : "unfinished";
            string ideal = isIdeal ? " ideal" : string.Empty;

            Filename = $"{dateStr} {lapTimeStr}{ideal}.csv";
            FullPath = FolderManager.Combine(Filename);
        }

        private void DisposeFileAndRename()
        {
            fileWriter.Dispose();
            FolderManager.Rename(TempFullPath, FullPath); // Rename the oldFileName into newFileName
        }

        public void WriteMetadata(TelemetryLapMetadata meta)
        {
            string json = JsonUtility.ToJson(meta, true);
            MetadataFullPath = $"{FullPath}.metadata";
            File.WriteAllText(MetadataFullPath, json);
        }

        public void WriteHeaders(IEnumerable<string> headers, IEnumerable<string> units)
        {
            if (HeadersWritten)
            {
                throw new InvalidOperationException("Headers already written");
            }

            if (headers.Any(h => ValidateHeader(h)))
            {
                throw new FormatException($"string:{separator} is not allowed in headers");
            }

            if (units.Any(h => ValidateHeader(h)))
            {
                throw new FormatException($"string:{separator} is not allowed in header units");
            }

            if (headers.Count() != units.Count())
            {
                throw new FormatException($"Headers and header units must have the same length");
            }

            ColumnCount = headers.Count();
            string headerLine = String.Join(separator, headers);
            fileWriter.WriteLine(headerLine);

            string headerUnitsLine = String.Join(separator, units);
            fileWriter.WriteLine(headerUnitsLine);

            HeadersWritten = true;
        }

        public void WriteRow(IEnumerable<float> row)
        {
            builder.Length = 0;

            foreach (float v in row)
            {
                builder.AppendFormat(invariantCulture,"{0:F5},", v);
            }

            builder.Length = builder.Length - 1;

            string line = builder.ToString();
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

        public void Delete()
        {
            FolderManager.Delete(FullPath);
            FolderManager.Delete(MetadataFullPath);
            FolderManager.Delete(TempFullPath);
        }
    } 
}
