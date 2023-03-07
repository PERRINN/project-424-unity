using System;

namespace Perrinn424
{
    [Serializable]
    public class BatteryPowerModel
    {
        [Serializable]
        public class Settings
        {
            public float capacity = 55f;
        }

        public Settings settings = new Settings();

        public float Power { get; private set; } 
        public float Capacity { get; private set; } 
        public float StateOfCharge { get; private set; }  //SOC
        public float DepthOfDischarge { get; private set; } //DOD


        public void InitModel()
        {
            Capacity = settings.capacity;
            StateOfCharge = Capacity;
            DepthOfDischarge = 0;
        }

        public void UpdateModel(float frontPower, float rearPower)
        {
            Power = frontPower + rearPower;

            BatteryCharge();

            StateOfCharge = (Capacity / 55) * 100;
            DepthOfDischarge = 100 - StateOfCharge;
        }

        void BatteryCharge()
        {
            if (Power > 0)
            {
                Capacity -= ((Power / 60) / 60) / 500;
            }
            else if (Power < 0)
            {
                Capacity -= ((Power / 60) / 60) / 500;
            }

            if (Capacity < 0)
            {
                Capacity = 0;
            }
        }
    } 
}
