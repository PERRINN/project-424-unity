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
                return 6;
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

                var timeSemantic = new Telemetry.SemanticInfo();
                timeSemantic.SetRangeAndFormat(-10, 10, "0.000", " s");
                channelInfo[4].SetNameAndSemantic("919TimeDiff", Telemetry.Semantic.Custom, timeSemantic);
                channelInfo[5].SetNameAndSemantic("IDRTimeDiff", Telemetry.Semantic.Custom, timeSemantic);
            }

            public override Telemetry.PollFrequency GetPollFrequency()
            {
                return Telemetry.PollFrequency.Normal;
            }

            public override void PollValues(float[] values, int index, Object instance)
            {
                values[index + 0] = performanceBenchmarkController.Porsche919.Speed;
                values[index + 1] = performanceBenchmarkController.IDR.Speed;
                values[index + 2] = performanceBenchmarkController.Porsche919.TraveledDistance;
                values[index + 3] = performanceBenchmarkController.IDR.TraveledDistance;
                values[index + 4] = performanceBenchmarkController.Porsche919.TimeDiff;
                values[index + 5] = performanceBenchmarkController.IDR.TimeDiff;
            }
        }
    } 
}
