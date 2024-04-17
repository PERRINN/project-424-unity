using System;
using UnityEngine;

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
        public float Capacity { get; private set; } //kWh
        public float StateOfCharge { get; private set; }  //SOC
        public float DepthOfDischarge { get; private set; } //DOD
        public float NetEnergy { get; private set; }//kWh
        public float PowerRMS { get; private set; }//kW
        public int PowerRMSCount { get; private set; }


        public void InitModel()
        {
            Power = 0f;
            Capacity = settings.capacity;
            StateOfCharge = Capacity;
            DepthOfDischarge = 0;
            NetEnergy = settings.capacity - Capacity;
            PowerRMS = 0;
            PowerRMSCount = 0;
        }

        public void Reset()
        {
            InitModel();
        }

        public void UpdateModel(float frontPower, float rearPower)
        {
            Power = frontPower + rearPower;
            //60*60*500
            // Capacity < 0 => Capacity = 0
            Capacity = Mathf.Max(Capacity - Power / 1_800_000f, 0);
            StateOfCharge = Mathf.InverseLerp(0f, settings.capacity, Capacity);
            DepthOfDischarge = 1f - StateOfCharge;
            NetEnergy = settings.capacity - Capacity;
            PowerRMS = Mathf.Sqrt(((PowerRMS*PowerRMS)*PowerRMSCount+Power*Power)/(PowerRMSCount+1));
            PowerRMSCount += 1;
        }
    }
}
