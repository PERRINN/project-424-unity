using Perrinn424.AutopilotSystem;
using Perrinn424.Utilities;
using System;
using UnityEngine;
using UnityEngine.UI;
using VehiclePhysics;
using VehiclePhysics.Timing;
using VehiclePhysics.UI;

namespace Perrinn424.UI
{
    public class Dashboard : MonoBehaviour
    {
        [SerializeField]
        private Text drsText;
        [SerializeField]
        private Text autopilotText;
        [SerializeField]
        private Text gearText;
        [SerializeField]
        private Text speedMPSText;
        [SerializeField]
        private Text speedKPHText;
        [SerializeField]
        private Text referenceDiffText;
        [SerializeField]
        private Text totalPowerText;
        [SerializeField]
        private Text capacityText;
        [SerializeField]
        private Text socText;

        [SerializeField]
        private VehicleBase vehicle;

        [SerializeField]
        LapTimer m_lapTimer = null;


        private BaseAutopilot autopilot;

        [SerializeField]
        private RefreshHelper refreshHelper;

        public void OnEnable()
        {
            autopilot = vehicle.GetComponentInChildren<BaseAutopilot>();
        }

        public void Update()
        {
            if (refreshHelper.Update(Time.deltaTime))
            {
                WriteDRS();
                WriteAutopilot();
                WriteGear();
                WriteSpeedInfo();
                WriteDiffs();
                WriteBatteryInfo();
            }
        }

        private void WriteAutopilot()
        {
            string autopilotFlag = autopilot.IsOn ? "ON" : "OFF";
            autopilotText.text = $"AUTOPILOT {autopilotFlag}";
        }

        private void WriteDRS()
        {
            float drsPosition = vehicle.data.Get(Channel.Custom, Perrinn424Data.DrsPosition) / 1000.0f;
            string drsFlag = drsPosition > 0f ? "ON" : "OFF";
            drsText.text = $"DRS {drsFlag}";
        }

        private void WriteGear()
        {
            int[] vehicleData = vehicle.data.Get(Channel.Vehicle);
            int gearId = vehicleData[VehicleData.GearboxGear];
            bool switchingGear = vehicleData[VehicleData.GearboxShifting] != 0;

            if (gearId == 0)
            {
                gearText.text = switchingGear ? " " : "N";
            }
            else
            {
                if (gearId > 0)
                {
                    gearText.text = "D";
                }
                else
                {
                    if (gearId == -1)
                    {
                        gearText.text = "R";
                    }
                    else
                    {
                        gearText.text = "R" + (-gearId).ToString();
                    }
                }
            }
        }

        private void WriteSpeedInfo()
        {
            float speed = vehicle.data.Get(Channel.Vehicle, VehicleData.Speed) / 1000.0f;
            speedMPSText.text = $"{speed:F0} MPS";
            speedKPHText.text = $"{speed*3.6f:F0} KPH";
        }

        private void WriteDiffs()
        {
            float compare = m_lapTimer.currentLapTime - autopilot.CalculatePlayingTime();
            string diff = compare.ToString("+0.00;-0.00");
            referenceDiffText.text = $"{diff} SEC";
        }

        private void WriteBatteryInfo()
        {
            int[] custom = vehicle.data.Get(Channel.Custom);
            float frontPower = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.ElectricalPower] / 1000.0f;
            float rearPower = custom[Perrinn424Data.RearMguBase + Perrinn424Data.ElectricalPower] / 1000.0f;

            float total = frontPower + rearPower;
            string totalStr = total.ToString("+0;-0");
            totalPowerText.text = $"{totalStr} KW";

            float batSOC = BatteryModel.batterySOC;
            socText.text = $"{batSOC:0.0} SOC";

            float batCapacity = BatteryModel.batteryCapacity;
            capacityText.text = $"{55 - batCapacity:0.00} KWH";
        }
    }
}
