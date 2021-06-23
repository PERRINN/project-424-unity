using UnityEngine;
using VehiclePhysics;

namespace Perrinn424
{
    public class PerformanceBenchmarkTelemetryProvider : BaseTelemetryProvider<PerformanceBenchmarkController, PerformanceBenchmarkTelemetryProvider.PerformanceBenchmarkTelemetry>
    {
        public class PerformanceBenchmarkTelemetry : Telemetry.ChannelGroup
        {
            private PerformanceBenchmarkController performanceBenchmarkController;

            public override int GetChannelCount()
            {
                return 2;
            }

            public override void GetChannelInfo(Telemetry.ChannelInfo[] channelInfo, Object instance)
            {
                performanceBenchmarkController = (PerformanceBenchmarkController)instance;

                channelInfo[0].SetNameAndSemantic("PorscheSpeed", Telemetry.Semantic.Speed);
                channelInfo[1].SetNameAndSemantic("VolkswagenSpeed", Telemetry.Semantic.Speed);
            }

            public override Telemetry.PollFrequency GetPollFrequency()
            {
                return Telemetry.PollFrequency.Normal;
            }

            public override void PollValues(float[] values, int index, Object instance)
            {
                values[index + 0] = performanceBenchmarkController.PorscheSpeed;
                values[index + 1] = performanceBenchmarkController.VolkswagenSpeed;
            }
        }
    } 
}
