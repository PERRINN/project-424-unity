using UnityEngine;
using VehiclePhysics;

namespace Perrinn424.PerformanceBenchmarkSystem
{
    public class PerformanceBenchmarkController : VehicleBehaviour
    {
        public IPerformanceBenchmark Porsche919 { get => newPorsche919; }

        [SerializeField]
        private NewPerformanceBenchmarkData newPorsche919Data;

        private NewPerformanceBenchmark newPorsche919;

        public override void OnEnableVehicle()
        {
            newPorsche919 = new NewPerformanceBenchmark(newPorsche919Data.samples);
        }

        public override void FixedUpdateVehicle()
        {
            Telemetry.DataRow latestTelemetry = vehicle.telemetry.latest;
            UpdateBenchmark((float)latestTelemetry.time, (float)latestTelemetry.distance);
        }

        private void UpdateBenchmark(float currentTime, float currentDistance)
        {
            newPorsche919.Update(currentTime, currentDistance);
        }
    }
}
