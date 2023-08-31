
﻿using Perrinn424.AutopilotSystem;
using UnityEngine;
using UnityEngine.UI;
using VehiclePhysics;
using VehiclePhysics.Timing;


namespace Perrinn424
{
    public class DashboardDisplay : VehicleBehaviour
    {
        public float windowSeconds = 4.0f;
        public float setMinSpeed = 10.0f;
        public float screenFPS = 5f;

        [Header("Display Channel")]
        public Text speedMps;
        public Text speedKph;
        public Text gear;
        //public Text frontMGURpm;
        //public Text rearMGURpm;
        public Text totalElecPower;
        public Image drsImage;
        public Image autopilotImage;
        public Text minIndicator;
        //public Text elecTorqueBal;
        //public Text mechTorqueBal;
        public Text timeDifference;
        public Text batterySOC;
        public Text batteryCapacity;

        float maxSpeed = float.MinValue;
        float minSpeed = float.MaxValue;
        float minSpdTime;
        bool minSpdWindow = false;
        bool setMinSpdTrigger = false;
        float drsPosition;
        //bool autopilotState = false;


        float elapsed;
        LapTimer m_lapTimer = null;
        private BaseAutopilot autopilot;
        private Battery battery;

        public override void OnEnableVehicle ()
        {
            m_lapTimer = FindObjectOfType<LapTimer>();
            autopilot = vehicle.GetComponentInChildren<BaseAutopilot>();
            battery = vehicle.GetComponentInChildren<Battery>();
        }

        public override void UpdateVehicle ()
        {
            elapsed += Time.deltaTime;
            if (elapsed > 1 / screenFPS)
            {
                elapsed = 0;

                int[] vehicleData = vehicle.data.Get(Channel.Vehicle);
                int[] custom = vehicle.data.Get(Channel.Custom);
                float speed = vehicleData[VehicleData.Speed] / 1000.0f;

                if (speed > setMinSpeed) { setMinSpdTrigger = true; }
                else
                {
                    setMinSpdTrigger = false;
                    minSpdWindow = false;
                    maxSpeed = float.MinValue;
                    minSpeed = float.MaxValue;
                }

                // Speed in m/s & minimum speed window
                if (speedMps != null)
                {
                    if (minSpeed > speed && setMinSpdTrigger)
                    {
                        minSpeed = speed;
                        StartTimer();
                        minSpdWindow = true;
                        minIndicator.enabled = false;
                    }

                    if (maxSpeed < speed) { maxSpeed = speed; }

                    if (minSpdWindow)
                    {
                        float systemTime = Time.time;
                        if (systemTime - minSpdTime > windowSeconds)
                        {
                            minSpdWindow = false;
                            maxSpeed = float.MinValue;
                        }
                        else { speedMps.text = minSpeed.ToString("0"); }

                        if (systemTime - minSpdTime > 0.02f) { minIndicator.enabled = true; }
                    }
                    else
                    {
                        minSpeed = maxSpeed - 1;
                        speedMps.text = speed.ToString("0");
                        minIndicator.enabled = false;
                    }
                }

                // Speed in kph
                if (speedKph != null) // km/hour
                    speedKph.text = (speed * 3.6f).ToString("0");

                // Gear
                if (gear != null)
                {
                    int gearId = vehicleData[VehicleData.GearboxGear];
                    bool switchingGear = vehicleData[VehicleData.GearboxShifting] != 0;

                    if (gearId == 0)
                        gear.text = switchingGear ? " " : "N";
                    else
                    if (gearId > 0)
                        gear.text = "D"; //gearId.ToString();
                    else
                    {
                        if (gearId == -1)
                            gear.text = "R";
                        else
                            gear.text = "R" + (-gearId).ToString();
                    }
                }

                // DRS signal
                drsPosition = vehicle.data.Get(Channel.Custom, Perrinn424Data.DrsPosition) / 1000.0f;

                if (drsImage != null)
                {
                    drsImage.color = new Color32(255, 255, 255, 0);
                    // drsImage.color = new Color32(255, 255, 255, (byte) (drsPosition * 255));
                    if (drsPosition > 0f)
                    {
                        drsImage.color = new Color32(255, 255, 255, 255);
                    }
                }

                // AUTOPILOT signal
                if (autopilotImage != null)
                {
                    byte alpha = (autopilot != null && autopilot.IsOn) ? (byte)255 : (byte)0;
                    autopilotImage.color = new Color32(255, 255, 255, alpha);
                }


                totalElecPower.text = battery.Power.ToString("+0;-0");
                batterySOC.text = string.Format("{0:0.00}", battery.StateOfCharge * 100f);
                batteryCapacity.text = battery.NetEnergy.ToString("0.00");

                // Time Difference with the Best Lap
                if (timeDifference != null && m_lapTimer != null && autopilot != null)
                {
                    timeDifference.text = autopilot.DeltaTime.ToString("+0.00;-0.00");
                }

            }
        }

        void StartTimer()
        {
            minSpdTime = Time.time;
        }

        string GetBalanceStr(float front, float rear)
        {
            // This also covers front == rear == 0
            if (front == rear) return "50.0";
            if (front != 0.0f && rear != 0.0f && Mathf.Sign(front) != Mathf.Sign(rear)) return "-  ";

            return (front / (front + rear) * 100).ToString("0.0");
        }
    }

}