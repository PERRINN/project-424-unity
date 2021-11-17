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
        public LapTimer lapTimer;

        public int frequency;
        public TelemetryLap telemetryLap;
        public float[] row;
        public RowHeader header;

        public float updateTime;

        public string[] channels;
        public int [] channelsIndex;

        public LapFileMetadata metadata;

        public override void OnEnableVehicle()
        {
            lapTimer.onBeginLap += LapBeginEventHandler;
            lapTimer.onLap += LapCompletedEventHandler;

            updateTime = 1f / frequency;

            var headerCount = RowHeader.ParamCount;
            var channelsCount = channels.Length;
            int width = headerCount + channelsCount;
            row = new float[width];
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

        //lapTime, !m_invalidLap, m_sectors, m_validSectors);
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
            updateTime -= Time.deltaTime;

            if (updateTime > 0f)
                return;

            updateTime = 1f / frequency;


            Telemetry.DataRow dataRow = vehicle.telemetry.latest;
            header.frame = dataRow.frame;
            header.time = dataRow.time;
            header.distance = dataRow.distance;
            header.totalTime = dataRow.totalTime;
            header.totalDistance = dataRow.totalDistance;
            header.segmentNum = dataRow.segmentNum;
            header.markers = dataRow.markers;
            header.markerTime = dataRow.markerTime;
            header.markerFlag = dataRow.markerFlag;

            row[0] = header.frame;
            row[1] = (float)header.time;
            row[2] = (float)header.distance;
            row[3] = (float)header.totalTime;
            row[4] = (float)header.totalDistance;
            row[5] = header.segmentNum;
            row[6] = lapTimer.currentSector;
            row[7] = header.markers;
            row[8] = header.markerTime;
            row[9] = Convert.ToSingle(header.markerFlag);

            for (int i = 0;  i < channelsIndex.Length; i++)
            {
                int valueIndex = channelsIndex[i];
                row[i + RowHeader.ParamCount] = dataRow.values[valueIndex];
            }

            telemetryLap.Write(row);
        }

        //private void OnApplicationQuit()
        //{
        //    SaveFile();
        //}

        private void SaveFile(LapFileMetadata meta)
        {

            using (LapFileWriter file = new LapFileWriter(meta))
            {
                file.WriteHeaders(telemetryLap.Headers);


                for (int rowIndex = 0; rowIndex < telemetryLap.data.rowCount; rowIndex++)
                {
                    for (int columnIndex = 0; columnIndex < telemetryLap.data.width; columnIndex++)
                    {
                        row[columnIndex] = telemetryLap.data[rowIndex, columnIndex];
                    }

                    file.WriteRowSafe(row);
                }

                Debug.Log($"File Saved {file.FullRelativePath}");
            }
        }
    }
}
