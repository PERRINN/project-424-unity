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

			public override int GetChannelCount()
			{
                return 5;
            }

            public override void GetChannelInfo(Telemetry.ChannelInfo[] channelInfo, Object instance)
            {
				autopilot = (Autopilot)instance;

				Telemetry.SemanticInfo errorSemantic = new Telemetry.SemanticInfo();
				errorSemantic.SetRangeAndFormat(-5, 5, "0.000", " m");

				float range = Mathf.Max(autopilot.maxForceP, autopilot.maxForceD);
				Telemetry.SemanticInfo pidSemantic = new Telemetry.SemanticInfo();
				pidSemantic.SetRangeAndFormat(-range, range, "0", " N");

				channelInfo[0].SetNameAndSemantic("AutopilotDistanceError", Telemetry.Semantic.Custom, errorSemantic);
				channelInfo[1].SetNameAndSemantic("AutopilotPID_P", Telemetry.Semantic.Custom, pidSemantic);
				channelInfo[2].SetNameAndSemantic("AutopilotPID_I", Telemetry.Semantic.Custom, pidSemantic);
				channelInfo[3].SetNameAndSemantic("AutopilotPID_D", Telemetry.Semantic.Custom, pidSemantic);
				channelInfo[4].SetNameAndSemantic("AutopilotPID_PID", Telemetry.Semantic.Custom, pidSemantic);
			}

			public override Telemetry.PollFrequency GetPollFrequency()
			{
				return Telemetry.PollFrequency.Normal;
			}

            public override void PollValues(float[] values, int index, Object instance)
            {
				values[index + 0] = autopilot.Error;
				values[index + 1] = autopilot.P;
				values[index + 2] = autopilot.I;
				values[index + 3] = autopilot.D;
				values[index + 4] = autopilot.PID;
            }
        }
	}
}
