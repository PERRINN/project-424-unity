using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using VehiclePhysics;
using VehiclePhysics.Timing;

namespace Perrinn424.TelemetryLapSystem
{
    public class TelemetryLapWriter : VehicleBehaviour
    {
        [SerializeField]
        private bool log;

        [SerializeField]
        private LapTimer lapTimer;

        [SerializeField]
        private bool disableOnBuilds = true;

        [SerializeField]
        private Frequency frequency;

        private DataRow dataRow;
        private float[] rowCache;

        [SerializeField]
        private Channels channels;

        private TelemetryLapFileWriter file;

        private List<TelemetryLapMetadata> telemetryLapMetadatas;

        public override void OnEnableVehicle()
        {
            if (disableOnBuilds && !Application.isEditor)
                {
                // OnDisableVehicle won't be called if the component is disabled here
                enabled = false;
                return;
                }

            lapTimer.onBeginLap += LapBeginEventHandler;
            lapTimer.onLap += LapCompletedEventHandler;

            frequency.Reset();
            channels.Reset(vehicle);

            List<string> headers = new List<string>();
            List<string> units = new List<string>();
            GetHeadersAndUnits(headers, units);

            file = new TelemetryLapFileWriter(headers, units);

            telemetryLapMetadatas = new List<TelemetryLapMetadata>();
        }

        public override void OnDisableVehicle()
        {
            lapTimer.onBeginLap -= LapBeginEventHandler;
            lapTimer.onLap -= LapCompletedEventHandler;
            SaveOnAbruptExit();
        }

        private void GetHeadersAndUnits(List<string> headers, List<string> units)
        {
            var dataRowCount = DataRow.ParamCount;
            var channelsCount = channels.Length;
            int width = dataRowCount + channelsCount;

            rowCache = new float[width];

            headers.AddRange(DataRow.Headers);
            headers.AddRange(channels.GetHeaders());

            units.AddRange(DataRow.Units);
            units.AddRange(channels.Units);
        }


        private void LapBeginEventHandler()
        {
            file.StartRecording();
            Log($"Recording at {file.TempFullRelativePath}");
        }

        private void LapCompletedEventHandler(float lapTime, bool validBool, float[] sectors, bool[] validSectors)
        {
            Save(true, lapTime, () => CreateRegularLapMetadata(lapTime, sectors));
        }

        private void FixedUpdate()
        {
            if (frequency.Update(Time.deltaTime)  && file.IsRecordingReady)
            {
                channels.RefreshIfNeeded();
                WriteLine();
            }
        }

        private void WriteLine()
        {
            WriteDataRowInCache();
            WriteChannelsInCache();
            WriteCacheInFile();
        }

        private void WriteDataRowInCache()
        {
            Telemetry.DataRow telemetryDataRow = vehicle.telemetry.latest;

            dataRow.frame = telemetryDataRow.frame;
            dataRow.time = telemetryDataRow.time;
            dataRow.distance = telemetryDataRow.distance;
            dataRow.totalTime = telemetryDataRow.totalTime;
            dataRow.totalDistance = telemetryDataRow.totalDistance;
            dataRow.segmentNum = telemetryDataRow.segmentNum;
            dataRow.markers = telemetryDataRow.markers;
            dataRow.markerTime = telemetryDataRow.markerTime;
            dataRow.markerFlag = telemetryDataRow.markerFlag;

            rowCache[0] = dataRow.frame;
            rowCache[1] = (float)dataRow.time;
            rowCache[2] = (float)dataRow.distance;
            rowCache[3] = (float)dataRow.totalTime;
            rowCache[4] = (float)dataRow.totalDistance;
            rowCache[5] = dataRow.segmentNum;
            rowCache[6] = lapTimer.currentSector;
            rowCache[7] = dataRow.markers;
            rowCache[8] = dataRow.markerTime;
            rowCache[9] = Convert.ToSingle(dataRow.markerFlag);
        }

        private void WriteChannelsInCache()
        {
            for (int i = 0; i < channels.Length; i++)
            {
                rowCache[i + DataRow.ParamCount] = channels.GetValue(i);
            }
        }

        private void WriteCacheInFile()
        {
            file.WriteRow(rowCache);
        }

        private void Save(bool isCompleted, float lapTime, Func<TelemetryLapMetadata> createMetadata)
        {
            try
            {
                file.StopRecordingAndSaveFile(isCompleted, false, lapTime);
                TelemetryLapMetadata metadata = createMetadata();
                file.WriteMetadata(metadata);
                telemetryLapMetadatas.Add(metadata);
                Log($"File Saved at {file.FullRelativePath}");
            }
            catch (Exception)
            {
                Debug.LogWarning($"Error saving {file.FullRelativePath}. Disabling CSV writing");
                this.enabled = false;
            }
        }


        private TelemetryLapMetadata CreateRegularLapMetadata(float lapTime, float[] sectors)
        {
            return CreateCommonMetadata(lapTime, true, sectors.Length, sectors.ToArray()); //sectors.ToArray() => make copy
        }

        private TelemetryLapMetadata CreateUnfinishLapMetadata()
        {
            var completedSectors = lapTimer.currentValidSectors.Count(validSector => validSector);

            //if the sector is valid, get its time. Infinity otherwise
            var sectorsTime =
                lapTimer
                .currentValidSectors
                .Select((validSector, index) => validSector ? lapTimer.currentSectors[index] : float.PositiveInfinity)
                .ToArray();

            return CreateCommonMetadata(lapTimer.currentLapTime, false, completedSectors, sectorsTime);
        }

        private TelemetryLapMetadata CreateCommonMetadata(float lapTime, bool isCompleted, float completedSectors, float [] sectorsTime)
        {

            TelemetryLapMetadata metadata = new TelemetryLapMetadata()
            {
                trackName = SceneManager.GetActiveScene().name,
                fileFormatVersion = 1,
                frequency = frequency,
                lapIndex = vehicle.telemetry.latest.segmentNum,
                lapTime = lapTime,
                completed = isCompleted,
                completedSectors = completedSectors,
                sectorsTime = sectorsTime.ToArray(),
                headers = file.Headers.ToArray(),
                headerUnits = file.Units.ToArray(),
                channels = channels.GetHeaders().ToArray(),
                channelsFrequency = channels.Frequencies.ToArray(),
                count = file.LineCount,
                csvFile = file.Filename,
                timeStamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                ideal = false,
                idealSectorOrigin = new string[0],
            };

            return metadata;
        }

        private void Log(string str)
        {
            if (!log)
                return;

            Debug.Log(str);
        }

        private void SaveOnAbruptExit()
        {
            if (file.IsRecordingReady)
            {
                Save(false, lapTimer.currentLapTime, CreateUnfinishLapMetadata);
            }

            if (telemetryLapMetadatas.Count > 1)
            {
                IdealTelemetryLapCreator.CreateSyntheticTelemetryLap(telemetryLapMetadatas);
            }
        }
    }
}
