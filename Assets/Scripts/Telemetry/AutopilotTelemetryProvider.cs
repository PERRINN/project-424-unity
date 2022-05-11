using Perrinn424.AutopilotSystem;
using UnityEngine;
using VehiclePhysics;

namespace Perrinn424
{
    public class AutopilotTelemetryProvider : BaseTelemetryProvider<Autopilot, AutopilotTelemetryProvider.AutopilotTelemetry>
	{
		public class AutopilotTelemetry : Telemetry.ChannelGroup
		{
			private Autopilot autopilot;
			private IPIDInfo pid;

			public override int GetChannelCount()
			{
                return 13;
            }

            public override void GetChannelInfo(Telemetry.ChannelInfo[] channelInfo, Object instance)
            {
				autopilot = (Autopilot)instance;
				pid = (IPIDInfo)instance;

				Telemetry.SemanticInfo errorSemantic = new Telemetry.SemanticInfo();
				errorSemantic.SetRangeAndFormat(-5, 5, "0.000", " m");

				float range = Mathf.Max(pid.MaxForceP, pid.MaxForceD);
				Telemetry.SemanticInfo pidSemantic = new Telemetry.SemanticInfo();
				pidSemantic.SetRangeAndFormat(-range, range, "0", " N");

				//PID info
				channelInfo[0].SetNameAndSemantic("AutopilotDistanceError", Telemetry.Semantic.Custom, errorSemantic);
				channelInfo[1].SetNameAndSemantic("AutopilotPID_P", Telemetry.Semantic.Custom, pidSemantic);
				channelInfo[2].SetNameAndSemantic("AutopilotPID_I", Telemetry.Semantic.Custom, pidSemantic);
				channelInfo[3].SetNameAndSemantic("AutopilotPID_D", Telemetry.Semantic.Custom, pidSemantic);
				channelInfo[4].SetNameAndSemantic("AutopilotPID_PID", Telemetry.Semantic.Custom, pidSemantic);

				//Reference Info
				channelInfo[5].SetNameAndSemantic("Autopilot_Reference_Speed", Telemetry.Semantic.Speed);

				var timeSemantic = new Telemetry.SemanticInfo();
                timeSemantic.SetRangeAndFormat(-10, 10, "0.000", " s");
                channelInfo[6].SetNameAndSemantic("Autopilot_Delta_Time", Telemetry.Semantic.Custom, timeSemantic);

                channelInfo[7].SetNameAndSemantic("Autopilot_Reference_Throttle", Telemetry.Semantic.Ratio);
				channelInfo[8].SetNameAndSemantic("Autopilot_Reference_BrakePressure", Telemetry.Semantic.BrakePressure);
				channelInfo[9].SetNameAndSemantic("Autopilot_Reference_SteeringAngle", Telemetry.Semantic.SteeringWheelAngle);
				channelInfo[10].SetNameAndSemantic("Autopilot_Reference_DRS", Telemetry.Semantic.Ratio);

				//Status Info
				var flagSemantic = new Telemetry.SemanticInfo();
				flagSemantic.SetRangeAndFormat(-1, 1, "True;False", "");
                channelInfo[11].SetNameAndSemantic("Autopilot_IsOn", Telemetry.Semantic.Custom, flagSemantic);
                channelInfo[12].SetNameAndSemantic("Autopilot_IsStartup", Telemetry.Semantic.Custom, flagSemantic);


            }

			public override Telemetry.PollFrequency GetPollFrequency()
			{
				return Telemetry.PollFrequency.Normal;
			}

            public override void PollValues(float[] values, int index, Object instance)
            {
				//PID info
				values[index + 0] = pid.Error;
				values[index + 1] = pid.P;
				values[index + 2] = pid.I;
				values[index + 3] = pid.D;
				values[index + 4] = pid.PID;

				//Reference Info
				values[index + 5] = autopilot.ReferenceSpeed;
				values[index + 6] = autopilot.DeltaTime;
				values[index + 7] = autopilot.ReferenceSample.throttle/100f;
				values[index + 8] = autopilot.ReferenceSample.brakePressure;
				values[index + 9] = autopilot.ReferenceSample.steeringAngle;
				values[index + 10] = autopilot.ReferenceSample.drsPosition/100;

				//Status Info
				//we insert 1,-1 instead of 1,0 to be able to use conditional format
				//https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-numeric-format-strings
				values[index + 11] = autopilot.IsOn ? 1f : -1f;
				values[index + 12] = autopilot.IsStartup ? 1f : -1f;

			}
		}
	}
}
