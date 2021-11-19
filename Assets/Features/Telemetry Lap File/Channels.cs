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
        }

        private void GetChannelsIndex()
        {
            IList<Telemetry.ChannelInfo> telemetryChannelList = vehicle.telemetry.channels;
            lastTelemetryChannelCount = telemetryChannelList.Count;

            for (int channelIndex = 0; channelIndex < channels.Length; channelIndex++)
            {
                if (channelsIndex[channelIndex] != channelNotFoundIndex)
                {
                    continue;
                }

                for (int telemetryChannelIndex = 0; telemetryChannelIndex < lastTelemetryChannelCount; telemetryChannelIndex++)
                {
                    Telemetry.ChannelInfo info = telemetryChannelList[telemetryChannelIndex];
                    if (info.fullName == channels[channelIndex])
                    {
                        channelsIndex[channelIndex] = telemetryChannelIndex;
                    }
                }
            }
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

            //if (vehicle.telemetry.channels.Count != lastChannelCount && channelsIndex.Any(index => index == channelNotFoundIndex))
            //{
            //    GetChannelsIndex();
            //}
        }


        public float GetValue(int index)
        {
            int valueIndex = channelsIndex[index];
            float value = valueIndex != channelNotFoundIndex ? DataRow.values[valueIndex] : float.NaN;
            return value;
        }

        public IEnumerable<string> GetHeaders()
        {
            return channels.Select(c => c.ToUpper());
        }
    } 
}
