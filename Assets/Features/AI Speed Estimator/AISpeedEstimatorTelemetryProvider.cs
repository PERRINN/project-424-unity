using Perrinn424;
using UnityEngine;
using VehiclePhysics;

public class AISpeedEstimatorTelemetryProvider : BaseTelemetryProvider<AISpeedEstimator, AISpeedEstimatorTelemetryProvider.AISpeedEstimatorTelemetry>
{
    public class AISpeedEstimatorTelemetry : Telemetry.ChannelGroup
    {
        private AISpeedEstimator aiSpeedEstimator;

        public override int GetChannelCount()
        {
            return 2;
        }

        public override void GetChannelInfo(Telemetry.ChannelInfo[] channelInfo, Object instance)
        {
            aiSpeedEstimator = (AISpeedEstimator)instance;
            channelInfo[0].SetNameAndSemantic("AIEstimatedSpeed", Telemetry.Semantic.Speed);
            channelInfo[1].SetNameAndSemantic("AIEstimatedSpeedError", Telemetry.Semantic.Speed);
        }

        public override Telemetry.PollFrequency GetPollFrequency()
        {
            return Telemetry.PollFrequency.Normal;
        }

        public override void PollValues(float[] values, int index, Object instance)
        {
            values[index + 0] = aiSpeedEstimator.EstimatedSpeed;
            values[index + 1] = aiSpeedEstimator.Error;
        }
    }

}
