using Perrinn424.AutopilotSystem;
using UnityEngine;
using VehiclePhysics;

namespace Perrinn424
{
    public class AutopilotTelemetryProvider : BaseTelemetryProvider<BaseAutopilot, AutopilotTelemetryProvider.AutopilotTelemetry>
	{
		public class AutopilotTelemetry : Telemetry.ChannelGroup
		{
			private IPIDInfo pid;

			public override int GetChannelCount()
			{
                return 5;
            }

            public override void GetChannelInfo(Telemetry.ChannelInfo[] channelInfo, Object instance)
            {
				pid = (IPIDInfo)instance;

				Telemetry.SemanticInfo errorSemantic = new Telemetry.SemanticInfo();
				errorSemantic.SetRangeAndFormat(-5, 5, "0.000", " m");

				float range = Mathf.Max(pid.MaxForceP, pid.MaxForceD);
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
				values[index + 0] = pid.Error;
				values[index + 1] = pid.P;
				values[index + 2] = pid.I;
				values[index + 3] = pid.D;
				values[index + 4] = pid.PID;
            }
        }
	}
}
