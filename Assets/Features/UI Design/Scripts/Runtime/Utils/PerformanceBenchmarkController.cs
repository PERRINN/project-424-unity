using VehiclePhysics;

namespace Perrinn424
{
    public class PerformanceBenchmarkController : VehicleBehaviour
    {
        private PerformanceBenchmark porsche919;
        private PerformanceBenchmark idr;

        public float Porsche919Diff { get; private set; } //[s]
        public float IDRDiff { get; private set; } //[s]

        public float Porsche919Speed { get; private set; } //[m/s]
        public float IDRSpeed { get; private set; } //[m/s]

        public override void OnEnableVehicle()
        {
            porsche919 = PerformanceBenchmarkHelper.CreatePorsche919();
            idr = PerformanceBenchmarkHelper.CreateIDR();
        }

        private void Update()
        {
            Telemetry.DataRow latestTelemetry = vehicle.telemetry.latest;
            Update((float)latestTelemetry.time, (float)latestTelemetry.distance);
        }

        private void Update(float currentTime, float currentDistance)
        {
            Porsche919Diff = porsche919.LapDiff(currentTime, currentDistance);
            IDRDiff = idr.LapDiff(currentTime, currentDistance);

            Porsche919Speed = porsche919.Speed;
            IDRSpeed = idr.Speed;
        }
    }
}
