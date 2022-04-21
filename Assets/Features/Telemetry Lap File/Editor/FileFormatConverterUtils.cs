using Perrinn424.AutopilotSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VehiclePhysics;
using Path = System.IO.Path;

namespace Perrinn424.TelemetryLapSystem.Editor
{
    public static class FileFormatConverterUtils
    {
        public static VPReplayAsset CSVToReplayAsset(string metadataPath)
        {
            TelemetryLapMetadata metadata = JsonUtility.FromJson<TelemetryLapMetadata>(File.ReadAllText(metadataPath));
            CheckHeaders(metadata.headers);

            string directoryPath = Path.GetDirectoryName(metadataPath);
            string telemetryPath = Path.Combine(directoryPath, metadata.csvFile);

            CSVLine csvLine = new CSVLine(metadata.headers);

            VPReplayAsset asset = ScriptableObject.CreateInstance<VPReplayAsset>();
            asset.timeStep = 1f / metadata.frequency;
            asset.notes = $"created synthetically from {telemetryPath}";
            asset.recordedData = new List<VPReplay.Frame>();//TODO pre allocate with meta.count
            asset.sceneName = metadata.trackName;


            IEnumerable<string> enumerable = File.ReadLines(telemetryPath);
            IEnumerator<string> enumerator = enumerable.GetEnumerator();
            enumerator.MoveNext();//skip headers
            enumerator.MoveNext();//skip units


            foreach (string line in enumerable)
            {
                csvLine.UpdateValues(line);

                VPReplay.Frame currentFrame = new VPReplay.Frame();

                float x = csvLine["POSITIONX"];
                float y = csvLine["POSITIONY"];
                float z = csvLine["POSITIONZ"];

                float eulerX = csvLine["ROTATIONX"];
                float eulerY = csvLine["ROTATIONY"];
                float eulerZ = csvLine["ROTATIONZ"];

                currentFrame.position = new Vector3(x, y, z);
                currentFrame.rotation = Quaternion.Euler(eulerX, eulerY, eulerZ);

                currentFrame.inputData = new int[InputData.Max];

                currentFrame.inputData[InputData.Steer] = (int)(csvLine["RAWSTEER"]);
                currentFrame.inputData[InputData.Throttle] = (int)(csvLine["RAWTHROTTLE"]);
                currentFrame.inputData[InputData.Brake] = (int)(csvLine["RAWBRAKE"]);
                currentFrame.inputData[InputData.AutomaticGear] = (int)(csvLine["AUTOMATICGEAR"]);

                asset.recordedData.Add(currentFrame);
            }

            asset.name = Path.GetFileNameWithoutExtension(metadata.csvFile);
            return asset;
        }

        private static void CheckHeaders(string[] headers)
        {
            string[] requiredHeaders = new[] {
                "POSITIONX", "POSITIONY", "POSITIONZ",
                "ROTATIONX", "ROTATIONY", "ROTATIONZ",
                "RAWSTEER", "RAWTHROTTLE", "RAWBRAKE", "AUTOMATICGEAR"
            };

            foreach (string requiredHeader in requiredHeaders)
            {
                if (!headers.Contains(requiredHeader))
                {
                    throw new ArgumentException($"{requiredHeader} header not found");
                }
            }
        }

        public static TelemetryLapAsset CSVToTelemetryLapAsset(string metadataPath)
        {
            TelemetryLapMetadata metadata = JsonUtility.FromJson<TelemetryLapMetadata>(File.ReadAllText(metadataPath));

            string directoryPath = Path.GetDirectoryName(metadataPath);
            string telemetryPath = Path.Combine(directoryPath, metadata.csvFile);

            TelemetryLapAsset asset = ScriptableObject.CreateInstance<TelemetryLapAsset>();
            asset.metadata = metadata;

            using (var s = new StreamReader(telemetryPath))
            {
                asset.table = Table.FromStream(s);
            }

            asset.name = Path.GetFileNameWithoutExtension(metadata.csvFile);
            return asset;
        }

