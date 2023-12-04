using UnityEngine;
using VehiclePhysics;

namespace Perrinn424.PerformanceBenchmarkSystem
{
    public class PerformanceBenchmarkController : VehicleBehaviour
    {
        public IPerformanceBenchmark Porsche919 { get => porsche919; }

        [SerializeField]
        private PerformanceBenchmarkData porsche919Data;

        private PerformanceBenchmark porsche919;

        public override void OnEnableVehicle()
        {
            porsche919 = new PerformanceBenchmark(porsche919Data.samples);
        }

        public override void FixedUpdateVehicle()
        {
            Telemetry.DataRow latestTelemetry = vehicle.telemetry.latest;
            UpdateBenchmark((float)latestTelemetry.time, (float)latestTelemetry.distance);
        }

        private void UpdateBenchmark(float currentTime, float currentDistance)
        {
            porsche919.Update(currentTime, currentDistance);
        }
    }
}
