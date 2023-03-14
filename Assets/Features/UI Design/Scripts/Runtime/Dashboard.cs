using UnityEngine;
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
            totalPowerText.text = $"{battery.Power:+0;-0} KW";
            socText.text = string.Format("{0:0.0} SOC", battery.StateOfCharge * 100f);
            capacityText.text = $"{battery.NetEnergy:0.00} KWH";
        }
    }
}
