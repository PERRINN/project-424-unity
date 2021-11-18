using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VehiclePhysics;
using VehiclePhysics.Timing;

namespace Perrinn424.LapFileSystem
{
    public class LapFileTool : VehicleBehaviour
    {
        [SerializeField]
        private LapTimer lapTimer;

        [SerializeField]
        private Frequency frequency;

        private TelemetryLap telemetryLap;
        private RowHeader rowHeader;
        private float[] rowCache;

        [SerializeField]
        private string[] channels;
        private int [] channelsIndex;

        private LapFileMetadata metadata;

        public override void OnEnableVehicle()
        {
            lapTimer.onBeginLap += LapBeginEventHandler;
            lapTimer.onLap += LapCompletedEventHandler;

            frequency.Reset();

            var headerCount = RowHeader.ParamCount;
            var channelsCount = channels.Length;
            int width = headerCount + channelsCount;
            rowCache = new float[width];
            List<string> headers = RowHeader.Headers.Split(',').ToList();

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

            headers.AddRange(channels.Select(c => c.ToUpper()));

            int expectedLapTime = 6 * 60; // 6 minutes (normally is about 5)
            int cacheCount = expectedLapTime * frequency;
            telemetryLap = new TelemetryLap(headers, cacheCount);
        }

        private void LapBeginEventHandler()
        {
            Debug.Log("Lap Begin");
            telemetryLap.Reset();
        }

        private void LapCompletedEventHandler(float lapTime, bool validBool, float[] sectors, bool[] validSectors)
        {
            Debug.Log("Lap completed");

            metadata = new LapFileMetadata()
            {
                frequency = frequency,
                lapIndex = vehicle.telemetry.latest.segmentNum,
                lapTime = lapTime,
                sectorsTime = sectors,
                headers = telemetryLap.Headers.ToArray(),
                count = telemetryLap.data.rowCount
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
            if (frequency.Update(Time.deltaTime))
            {
                WriteLine();
            }
        }

        private void WriteLine()
        {
            Telemetry.DataRow dataRow = vehicle.telemetry.latest;
            WriteHeaders(dataRow);
            WriteChannels(dataRow);
            
            telemetryLap.Write(rowCache);
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

        private void SaveFile(LapFileMetadata meta)
        {

            using (LapFileWriter file = new LapFileWriter(meta))
            {
                file.WriteHeaders(telemetryLap.Headers);


                for (int rowIndex = 0; rowIndex < telemetryLap.data.rowCount; rowIndex++)
                {
                    for (int columnIndex = 0; columnIndex < telemetryLap.data.width; columnIndex++)
                    {
                        rowCache[columnIndex] = telemetryLap.data[rowIndex, columnIndex];
                    }

                    file.WriteRowSafe(rowCache);
                }

                Debug.Log($"File Saved {file.FullRelativePath}");
            }
        }

        //private void OnApplicationQuit()
        //{
        //    SaveFile(new LapFileMetadata());
        //}
    }
}
