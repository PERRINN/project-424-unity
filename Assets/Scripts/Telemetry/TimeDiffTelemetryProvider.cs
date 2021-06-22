using UnityEngine;
using VehiclePhysics;

namespace Perrinn424
{
    public class TimeDiffTelemetryProvider : BaseTelemetryProvider<TimeDiff919, TimeDiffTelemetryProvider.TimeDiffTelemetry>
    {
        public class TimeDiffTelemetry : Telemetry.ChannelGroup
        {
            private TimeDiff919 timeDiff;

            public override int GetChannelCount()
            {
                return 2;
            }

            public override void GetChannelInfo(Telemetry.ChannelInfo[] channelInfo, Object instance)
            {
                timeDiff = (TimeDiff919)instance;

                channelInfo[0].SetNameAndSemantic("PorscheSpeed", Telemetry.Semantic.Speed);
                channelInfo[1].SetNameAndSemantic("VolkswagenSpeed", Telemetry.Semantic.Speed);
            }

            public override Telemetry.PollFrequency GetPollFrequency()
            {
                return Telemetry.PollFrequency.Normal;
            }

            public override void PollValues(float[] values, int index, Object instance)
            {
                values[index + 0] = timeDiff.PorscheSpeed;
                values[index + 1] = timeDiff.VolkswagenSpeed;
            }
        }
    } 
}
