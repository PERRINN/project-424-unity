using VehiclePhysics;

namespace Perrinn424
{
    public class PerformanceBenchmarkController : VehicleBehaviour
    {
        private PerformanceBenchmark porsche919;
        private PerformanceBenchmark idr;

        public IPerformanceBenchmarkData Porsche919 { get => porsche919; }
        public IPerformanceBenchmarkData IDR { get => idr; } 

        public override void OnEnableVehicle()
        {
            porsche919 = PerformanceBenchmarkHelper.CreatePorsche919();
            idr = PerformanceBenchmarkHelper.CreateIDR();
        }

        public override void FixedUpdateVehicle()
        {
            Telemetry.DataRow latestTelemetry = vehicle.telemetry.latest;
            UpdateBenchmark((float)latestTelemetry.time, (float)latestTelemetry.distance);
        }

        private void UpdateBenchmark(float currentTime, float currentDistance)
        {
            porsche919.Update(currentTime, currentDistance);
            idr.Update(currentTime, currentDistance);
        }
    }
}
