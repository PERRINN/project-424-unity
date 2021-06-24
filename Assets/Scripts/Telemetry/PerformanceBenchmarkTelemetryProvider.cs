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
                return 4;
            }

            public override void GetChannelInfo(Telemetry.ChannelInfo[] channelInfo, Object instance)
            {
                performanceBenchmarkController = (PerformanceBenchmarkController)instance;

                channelInfo[0].SetNameAndSemantic("919Speed", Telemetry.Semantic.Speed);
                channelInfo[1].SetNameAndSemantic("IDRSpeed", Telemetry.Semantic.Speed);

                var distanceSemantic = new Telemetry.SemanticInfo();
                distanceSemantic.SetRangeAndFormat(0, 21000, "0.000", " km", multiplier: 0.001f);

                channelInfo[2].SetNameAndSemantic("919TraveledDistance", Telemetry.Semantic.Custom, distanceSemantic);
                channelInfo[3].SetNameAndSemantic("IDRTraveledDistance", Telemetry.Semantic.Custom, distanceSemantic);
            }

            public override Telemetry.PollFrequency GetPollFrequency()
            {
                return Telemetry.PollFrequency.Normal;
            }

            public override void PollValues(float[] values, int index, Object instance)
            {
                values[index + 0] = performanceBenchmarkController.Porsche919Speed;
                values[index + 1] = performanceBenchmarkController.IDRSpeed;
                values[index + 2] = performanceBenchmarkController.Porsche919TraveledDistance;
                values[index + 3] = performanceBenchmarkController.IDRTraveledDistance;
            }
        }
    } 
}
