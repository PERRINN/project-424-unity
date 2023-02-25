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
        private float[] unitsMultiplier;

        public float[] Frequencies { get; private set; }

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

            BuildInfoArrays();
        }

        private void BuildInfoArrays()
        {
            GetChannelsIndex();
            GetUnits();
            GetFrequencies();
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
            var semantics =
                channelsIndex
                .Select(index => vehicle.telemetry.GetChannelSemmantic(index));

            Units = semantics.Select(semantic => semantic.displayUnits).ToArray();
            unitsMultiplier = semantics.Select(semantic => semantic.displayMultiplier).ToArray();
        }

        private void GetFrequencies()
        {

            Frequencies = channelsIndex.Select(GetFrequency).ToArray();
        }

        private float GetFrequency(int channelIndex)
        {
            if (channelIndex == channelNotFoundIndex)
            {
                return -1.0f;
            }

            return vehicle.telemetry.channels[channelIndex].group.actualFrequency;
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
                    BuildInfoArrays();
                    return;
                }
            }
        }

        public float GetValue(int index)
        {
            if (IsChannelValidAndActive(index))
            {
                int telemetryChannelIndex = GetTelemetryChannelIndex(index);
                return DataRow.values[telemetryChannelIndex] * unitsMultiplier[index];
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
