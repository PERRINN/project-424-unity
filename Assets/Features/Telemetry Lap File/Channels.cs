using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VehiclePhysics;

namespace Perrinn424.TelemetryLapSystem
{
    [Serializable]
    public class Channels
    {
        [SerializeField]
        private string[] channels;
        public string[] Units { get; private set; }

        private int[] channelsIndex;
        private const int channelNotFoundIndex = -1;
        private int lastTelemetryChannelCount;

        private VehicleBase vehicle;

        public int Length => channels.Length;
        private Telemetry.DataRow DataRow => vehicle.telemetry.latest;


        public void Reset(VehicleBase vehicle)
        {
            this.vehicle = vehicle;
            channelsIndex = Enumerable.Repeat(channelNotFoundIndex, channels.Length).ToArray();
            GetChannelsIndex();
            GetUnits();
        }

        private void GetChannelsIndex()
        {
            IReadOnlyList<Telemetry.ChannelInfo> telemetryChannelList = vehicle.telemetry.channels;
            lastTelemetryChannelCount = telemetryChannelList.Count;

            for (int channelIndex = 0; channelIndex < channels.Length; channelIndex++)
            {
                if (IsChannelValidAndActive(channelIndex))
                {
                    continue;
                }

                int telemetryChannelIndex = vehicle.telemetry.GetChannelIndex(channels[channelIndex]);
                SetTelemetryChannelIndex(channelIndex, telemetryChannelIndex);
            }
        }

        private void GetUnits()
        {
            Units =
                channelsIndex
                .Select(index => vehicle.telemetry.GetChannelSemmantic(index).displayUnits)
                .ToArray();
        }

        public void RefreshIfNeeded()
        {
            if (vehicle.telemetry.channels.Count == lastTelemetryChannelCount)
            {
                return;
            }

            foreach (int channelIndex in channelsIndex)
            {
                if (channelIndex == channelNotFoundIndex)
                {
                    GetChannelsIndex();
                    return;
                }
            }

        }

        public float GetValue(int index)
        {
            if (IsChannelValidAndActive(index))
            {
                int telemetryChannelIndex = GetTelemetryChannelIndex(index);
                return DataRow.values[telemetryChannelIndex];
            }

            return float.NaN;
        }

        public IEnumerable<string> GetHeaders()
        {
            return channels.Select(c => c.ToUpper());
        }


        private bool IsChannelValidAndActive(int index)
        {
            int telemetryChannelIndex = GetTelemetryChannelIndex(index);

            if (telemetryChannelIndex == channelNotFoundIndex)
            {
                return false;
            }

            if (vehicle.telemetry.channels[telemetryChannelIndex].group.instance == null) //registered but not active
            {
                return false;
            }

            return true;
        }

        private int GetTelemetryChannelIndex(int channelIndex)
        {
            return channelsIndex[channelIndex];
        }

        private void SetTelemetryChannelIndex(int channelIndex, int telemetryChannelIndex)
        {
            channelsIndex[channelIndex] = telemetryChannelIndex;
        }
    } 
}
