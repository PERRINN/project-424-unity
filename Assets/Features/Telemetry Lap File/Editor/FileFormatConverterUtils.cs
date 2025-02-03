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
                currentFrame.inputData[InputData.Retarder] = (int)(csvLine["LIFTANDCOAST"]);

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
                "RAWSTEER", "RAWTHROTTLE", "RAWBRAKE", "AUTOMATICGEAR", "LIFTANDCOAST"
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
                currentFrame.inputData[InputData.Retarder] = (int)(table[rowIndex, "LIFTANDCOAST"]);

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
                    gear = 0,
                    steeringAngle = float.NaN,
                    throttle = float.NaN,
                    brake = float.NaN,
                    speed = float.NaN

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
            recordedLap.timestamp = metadata.timeStamp;
            recordedLap.frequency = metadata.frequency;
            recordedLap.lapTime = metadata.lapTime;

            Table table = telemetryLap.table;
            List<Sample> samples = new List<Sample>(table.RowCount);

            for (int rowIndex = 0; rowIndex < table.RowCount; rowIndex++)
            {
                Sample sample = new Sample();

                sample.speed = table[rowIndex, "Speed"] / 3.6f; //km/h => m/s

                float x = table[rowIndex, "PositionX"];
                float y = table[rowIndex, "PositionY"];
                float z = table[rowIndex, "PositionZ"];

                float eulerX = table[rowIndex, "RotationX"];
                float eulerY = table[rowIndex, "RotationY"];
                float eulerZ = table[rowIndex, "RotationZ"];

                sample.position = new Vector3(x, y, z);
                sample.rotation = Quaternion.Euler(eulerX, eulerY, eulerZ);

                sample.gear = (int)table[rowIndex, "Gear"];
                sample.steeringAngle = table[rowIndex, "SteeringAngle"];
                sample.throttle = table[rowIndex, "Throttle"];

                // Assume old format if there's no BrakePressure header
                if (!table.TryGetValue(rowIndex, "Brake", out sample.brake))
                    sample.brake = table[rowIndex, "BrakePressure"];

                table.TryGetValue(rowIndex, "AeroDrsPosition", out sample.drsPosition);
                if (table.TryGetValue(rowIndex, "LiftAndCoast", out float floatValue))
                    sample.liftAndCoast = (int)floatValue;

                samples.Add(sample);
            }

            recordedLap.name = telemetryLap.name;
            recordedLap.samples = samples;
            return recordedLap;
        }
    }
}
