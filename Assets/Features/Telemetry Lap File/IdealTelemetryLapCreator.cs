using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Perrinn424.TelemetryLapSystem
{
    public class IdealTelemetryLapCreator
    {
        private readonly int sectorCount;
        private IReadOnlyList<TelemetryLapMetadata> metadatas;
        private LapTimeTable timeTable;

        private Dictionary<int, string> lapToFilename;
        private string[] headers;

        public static void CreateSyntheticTelemetryLap(IReadOnlyList<TelemetryLapMetadata> telemetryLapMetadatas, int sectorCount)
        {
            new IdealTelemetryLapCreator(telemetryLapMetadatas, sectorCount).CreateSyntheticFile();
        }

        public IdealTelemetryLapCreator(IReadOnlyList<TelemetryLapMetadata> telemetryLapMetadatas, int sectorCount)
        {
            this.sectorCount = sectorCount;
            this.metadatas = telemetryLapMetadatas;
            this.lapToFilename = new Dictionary<int, string>();
            this.timeTable = new LapTimeTable(sectorCount);
        }

        private void CreateSyntheticFile()
        {
            ProcessMetadata();

            int[] bestLapsPerSector = timeTable.GetBestLapForEachSector();



            TelemetryLapFileWriter telemetryLapFileWriter = new TelemetryLapFileWriter(headers);
            telemetryLapFileWriter.StartRecording();

            for (int sectorIndex = 0; sectorIndex < sectorCount; sectorIndex++)
            {
                int indexOfbestLapInSectorI = bestLapsPerSector[sectorIndex];
                string filename = lapToFilename[indexOfbestLapInSectorI];
                Read(sectorIndex, filename, telemetryLapFileWriter);
            }


            TelemetryLapMetadata bestLapInFirstSector = metadatas[bestLapsPerSector[0]];

            float[] sectorsTime = new float[5];
            string[] origin = new string[5];
            for (int i = 0; i < sectorsTime.Length; i++)
            {
                int indexOfbestLapInSectorI = bestLapsPerSector[i];
                sectorsTime[i] = timeTable[indexOfbestLapInSectorI][i];
                origin[i] = lapToFilename[indexOfbestLapInSectorI];
            }

            TelemetryLapMetadata finalMetadata = new TelemetryLapMetadata()
            {
                trackName = bestLapInFirstSector.trackName,
                fileFormatVersion = bestLapInFirstSector.fileFormatVersion,
                frequency = bestLapInFirstSector.frequency,
                lapIndex = 0,
                completed = true,
                completedSectors = bestLapInFirstSector.completedSectors,
                lapTime = sectorsTime.Sum(),
                sectorsTime = sectorsTime,
                ideal = true,
                idealSectorOrigin = origin
            };
            telemetryLapFileWriter.StopRecordingAndSaveFile(finalMetadata);

        }

        private void ProcessMetadata()
        {
            Validate();

            foreach (TelemetryLapMetadata metadata in metadatas)
            {
                ProcessMetadata(metadata);
            }

            headers = metadatas[0].headers;
        }

        private void Validate()
        {
            if (!metadatas.All(m => m.sectorsTime.Length == sectorCount))
            {
                throw new ArgumentException($"All sectors must be {sectorCount}");
            }

            if (!metadatas.Any(m => m.completed))
            {
                throw new ArgumentException($"At least one lap must be completed");
            }

            if (!AllEqual(m => m.fileFormatVersion, (x, y) => x == y))
            {
                throw new ArgumentException($"All laps must have the same file format");
            }

            if (!AllEqual(m => m.frequency, (x, y) => x == y))
            {
                throw new ArgumentException($"All laps must have the same frequency");
            }

            if (!AllEqual(m => m.trackName, (x, y) => x == y))
            {
                throw new ArgumentException($"All laps must be associated to the same track");
            }

            if (!AllEqual(m => m.headers, HeaderComparer))
            {
                throw new ArgumentException($"All laps must have the same headers");
            }
        }

        private bool HeaderComparer(string [] a, string [] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }

            return true;
        }

        private bool AllEqual<T>(Func<TelemetryLapMetadata, T> getter, Func<T,T,bool> comparer)
        {
            TelemetryLapMetadata firstItem = metadatas[0];
            Func<TelemetryLapMetadata, bool> predicate = metadata => comparer(getter(metadata), getter(firstItem)); // comparison with the first item 

            return
                metadatas
                .Skip(1)
                .All(predicate);            
        }

        private void ProcessMetadata(TelemetryLapMetadata telemetryLapMetadata)
        {
            lapToFilename[timeTable.LapCount] = telemetryLapMetadata.csvFile;
            timeTable.AddLap(telemetryLapMetadata.sectorsTime);
        }


        private void Read(int sector, string filename, TelemetryLapFileWriter telemetryLapFileWriter)
        {
            if (!filename.Contains("Telemetry"))
            {
                filename = Path.Combine("Telemetry", filename);
            }
            
            if (!File.Exists(@filename))
            {
                Debug.LogError($"{filename} not found");
                return;
            }

            //https://weblogs.asp.net/lorenh/236134
            IEnumerable<string> enumerable = File.ReadLines(@filename);
            IEnumerator<string> enumerator = enumerable.GetEnumerator();
            enumerator.MoveNext();//skip headers

            string[] headers = enumerator.Current.Split(',');
            int sectorIndex = Array.IndexOf(headers, "SECTOR");

            int lineCount = 1;
            bool inSector = false;
            foreach (string line in enumerable)
            {

                float[] values = line.Split(',').Select(v => float.Parse(v, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
                
                int currentSector = (int)values[sectorIndex];
                if (currentSector == sector && !inSector)
                {
                    Debug.Log($"Sector {sector} starts in line {lineCount} with {line}");
                    inSector = true;
                }

                if (currentSector != sector && inSector)
                {
                    Debug.Log($"Sector {sector} ends in line {lineCount}  with {line}");
                    inSector = false;
                    return;
                }

                if (inSector)
                {
                    telemetryLapFileWriter.WriteRow(values);
                }

                lineCount++;
            }
        }
    }
}