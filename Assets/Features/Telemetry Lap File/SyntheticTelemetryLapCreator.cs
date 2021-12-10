using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Perrinn424.TelemetryLapSystem
{
    public class SyntheticTelemetryLapCreator
    {
        private IReadOnlyList<TelemetryLapMetadata> telemetryLapMetadatas;
        private LapTimeTable timeTable;

        private Dictionary<int, string> lapToFilename;
        public string[] headers;


        public static void CreateSyntheticTelemetryLap(IReadOnlyList<TelemetryLapMetadata> telemetryLapMetadatas)
        {
            new SyntheticTelemetryLapCreator(telemetryLapMetadatas).CreateSyntheticFile();
        }

        public SyntheticTelemetryLapCreator(IReadOnlyList<TelemetryLapMetadata> telemetryLapMetadatas)
        {
            this.telemetryLapMetadatas = telemetryLapMetadatas;
            lapToFilename = new Dictionary<int, string>();
            timeTable = new LapTimeTable(5);
        }

        private void CreateSyntheticFile()
        {
            foreach (TelemetryLapMetadata metadata in telemetryLapMetadatas)
            {
                ProcessMetadata(metadata);
            }

            int[] bestLapsPerSector = timeTable.GetBestLapForEachSector();


            FileStream fs = new FileStream("Telemetry/sync.csv", FileMode.Create, FileAccess.Write);
            CSVFileWriter fileWriter = new CSVFileWriter(fs);
            fileWriter.WriteLine(string.Join(",", headers));

            TelemetryLapFileWriter telemetryLapFileWriter = new TelemetryLapFileWriter(headers);
            telemetryLapFileWriter.StartRecording();

            for (int sectorIndex = 0; sectorIndex < 5; sectorIndex++)
            {
                int indexOfbestLapInSectorI = bestLapsPerSector[sectorIndex];
                string filename = lapToFilename[indexOfbestLapInSectorI];
                string msg = $"Best lap in sector {sectorIndex} is {filename}";
                print(msg);
                Read(sectorIndex, filename, fileWriter, telemetryLapFileWriter);
            }

            fileWriter.Dispose();

            TelemetryLapMetadata bestLapInFirstSector = telemetryLapMetadatas[bestLapsPerSector[0]];

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

            //Read(3, indexDictionary[3]);
        }

        private void ProcessMetadata(TelemetryLapMetadata telemetryLapMetadata)
        {
            if (telemetryLapMetadata.sectorsTime.Length == 5)
            {
                lapToFilename[timeTable.LapCount] = telemetryLapMetadata.csvFile;
                timeTable.AddLap(telemetryLapMetadata.sectorsTime);
            }

            headers = telemetryLapMetadata.headers;
        }

        private void Read(int sector, string filename, CSVFileWriter csv, TelemetryLapFileWriter telemetryLapFileWriter)
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

            int count = 1;
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
                    Debug.Log($"Sector {sector} starts in line {count} with {line}");
                    inSector = true;
                }

                if (currentSector != sector && inSector)
                {
                    Debug.Log($"Sector {sector} ends in line {count}  with {line}");
                    inSector = false;
                    return;
                }

                if (inSector)
                {
                    csv.WriteLine(line);
                    telemetryLapFileWriter.WriteRow(values);
                }

                count++;
            }
        }

        private void print(string s)
        {
            Debug.Log(s);
        }
    }

}