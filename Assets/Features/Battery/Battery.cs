using UnityEngine;
using VehiclePhysics;
using VehiclePhysics.Timing;

namespace Perrinn424
{
    public class Battery : VehicleBehaviour
    {
        public LapTimer lapTimer;
        public BatteryPowerModel powerModel = new BatteryPowerModel();
        public BatteryTemperatureModel temperatureModel = new BatteryTemperatureModel();

        public float Power => powerModel.Power;
        public float StateOfCharge => powerModel.StateOfCharge;
        public float NetEnergy => powerModel.NetEnergy; //Kwh
        public float PowerRMS => powerModel.PowerRMS; //kW
        public float Capacity => powerModel.settings.capacity;

        public float TotalHeat => temperatureModel.TotalHeat; //W
        public float AirMassFlow => temperatureModel.AirMassFlow; //kg/s
        public float HeatDissipation => temperatureModel.HeatDissipation; //W/degC
        public float HeatDissipated => temperatureModel.HeatDissipated; //J
        public float TemperatureModule => temperatureModel.TemperatureModule; //degC
        public float HeatInternal => temperatureModel.HeatInternal; //J


        private void Start()
        {
            powerModel.InitModel();
        }

        public override void OnEnableVehicle()
        {
            if (lapTimer != null) lapTimer.onBeginLap += LapBeginEventHandler;
        }

        private void LapBeginEventHandler()
        {
            powerModel.Reset();
            temperatureModel.Reset(Time.deltaTime, vehicle.speed, powerModel.Power);
        }

        public override void OnDisableVehicle()
        {
            if (lapTimer != null) lapTimer.onBeginLap -= LapBeginEventHandler;
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
