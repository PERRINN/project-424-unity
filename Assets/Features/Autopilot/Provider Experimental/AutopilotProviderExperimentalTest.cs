using VehiclePhysics;

namespace Perrinn424.AutopilotSystem
{
    public class AutopilotProviderExperimentalTest : VehicleBehaviour
    {
        public RecordedLap recordedLap;
        public AutopilotProviderExperimental provider;

        public Sample current;
        public override void FixedUpdateVehicle()
        {
            current = provider[vehicle.transform.position];
        }

        public override void OnEnableVehicle()
        {
            provider = new AutopilotProviderExperimental(recordedLap);
        }
    }
}