        public static VPReplayAsset TelemetryLapToReplayAsset(TelemetryLapAsset telemetryLap)
        {
            VPReplayAsset replayAsset = ScriptableObject.CreateInstance<VPReplayAsset>();
            TelemetryLapMetadata metadata = telemetryLap.metadata;
            replayAsset.timeStep = 1f / metadata.frequency;
            replayAsset.notes = $"created synthetically from {AssetDatabase.GetAssetPath(telemetryLap)}";
            replayAsset.sceneName = metadata.trackName;

            List <VPReplay.Frame> recordedData = new List<VPReplay.Frame>();//TODO pre allocate with meta.count
            Table table = telemetryLap.table;

            for (int rowIndex = 0; rowIndex < table.RowCount; rowIndex++)
            {
                VPReplay.Frame currentFrame = new VPReplay.Frame();

                float x = table[rowIndex,"POSITIONX"];
                float y = table[rowIndex, "POSITIONY"];
                float z = table[rowIndex, "POSITIONZ"];

                float eulerX = table[rowIndex, "ROTATIONX"];
                float eulerY = table[rowIndex, "ROTATIONY"];
                float eulerZ = table[rowIndex, "ROTATIONZ"];

                currentFrame.position = new Vector3(x, y, z);
                currentFrame.rotation = Quaternion.Euler(eulerX, eulerY, eulerZ);

                currentFrame.inputData = new int[InputData.Max];

                currentFrame.inputData[InputData.Steer] = (int)(table[rowIndex, "RAWSTEER"]);
                currentFrame.inputData[InputData.Throttle] = (int)(table[rowIndex, "RAWTHROTTLE"]);
                currentFrame.inputData[InputData.Brake] = (int)(table[rowIndex, "RAWBRAKE"]);
                currentFrame.inputData[InputData.AutomaticGear] = (int)(table[rowIndex, "AUTOMATICGEAR"]);

                recordedData.Add(currentFrame);
            }

            replayAsset.name = telemetryLap.name;
            replayAsset.recordedData = recordedData;
            return replayAsset;
        }

        public static RecordedLap ReplayAssetToRecordedLap(VPReplayAsset replayAsset)
        {
            RecordedLap recordedLap = ScriptableObject.CreateInstance<RecordedLap>();
            recordedLap.lapTime = replayAsset.recordedData.Count * replayAsset.timeStep;
            recordedLap.frequency = 1f/replayAsset.timeStep;

            var replayFrames = replayAsset.recordedData;
            List<Sample> samples = new List<Sample>(replayFrames.Count);

            for (int i = 0; i < replayFrames.Count; i++)
            {
                VPReplay.Frame replayFrame = replayFrames[i];
                Sample sample = new Sample()
                {
                    position = replayFrame.position,
                    rotation = replayFrame.rotation,
                    rawSteer = replayFrame.inputData[InputData.Steer],
                    rawThrottle = replayFrame.inputData[InputData.Throttle],
                    rawBrake = replayFrame.inputData[InputData.Brake],
                    automaticGear = replayFrame.inputData[InputData.AutomaticGear],
                    gear = 0,
                    steeringAngle = float.NaN,
                    throttle = float.NaN,
                    brakePressure = float.NaN

                };

                samples.Add(sample);
            }

            recordedLap.name = replayAsset.name;
            recordedLap.samples = samples;
            return recordedLap;
        }

        public static RecordedLap TelemetryLapToRecordedLap(TelemetryLapAsset telemetryLap)
        {
            RecordedLap recordedLap = ScriptableObject.CreateInstance<RecordedLap>();
            TelemetryLapMetadata metadata = telemetryLap.metadata;
            recordedLap.frequency = metadata.frequency;
            recordedLap.lapTime = metadata.lapTime;

            Table table = telemetryLap.table;
            List<Sample> samples = new List<Sample>(table.RowCount);

            for (int rowIndex = 0; rowIndex < table.RowCount; rowIndex++)
            {
                Sample sample = new Sample();

                float x = table[rowIndex, "POSITIONX"];
                float y = table[rowIndex, "POSITIONY"];
                float z = table[rowIndex, "POSITIONZ"];

                float eulerX = table[rowIndex, "ROTATIONX"];
                float eulerY = table[rowIndex, "ROTATIONY"];
                float eulerZ = table[rowIndex, "ROTATIONZ"];

                sample.position = new Vector3(x, y, z);
                sample.rotation = Quaternion.Euler(eulerX, eulerY, eulerZ);


                sample.rawSteer = (int)(table[rowIndex, "RAWSTEER"]);
                sample.rawThrottle = (int)(table[rowIndex, "RAWTHROTTLE"]);
                sample.rawBrake = (int)(table[rowIndex, "RAWBRAKE"]);
                sample.automaticGear = (int)(table[rowIndex, "AUTOMATICGEAR"]);

                sample.gear = (int)table[rowIndex, "GEAR"];
                sample.steeringAngle = table[rowIndex, "STEERINGANGLE"];
                sample.throttle = table[rowIndex, "THROTTLE"];
                sample.brakePressure = table[rowIndex, "BRAKEPRESSURE"];

                table.TryGetValue(rowIndex, "AERODRSPOSITION", out sample.drsPosition);

                samples.Add(sample);
            }

            recordedLap.name = telemetryLap.name;
            recordedLap.samples = samples;
            return recordedLap;
        }
    }
}
