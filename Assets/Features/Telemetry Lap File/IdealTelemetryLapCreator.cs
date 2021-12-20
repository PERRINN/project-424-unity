using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Perrinn424.TelemetryLapSystem
{
    public class IdealTelemetryLapCreator
    {
        private int sectorCount;
        private IReadOnlyList<TelemetryLapMetadata> metadatas;
        private LapTimeTable timeTable;

        private Dictionary<int, string> lapToFilename;
        private string[] headers;

        private IdealTelemetryLapCreatorCorrector corrector;
        //private CSVLine previousLine;
        //private CSVLine currentLine;
        //private CSVLine correctedLine;

        public static TelemetryLapMetadata CreateSyntheticTelemetryLap(IReadOnlyList<TelemetryLapMetadata> telemetryLapMetadatas)
        {
            return new IdealTelemetryLapCreator(telemetryLapMetadatas).CreateSyntheticFile();
        }

        public IdealTelemetryLapCreator(IReadOnlyList<TelemetryLapMetadata> telemetryLapMetadatas)
        {
            this.metadatas = telemetryLapMetadatas;
            this.lapToFilename = new Dictionary<int, string>();
        }

        private TelemetryLapMetadata CreateSyntheticFile()
        {
            ProcessMetadata();

            int[] bestLapsPerSector = timeTable.GetBestLapForEachSector();

            TelemetryLapFileWriter telemetryLapFileWriter = new TelemetryLapFileWriter(headers);
            telemetryLapFileWriter.StartRecording();

            for (int sectorIndex = 0; sectorIndex < sectorCount; sectorIndex++)
            {
                int indexOfbestLapInSectorI = bestLapsPerSector[sectorIndex];
                string filename = lapToFilename[indexOfbestLapInSectorI];
                WriteSectorInFile(sectorIndex, filename, telemetryLapFileWriter);
            }


            TelemetryLapMetadata bestLapInFirstSector = metadatas[bestLapsPerSector[0]];

            float[] sectorsTime = new float[sectorCount];
            string[] origin = new string[sectorCount];
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
                completedSectors = sectorCount,
                lapTime = sectorsTime.Sum(),
                sectorsTime = sectorsTime,
                ideal = true,
                idealSectorOrigin = origin
            };

            telemetryLapFileWriter.StopRecordingAndSaveFile(finalMetadata);

            return finalMetadata;

        }

        private void ProcessMetadata()
        {
            ValidateMetadata();
            
            headers = metadatas[0].headers;
            //previousLine = new CSVLine(headers);
            //currentLine = new CSVLine(headers);
            //correctedLine = new CSVLine(headers);

            ValidateHeaders();
            float dt = 1f / metadatas[0].frequency;
            corrector = new IdealTelemetryLapCreatorCorrector(headers, dt);
            sectorCount = metadatas[0].sectorsTime.Length;
            this.timeTable = new LapTimeTable(sectorCount);


            foreach (TelemetryLapMetadata metadata in metadatas)
            {
                ProcessMetadata(metadata);
            }
        }

        private void ValidateHeaders()
        {
            if (!headers.Contains("SECTOR"))
            {
                throw new ArgumentException($"Headers must contain SECTOR field");
            }

            if (!headers.Contains("TIME"))
            {
                throw new ArgumentException($"Headers must contain TIME field");
            }
        }

        private void ValidateMetadata()
        {
            if (!AllEqual(m => m.sectorsTime, SectorComparer))
            {
                throw new ArgumentException($"All laps must contains the same number of sector and greater than 0");
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

        private bool SectorComparer(float[] a, float [] b)
        {
            return a.Length == b.Length && a.Length > 0;
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


        private void WriteSectorInFile(int sector, string filename, TelemetryLapFileWriter telemetryLapFileWriter)
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

            bool inSector = false;
            foreach (string line in enumerable)
            {
                corrector.ReadLine(line);
                //currentLine.UpdateValues(line);

                int sectorInLine = corrector.LineSector;
                if (sectorInLine == sector && !inSector)
                {
                    //Debug.Log($"Sector {sector} starts in line {lineCount} with {line}");
                    inSector = true;
                }

                if (sectorInLine != sector && inSector)
                {
                    //Debug.Log($"Sector {sector} ends in line {lineCount}  with {line}");
                    inSector = false;
                    return;
                }

                if (inSector)
                {
                    //corrector.Correct(correctedLine, currentLine, previousLine);
                    corrector.Correct();
                    //corrector.Correct(currentLine, previousLine);
                    telemetryLapFileWriter.WriteRow(corrector.CorrectedValues);
                    //previousLine.UpdateValues(currentLine);
                }
            }
        }
    }
}