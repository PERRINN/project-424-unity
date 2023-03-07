using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using VehiclePhysics;

namespace Perrinn424.UI
{
    public class Dashboard : BaseDashboard
    {
        [SerializeField]
        private Text drsText;
        [SerializeField]
        private Text autopilotText;
        [SerializeField]
        private Text speedMPSText;
        [SerializeField]
        private Text speedKPHText;
        [SerializeField]
        private Text totalPowerText;
        [SerializeField]
        private Text capacityText;
        [SerializeField]
        private Text socText;


        private Battery battery;

        public new void OnEnable()
        {
            battery = vehicle.GetComponentInChildren<Battery>();
            base.OnEnable();
        }

        protected override void UpdateValues()
        {
            WriteDRS();
            WriteAutopilot();
            WriteGear();
            WriteSpeedInfo();
            WriteDiffs();
            WriteBatteryInfo();
        }

        private void WriteAutopilot()
        {
            string autopilotFlag = autopilot.IsOn ? "ON" : "OFF";
            autopilotText.text = $"AUTOPILOT {autopilotFlag}";
        }

        private void WriteDRS()
        {
            string drsFlag = IsDrsOn() ? "ON" : "OFF";
            drsText.text = $"DRS {drsFlag}";
        }

        private void WriteSpeedInfo()
        {
            float speed = vehicle.data.Get(Channel.Vehicle, VehicleData.Speed) / 1000.0f;
            speedMPSText.text = $"{speed:F0} MPS";
            speedKPHText.text = $"{speed*3.6f:F0} KPH";
        }

        protected override void WriteDiffs()
        {
            referenceDiffText.text = autopilot.DeltaTime.ToString("+0.00 SEC;-0.00 SEC"); ;
        }

        private void WriteBatteryInfo()
        {
            int[] custom = vehicle.data.Get(Channel.Custom);
            float frontPower = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.ElectricalPower] / 1000.0f;
            float rearPower = custom[Perrinn424Data.RearMguBase + Perrinn424Data.ElectricalPower] / 1000.0f;

            float total = frontPower + rearPower;
            Assert.IsTrue(total == battery.Power);
            string totalStr = total.ToString("+0;-0");
            totalPowerText.text = $"{totalStr} KW";

            float batSOC = Battery.batterySOC;
            Assert.IsTrue(Mathf.Approximately(batSOC, battery.StateOfCharge*100f));
            socText.text = $"{batSOC:0.0} SOC";

            float batCapacity = Battery.batteryCapacity;
            batCapacity = 55 - batCapacity;
            Assert.IsTrue(Mathf.Approximately(batCapacity, battery.WeirdInverseCalculation));

            capacityText.text = $"{batCapacity:0.00} KWH";
        }
    }
}
