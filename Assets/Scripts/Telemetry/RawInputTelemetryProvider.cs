using UnityEngine;
using VehiclePhysics;

public class RawInputTelemetryProvider : VehicleBehaviour
{
	public bool emitTelemetry = true;


    public override bool EmitTelemetry()
	{
		return emitTelemetry;
	}

	public override void RegisterTelemetry()
	{
		vehicle.telemetry.Register<RawInputTelemetry>(vehicle);
	}


	public override void UnregisterTelemetry()
	{
		vehicle.telemetry.Unregister<RawInputTelemetry>(vehicle);
	}

	private class RawInputTelemetry : Telemetry.ChannelGroup
	{
		VehicleBase vehicle;
		public override int GetChannelCount()
		{
			return 4;
		}

		public override void GetChannelInfo(Telemetry.ChannelInfo[] channelInfo, Object instance)
		{
			vehicle = (VehicleBase)instance;

			channelInfo[0].SetNameAndSemantic("RawSteer", Telemetry.Semantic.Default);
			channelInfo[1].SetNameAndSemantic("RawThrottle", Telemetry.Semantic.Default);
			channelInfo[2].SetNameAndSemantic("RawBrake", Telemetry.Semantic.Default);
			channelInfo[3].SetNameAndSemantic("AutomaticGear", Telemetry.Semantic.Default);
		}

		public override Telemetry.PollFrequency GetPollFrequency()
		{
			return Telemetry.PollFrequency.Normal;
		}

		public override void PollValues(float[] values, int index, Object instance)
		{
			int[] input = vehicle.data.Get(Channel.Input);

			values[index + 0] = input[InputData.Steer];
			values[index + 1] = input[InputData.Throttle];
			values[index + 2] = input[InputData.Brake];
			values[index + 3] = input[InputData.AutomaticGear];
		}
	}
}
