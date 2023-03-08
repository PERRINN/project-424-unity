using VehiclePhysics;
using UnityEngine;

namespace Perrinn424
{
    public class BatteryTelemetryProvider : BaseTelemetryProvider<Battery, BatteryTelemetryProvider.BatteryTelemetry>
    {
        public class BatteryTelemetry : Telemetry.ChannelGroup
        {
			private Battery battery;

			public override int GetChannelCount()
			{
				return 1;
			}

			public override void GetChannelInfo(Telemetry.ChannelInfo[] channelInfo, Object instance)
			{
				battery = (Battery)instance;


				Telemetry.SemanticInfo temperatureSemantic = new Telemetry.SemanticInfo();
				temperatureSemantic.SetRangeAndFormat(0, 70, "0", " degC");

				//PID info
				channelInfo[0].SetNameAndSemantic("BatteryTemperature", Telemetry.Semantic.Custom, temperatureSemantic);

			}

			public override Telemetry.PollFrequency GetPollFrequency()
			{
				return Telemetry.PollFrequency.Normal;
			}

			public override void PollValues(float[] values, int index, Object instance)
			{
				values[index + 0] = battery.HeatInternal;
			}
		}
    } 
}
