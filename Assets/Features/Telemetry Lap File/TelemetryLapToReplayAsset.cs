using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using VehiclePhysics;

namespace Perrinn424.TelemetryLapSystem
{
    public static class TelemetryLapToReplayAsset
    {
        public static VPReplayAsset Create(string metadataPath)
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
    } 
}
