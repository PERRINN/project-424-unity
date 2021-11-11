using System;
using System.Collections.Generic;
using System.Linq;
using VehiclePhysics;

namespace Perrinn424.LapFileSystem
{
    public class LapFileTool : VehicleBehaviour
    {
        public int frequency;
        public Dataset dataset;
        public TelemetryLap telemetryLap;
        public float[] row;
        public RowHeader header;
        void Start()
        {
            dataset = new Dataset(8, 3);
            dataset.Write(new[] { 1f, 2f, 3f });
            dataset.Write(new[] { 4f, 5f, 6f });
            dataset.Write(new[] { 7f, 8f, 9f });
        }

        public override void OnEnableVehicle()
        {
            var headerCount = RowHeader.ParamCount;
            var channelsCount = vehicle.telemetry.channels.Count;
            int width = headerCount + channelsCount;
            row = new float[width];

            List<string> headers = RowHeader.Headers.Split(',').ToList();
            foreach (Telemetry.ChannelInfo ci in vehicle.telemetry.channels)
            {
                headers.Add(ci.fullName.ToUpper());
            }

            int expectedLapTime = 6 * 60; // 6 minutes (normally is about 5)
            int cacheCount = expectedLapTime * frequency;
            telemetryLap = new TelemetryLap(headers, cacheCount);
        }

        public override void OnDisableVehicle()
        {
        }

        private void FixedUpdate()
        {
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
            row[6] = header.markers;
            row[7] = header.markerTime;
            row[8] = Convert.ToSingle(header.markerFlag);

            for (int i = 0, c = dataRow.values.Length; i < c; i++)
            {
                row[i + RowHeader.ParamCount] = dataRow.values[i];
            }

            telemetryLap.Write(row);
        }

    }
}
