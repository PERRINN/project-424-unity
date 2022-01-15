using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VehiclePhysics;

public class InputCSVProvider : VehicleBehaviour
{
	public bool emitTelemetry = true;

	//CSVInputTelemetry csvinputTelemetry;
 //   public override void OnEnableVehicle()
 //   {
 //   }

    public override bool EmitTelemetry()
	{
		return emitTelemetry;
	}

	public override void RegisterTelemetry()
	{
		vehicle.telemetry.Register<CSVInputTelemetry>(vehicle);
	}


	public override void UnregisterTelemetry()
	{
		vehicle.telemetry.Unregister<CSVInputTelemetry>(vehicle);
	}

	private class CSVInputTelemetry : Telemetry.ChannelGroup
	{
		VehicleBase vehicle;
		public override int GetChannelCount()
		{
			return 4;
		}

		public override void GetChannelInfo(Telemetry.ChannelInfo[] channelInfo, Object instance)
		{
			vehicle = (VehicleBase)instance;

			//Telemetry.SemanticInfo errorSemantic = new Telemetry.SemanticInfo();
			//errorSemantic.SetRangeAndFormat(-5, 5, "0.000", " m");

			//float range = Mathf.Max(autopilot.maxForceP, autopilot.maxForceD);
			//Telemetry.SemanticInfo pidSemantic = new Telemetry.SemanticInfo();
			//pidSemantic.SetRangeAndFormat(-range, range, "0", " N");

			channelInfo[0].SetNameAndSemantic("STEERCSV", Telemetry.Semantic.Default);
			channelInfo[1].SetNameAndSemantic("THROTTLECSV", Telemetry.Semantic.Default);
			channelInfo[2].SetNameAndSemantic("BRAKECSV", Telemetry.Semantic.Default);
			channelInfo[3].SetNameAndSemantic("AUTOMATICGEARCSV", Telemetry.Semantic.Default);
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
