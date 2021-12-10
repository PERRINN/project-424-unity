using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Perrinn424.TelemetryLapSystem
{
    public class SyntheticTelemetryLapCreator
    {
        private readonly int sectorCount;
        private IReadOnlyList<TelemetryLapMetadata> metadatas;
        private LapTimeTable timeTable;

        private Dictionary<int, string> lapToFilename;
        public string[] headers;


        public static void CreateSyntheticTelemetryLap(IReadOnlyList<TelemetryLapMetadata> telemetryLapMetadatas, int sectorCount)
        {
            new SyntheticTelemetryLapCreator(telemetryLapMetadatas, sectorCount).CreateSyntheticFile();
        }

        public SyntheticTelemetryLapCreator(IReadOnlyList<TelemetryLapMetadata> telemetryLapMetadatas, int sectorCount)
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
                string msg = $"Best lap in sector {sectorIndex} is {filename}";
                print(msg);
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
                synthetic = true,
                syntheticSectorOrigin = origin
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

            if (!SameValue(m => m.fileFormatVersion, (x, y) => x == y))
            {
                throw new ArgumentException($"All laps must have the same file format");
            }

            if (!SameValue(m => m.frequency, (x, y) => x == y))
            {
                throw new ArgumentException($"All laps must have the same frequency");
            }

            if (!SameValue(m => m.trackName, (x, y) => x == y))
            {
                throw new ArgumentException($"All laps must be associated to the same track");
            }

            if (!SameValue(m => m.headers, HeaderComparer))
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

        private bool SameValue<T>(Func<TelemetryLapMetadata, T> getter, Func<T,T,bool> comparer)
        {
            var firstItem = getter(metadatas[0]);
            bool allEqual = metadatas.Skip(1)
              .All(m => comparer(getter(m), firstItem));
            
            return allEqual;
        }

        private void ProcessMetadata(TelemetryLapMetadata telemetryLapMetadata)
        {
            lapToFilename[timeTable.LapCount] = telemetryLapMetadata.csvFile;
            timeTable.AddLap(telemetryLapMetadata.sectorsTime);
        }



        private void Read(int sector, string filename, TelemetryLapFileWriter telemetryLapFileWriter)
        {
            print("-------------------------------------------");
            print(filename);
            if (!filename.Contains("Telemetry"))
            {
                filename = Path.Combine("Telemetry", filename);
            }
            if (!File.Exists(@filename))
            {
                Debug.LogError($"{filename} erroooooooooooooooor");
                return;
            }

            //https://weblogs.asp.net/lorenh/236134
            IEnumerable<string> enumerable = File.ReadLines(@filename);
            IEnumerator<string> enumerator = enumerable.GetEnumerator();
            enumerator.MoveNext();//skip headers
            print(enumerator.Current);
            print("______________");

            string[] headers = enumerator.Current.Split(',');
            int sectorIndex = Array.IndexOf(headers, "SECTOR");

            int lineCount = 1;
            bool inSector = false;
            foreach (string line in enumerable)
            {
                //print(line);
                //count++;
                //if (count > 10)
                //    return;

                float[] values = line.Split(',').Select(v => float.Parse(v, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
                //int currentSector = (int)float.Parse(line.Split(',')[sectorIndex], System.Globalization.CultureInfo.InvariantCulture);
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
                    //csv.WriteLine(line);
                    telemetryLapFileWriter.WriteRow(values);
                }

                lineCount++;
            }
        }

        private void print(string s)
        {
            Debug.Log(s);
        }
    }

}