using UnityEngine;
using VehiclePhysics;

namespace Perrinn424.SpeedEstimatorSystem
{
    public class KFSpeedEstimatorTelemetryProvider : BaseTelemetryProvider<KFSpeedEstimator, KFSpeedEstimatorTelemetryProvider.KFSpeedEstimatorTelemetry>
    {
        public class KFSpeedEstimatorTelemetry : Telemetry.ChannelGroup
        {
            private KFSpeedEstimator KFSpeedEstimator;

            public override int GetChannelCount()
            {
                return 3;
            }

            public override void GetChannelInfo(Telemetry.ChannelInfo[] channelInfo, Object instance)
            {
                KFSpeedEstimator = (KFSpeedEstimator)instance;
                channelInfo[0].SetNameAndSemantic("KFEstimatedSpeed", Telemetry.Semantic.Speed);

                var errorSemantic = new Telemetry.SemanticInfo();
                errorSemantic.SetRangeAndFormat(-50, 50, "0.00", " %", multiplier: 1.0f, quantization: 1);

                channelInfo[1].SetNameAndSemantic("KFEstimatedSpeedError", Telemetry.Semantic.Custom, errorSemantic);

                var distanceSemantic = new Telemetry.SemanticInfo();
                distanceSemantic.SetRangeAndFormat(0, 21000, "0.000", " km", multiplier: 0.001f, quantization: 1000);

                channelInfo[2].SetNameAndSemantic("KFEstimatedLapDistance", Telemetry.Semantic.Custom, distanceSemantic);
            }

            public override Telemetry.PollFrequency GetPollFrequency()
            {
                return Telemetry.PollFrequency.Normal;
            }

            public override void PollValues(float[] values, int index, Object instance)
            {
                values[index + 0] = KFSpeedEstimator.KFSpeed;
                values[index + 1] = KFSpeedEstimator.KFError;
                values[index + 2] = KFSpeedEstimator.EstimatedLapDistance;
            }
        }
    } 
}
