using UnityEngine;
using UnityEngine.Assertions;
using VehiclePhysics;
using VehiclePhysics.Timing;

namespace Perrinn424
{
    public class Battery : VehicleBehaviour
    {
        public static float batteryCapacity;
        public static float batterySOC;
        public static float frontPower;
        public static float rearPower;
        public static float powerTotal;

        float batteryDOD;

        [SerializeField]
        private LapTimer lapTimer;

        [SerializeField]
        private BatteryTemperatureModel temperatureModel;
        [SerializeField]
        private BatteryPowerModel powerModel;


        public float Power => powerModel.Power;
        public float StateOfCharge => powerModel.StateOfCharge;

        //Legacy calculation that was made on the UI elements.
        public float CapacityUsage => powerModel.CapacityUsage;

        private void Start()
        {
            batteryCapacity = 55;
            batterySOC = batteryCapacity;
            batteryDOD = 0;

            powerModel.InitModel();
        }

        public override void OnEnableVehicle()
        {
            lapTimer.onBeginLap += LapBeginEventHandler;
        }

        private void LapBeginEventHandler()
        {
            temperatureModel.Reset(Time.deltaTime, vehicle.speed, powerTotal);
        }

        public override void OnDisableVehicle()
        {
            lapTimer.onBeginLap -= LapBeginEventHandler;
        }


        public override void FixedUpdateVehicle()
        {
            int[] custom = vehicle.data.Get(Channel.Custom);

            frontPower = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.ElectricalPower] / 1000.0f;
            rearPower = custom[Perrinn424Data.RearMguBase + Perrinn424Data.ElectricalPower] / 1000.0f;

            powerTotal = frontPower + rearPower;

            BatteryCharge();

            batterySOC = (batteryCapacity / 55) * 100;
            batteryDOD = 100 - batterySOC;

            powerModel.UpdateModel(frontPower, rearPower);
            temperatureModel.UpdateModel(Time.deltaTime, vehicle.speed, powerTotal);

            float error = Mathf.Abs(powerModel.Power - powerTotal);
            error += Mathf.Abs(powerModel.Capacity - batteryCapacity);
            error += Mathf.Abs(powerModel.StateOfCharge*100f - batterySOC);
            error += Mathf.Abs(powerModel.DepthOfDischarge*100f - batteryDOD);

            Assert.IsTrue(error < 1e-3f);
        }

        public override void UpdateVehicle()
        {
            SteeringScreen.batSOC = batterySOC;
            SteeringScreen.batCapacity = batteryCapacity;
        }

        void BatteryCharge()
        {
            if (powerTotal > 0)
            {
                batteryCapacity -= ((powerTotal / 60) / 60) /500;
            }
            else if (powerTotal < 0)
            {
                batteryCapacity -= ((powerTotal / 60) / 60) / 500;
            }

            if (batteryCapacity < 0)
            {
                batteryCapacity = 0;
            }
        }
    }
}