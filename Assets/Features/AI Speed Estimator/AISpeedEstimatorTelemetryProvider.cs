using UnityEngine;
using VehiclePhysics;

namespace Perrinn424.AISpeedEstimatorSystem
{
    public class AISpeedEstimatorTelemetryProvider : BaseTelemetryProvider<AISpeedEstimatorContainer, AISpeedEstimatorTelemetryProvider.AISpeedEstimatorTelemetry>
    {
        public class AISpeedEstimatorTelemetry : Telemetry.ChannelGroup
        {
            private AISpeedEstimatorContainer aiSpeedEstimator;

            public override int GetChannelCount()
            {
                return 3;
            }

            public override void GetChannelInfo(Telemetry.ChannelInfo[] channelInfo, Object instance)
            {
                aiSpeedEstimator = (AISpeedEstimatorContainer)instance;
                channelInfo[0].SetNameAndSemantic("AIEstimatedSpeed", Telemetry.Semantic.Speed);

                var errorSemantic = new Telemetry.SemanticInfo();
                errorSemantic.SetRangeAndFormat(-50, 50, "0.00", " %", multiplier: 1.0f, quantization: 1);

                channelInfo[1].SetNameAndSemantic("AIEstimatedSpeedError", Telemetry.Semantic.Custom, errorSemantic);

                var distanceSemantic = new Telemetry.SemanticInfo();
                distanceSemantic.SetRangeAndFormat(0, 21000, "0.000", " km", multiplier: 0.001f, quantization: 1000);

                channelInfo[2].SetNameAndSemantic("AIEstimatedLapDistance", Telemetry.Semantic.Custom, distanceSemantic);
            }

            public override Telemetry.PollFrequency GetPollFrequency()
            {
                return Telemetry.PollFrequency.Normal;
            }

            public override void PollValues(float[] values, int index, Object instance)
            {
                values[index + 0] = aiSpeedEstimator.EstimatedSpeed;
                values[index + 1] = aiSpeedEstimator.Error;
                values[index + 2] = aiSpeedEstimator.EstimatedLapDistance;
            }
        }

    }
}
