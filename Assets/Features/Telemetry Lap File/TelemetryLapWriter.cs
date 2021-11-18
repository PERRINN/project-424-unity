using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
        private Frequency frequency;

        private RowHeader rowHeader;
        private float[] rowCache;

        [SerializeField]
        private string[] channels;
        private int [] channelsIndex;

        private TelemetryLapMetadata metadata;
        private TelemetryLapFileWriter file;

        public override void OnEnableVehicle()
        {
            lapTimer.onBeginLap += LapBeginEventHandler;
            lapTimer.onLap += LapCompletedEventHandler;
            frequency.Reset();

            GetChannelsIndex();

            List<string> headers = GetHeaders();

            file = new TelemetryLapFileWriter(headers);
        }

        private void GetChannelsIndex()
        {
            channelsIndex = Enumerable.Repeat(-1, channels.Length).ToArray();
            IList<Telemetry.ChannelInfo> channelList = vehicle.telemetry.channels;

            for (int totalChannelIndex = 0; totalChannelIndex < channelList.Count; totalChannelIndex++)
            {
                Telemetry.ChannelInfo info = channelList[totalChannelIndex];
                for (int fileChannelIndex = 0; fileChannelIndex < channels.Length; fileChannelIndex++)
                {
                    if (info.fullName == channels[fileChannelIndex])
                    {
                        channelsIndex[fileChannelIndex] = totalChannelIndex;
                    }
                }
            }
        }

        private List<string> GetHeaders()
        {
            var rowHeaderCount = RowHeader.ParamCount;
            var channelsCount = channels.Length;
            int width = rowHeaderCount + channelsCount;
            rowCache = new float[width];
            List<string> headers = RowHeader.Headers.Split(',').ToList();
            headers.AddRange(channels.Select(c => c.ToUpper()));
            return headers;
        }


        private void LapBeginEventHandler()
        {
            file.StartRecording();
            Log($"Recording at {file.TempFullRelativePath}");
        }

        private void LapCompletedEventHandler(float lapTime, bool validBool, float[] sectors, bool[] validSectors)
        {
            metadata = new TelemetryLapMetadata()
            {
                frequency = frequency,
                lapIndex = vehicle.telemetry.latest.segmentNum,
                lapTime = lapTime,
                completed = true,
                completedSectors = sectors.Length,
                sectorsTime = sectors
            };

            SaveFile(metadata);
        }

        public override void OnDisableVehicle()
        {
            lapTimer.onBeginLap -= LapBeginEventHandler;
            lapTimer.onLap -= LapCompletedEventHandler;

        }

        private void FixedUpdate()
        {
            if (frequency.Update(Time.deltaTime)  && file.IsRecordingReady)
            {
                WriteLine();
            }
        }

        private void WriteLine()
        {
            Telemetry.DataRow dataRow = vehicle.telemetry.latest;
            WriteHeaders(dataRow);
            WriteChannels(dataRow);
            file.WriteRow(rowCache);
        }

        private void WriteHeaders(Telemetry.DataRow dataRow)
        {
            rowHeader.frame = dataRow.frame;
            rowHeader.time = dataRow.time;
            rowHeader.distance = dataRow.distance;
            rowHeader.totalTime = dataRow.totalTime;
            rowHeader.totalDistance = dataRow.totalDistance;
            rowHeader.segmentNum = dataRow.segmentNum;
            rowHeader.markers = dataRow.markers;
            rowHeader.markerTime = dataRow.markerTime;
            rowHeader.markerFlag = dataRow.markerFlag;

            rowCache[0] = rowHeader.frame;
            rowCache[1] = (float)rowHeader.time;
            rowCache[2] = (float)rowHeader.distance;
            rowCache[3] = (float)rowHeader.totalTime;
            rowCache[4] = (float)rowHeader.totalDistance;
            rowCache[5] = rowHeader.segmentNum;
            rowCache[6] = lapTimer.currentSector;
            rowCache[7] = rowHeader.markers;
            rowCache[8] = rowHeader.markerTime;
            rowCache[9] = Convert.ToSingle(rowHeader.markerFlag);
        }

        private void WriteChannels(Telemetry.DataRow dataRow)
        {
            for (int i = 0; i < channelsIndex.Length; i++)
            {
                int valueIndex = channelsIndex[i];
                rowCache[i + RowHeader.ParamCount] = dataRow.values[valueIndex];
            }
        }

        private void SaveFile(TelemetryLapMetadata meta)
        {
            file.StopRecordingAndSaveFile(meta);
            Log($"File Saved at {file.FullRelativePath}");
        }

        private void Log(string str)
        {
            if (!log)
                return;

            Debug.Log(str);
        }

        private void OnApplicationQuit()
        {
            if (!file.IsRecordingReady)
                return;

            metadata = new TelemetryLapMetadata()
            {
                frequency = frequency,
                lapIndex = vehicle.telemetry.latest.segmentNum,
                lapTime = lapTimer.currentLapTime,
                completed = false,
                completedSectors = lapTimer.currentSector,
                sectorsTime = new float[0]
            };

            SaveFile(metadata);
        }
    }
}
