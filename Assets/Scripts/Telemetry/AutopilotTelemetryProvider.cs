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
                return 14;
            }

            public override void GetChannelInfo(Telemetry.ChannelInfo[] channelInfo, Object instance)
            {
				autopilot = (Autopilot)instance;
				pid = (IPIDInfo)instance;

				Telemetry.SemanticInfo errorSemantic = new Telemetry.SemanticInfo();
				errorSemantic.SetRangeAndFormat(-1, 1, "0.0", " mm", multiplier:1000);

				float range = Mathf.Max(pid.MaxForceP, pid.MaxForceD);
				Telemetry.SemanticInfo pidSemantic = new Telemetry.SemanticInfo();
				pidSemantic.SetRangeAndFormat(-range, range, "0", " N");

				//PID info
				channelInfo[0].SetNameAndSemantic("AutopilotDistanceError", Telemetry.Semantic.Custom, errorSemantic);
				channelInfo[1].SetNameAndSemantic("AutopilotPID_P", Telemetry.Semantic.Custom, pidSemantic);
				channelInfo[2].SetNameAndSemantic("AutopilotPID_I", Telemetry.Semantic.Custom, pidSemantic);
				channelInfo[3].SetNameAndSemantic("AutopilotPID_D", Telemetry.Semantic.Custom, pidSemantic);
				channelInfo[4].SetNameAndSemantic("AutopilotPID", Telemetry.Semantic.Custom, pidSemantic);

				//Reference Info
				channelInfo[5].SetNameAndSemantic("AutopilotReferenceSpeed", Telemetry.Semantic.Speed);

				var timeSemantic = new Telemetry.SemanticInfo();
                timeSemantic.SetRangeAndFormat(-10, 10, "0.000", " s");
                channelInfo[6].SetNameAndSemantic("AutopilotDeltaTime", Telemetry.Semantic.Custom, timeSemantic);

                channelInfo[7].SetNameAndSemantic("AutopilotReferenceThrottle", Telemetry.Semantic.Ratio);
				channelInfo[8].SetNameAndSemantic("AutopilotReferenceBrake", Telemetry.Semantic.Ratio);
				channelInfo[9].SetNameAndSemantic("AutopilotReferenceSteeringAngle", Telemetry.Semantic.SteeringWheelAngle);
				channelInfo[10].SetNameAndSemantic("AutopilotReferenceDRS", Telemetry.Semantic.Ratio);
				channelInfo[11].SetNameAndSemantic("AutopilotReferenceLiftAndCoast", Telemetry.Semantic.Boolean);

				//Status Info
				var flagSemantic = new Telemetry.SemanticInfo();
				flagSemantic.SetRangeAndFormat(0, 1, "True;;False", "");
                channelInfo[12].SetNameAndSemantic("AutopilotIsOn", Telemetry.Semantic.Custom, flagSemantic);
                channelInfo[13].SetNameAndSemantic("AutopilotIsStartup", Telemetry.Semantic.Custom, flagSemantic);
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
				values[index + 8] = autopilot.ReferenceSample.brake/100f;
				values[index + 9] = autopilot.ReferenceSample.steeringAngle;
				values[index + 10] = autopilot.ReferenceSample.drsPosition/100;
				values[index + 11] = autopilot.ReferenceSample.liftAndCoast;

				//Status Info
				//we insert 1,-1 instead of 1,0 to be able to use conditional format
				//https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-numeric-format-strings
				//EDY: Why don't just use the three-section conditional format, "True;;False", instead of feeding the telemetry with formatting-related values?
				values[index + 12] = autopilot.IsOn ? 1 : 0;
				values[index + 13] = autopilot.IsStartup ? 1 : 0;
			}
		}
	}
}
