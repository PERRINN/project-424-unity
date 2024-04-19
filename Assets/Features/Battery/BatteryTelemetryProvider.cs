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
				return 9;
			}

			public override void GetChannelInfo(Telemetry.ChannelInfo[] channelInfo, Object instance)
			{
				battery = (Battery)instance;


				Telemetry.SemanticInfo totalHeatSemantic = new Telemetry.SemanticInfo();
				totalHeatSemantic.SetRangeAndFormat(0, 120000, "0", " W");
				channelInfo[0].SetNameAndSemantic("BatteryTotalHeat", Telemetry.Semantic.Custom, totalHeatSemantic);

        Telemetry.SemanticInfo airMassFlowSemantic = new Telemetry.SemanticInfo();
        airMassFlowSemantic.SetRangeAndFormat(0, 5, "0.0", " kg/s");
        channelInfo[1].SetNameAndSemantic("BatteryAirMassFlow", Telemetry.Semantic.Custom, airMassFlowSemantic);

        Telemetry.SemanticInfo heatDissipationSemantic = new Telemetry.SemanticInfo();
        heatDissipationSemantic.SetRangeAndFormat(0, 4000, "0.0", " W/degC");
        channelInfo[2].SetNameAndSemantic("BatteryHeatDissipation", Telemetry.Semantic.Custom, heatDissipationSemantic);

        Telemetry.SemanticInfo heatSemantic = new Telemetry.SemanticInfo();
        heatSemantic.SetRangeAndFormat(-200, 200, "0.0", " J");
        channelInfo[3].SetNameAndSemantic("BatteryHeatDissipated", Telemetry.Semantic.Custom, heatSemantic);

        Telemetry.SemanticInfo temperatureSemantic = new Telemetry.SemanticInfo();
        temperatureSemantic.SetRangeAndFormat(0, 75, "0.0", " degC");
        channelInfo[4].SetNameAndSemantic("BatteryTemperature", Telemetry.Semantic.Custom, temperatureSemantic);

        channelInfo[5].SetNameAndSemantic("BatteryQInternal", Telemetry.Semantic.Custom, heatSemantic);

        Telemetry.SemanticInfo netEnergySemantic = new Telemetry.SemanticInfo();
        netEnergySemantic.SetRangeAndFormat(0, battery.Capacity, "0.00", " kWh");
        channelInfo[6].SetNameAndSemantic("BatteryNetEnergy", Telemetry.Semantic.Custom, netEnergySemantic);

        Telemetry.SemanticInfo PowerRMSSemantic = new Telemetry.SemanticInfo();
        PowerRMSSemantic.SetRangeAndFormat(0, 1000, "0.0", " kW");
        channelInfo[7].SetNameAndSemantic("BatteryPowerRMS", Telemetry.Semantic.Custom, PowerRMSSemantic);

        Telemetry.SemanticInfo PowerSemantic = new Telemetry.SemanticInfo();
        PowerSemantic.SetRangeAndFormat(0, 1000, "0.0", " kW");
        channelInfo[8].SetNameAndSemantic("BatteryPower", Telemetry.Semantic.Custom, PowerSemantic);
    }

			public override Telemetry.PollFrequency GetPollFrequency()
			{
				return Telemetry.PollFrequency.Normal;
			}

			public override void PollValues(float[] values, int index, Object instance)
			{
				values[index + 0] = battery.TotalHeat;
        values[index + 1] = battery.AirMassFlow;
        values[index + 2] = battery.HeatDissipation;
        values[index + 3] = battery.HeatDissipated;
        values[index + 4] = battery.TemperatureModule;
        values[index + 5] = battery.HeatInternal;
        values[index + 6] = battery.NetEnergy;
        values[index + 7] = battery.PowerRMS;
        values[index + 8] = battery.Power;
    }
		}
    }
}
