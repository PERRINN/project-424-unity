using UnityEngine;
using VehiclePhysics;

namespace Perrinn424.PerformanceBenchmarkSystem
{
    public class PerformanceBenchmarkController : VehicleBehaviour
    {
        [SerializeField]
        private PerformanceBenchmarkData porsche919Data;
        [SerializeField]
        private PerformanceBenchmarkData idrData;

        private PerformanceBenchmark porsche919;
        private PerformanceBenchmark idr;

        public IPerformanceBenchmark Porsche919 { get => newPorsche919; }
        public IPerformanceBenchmark IDR { get => idr; }


        [SerializeField]
        private NewPerformanceBenchmarkData newPorsche919Data;

        private NewPerformanceBenchmark newPorsche919;

        public override void OnEnableVehicle()
        {
            porsche919 = new PerformanceBenchmark(porsche919Data.samples, porsche919Data.frequency);
            idr = new PerformanceBenchmark(idrData.samples, idrData.frequency);

            newPorsche919 = new NewPerformanceBenchmark(newPorsche919Data.samples);
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

            newPorsche919.Update(currentTime, currentDistance);
        }
    }
}
