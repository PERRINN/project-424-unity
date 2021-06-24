using VehiclePhysics;

namespace Perrinn424
{
    public class PerformanceBenchmarkController : VehicleBehaviour
    {
        private PerformanceBenchmark porsche919;
        private PerformanceBenchmark idr;
        public float Porsche919Diff => porsche919.TimeDiff; //[s]
        public float IDRDiff => idr.TimeDiff; //[s]

        public float Porsche919Speed => porsche919.Speed;//[m/s]
        public float IDRSpeed => idr.Speed;//[m/s]

        public float Porsche919TraveledDistance => porsche919.TraveledDistance;//[m]
        public float IDRTraveledDistance => idr.TraveledDistance;//[m]

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
            porsche919.Update(currentTime, currentDistance);
            idr.Update(currentTime, currentDistance);
        }
    }
}
