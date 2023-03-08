using UnityEngine;
using VehiclePhysics;
using VehiclePhysics.Timing;

namespace Perrinn424
{
    public class Battery : VehicleBehaviour
    {
        [SerializeField]
        private LapTimer lapTimer;

        [SerializeField]
        private BatteryTemperatureModel temperatureModel;
        [SerializeField]
        private BatteryPowerModel powerModel;


        public float Power => powerModel.Power;
        public float StateOfCharge => powerModel.StateOfCharge;
        public float CapacityUsage => powerModel.CapacityUsage;

        private void Start()
        {
            powerModel.InitModel();
        }

        public override void OnEnableVehicle()
        {
            lapTimer.onBeginLap += LapBeginEventHandler;
        }

        private void LapBeginEventHandler()
        {
            temperatureModel.Reset(Time.deltaTime, vehicle.speed, powerModel.Power);
        }

        public override void OnDisableVehicle()
        {
            lapTimer.onBeginLap -= LapBeginEventHandler;
        }


        public override void FixedUpdateVehicle()
        {
            int[] custom = vehicle.data.Get(Channel.Custom);

            float frontPower = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.ElectricalPower] / 1000.0f;
            float rearPower = custom[Perrinn424Data.RearMguBase + Perrinn424Data.ElectricalPower] / 1000.0f;

            powerModel.UpdateModel(frontPower, rearPower);
            temperatureModel.UpdateModel(Time.deltaTime, vehicle.speed, powerModel.Power);
        }
    }
}