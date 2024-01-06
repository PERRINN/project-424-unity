
﻿using Perrinn424.AutopilotSystem;
using UnityEngine;
using UnityEngine.UI;
using VehiclePhysics;
using VehiclePhysics.Timing;


namespace Perrinn424
{
    public class DashboardDisplay : VehicleBehaviour
    {
        public float screenFPS = 5f;

        [Header("Min Speed Display")]
        public bool enableMinSpeedDisplay = false;
        public float windowSeconds = 4.0f;
        public float setMinSpeed = 10.0f;

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


        float m_maxSpeed = float.MinValue;
        float m_minSpeed = float.MaxValue;
        float m_minSpdTime;
        bool m_minSpdWindow = false;
        bool m_setMinSpdTrigger = false;

        float m_elapsed;
        bool m_lapStarted;
        LapTimer m_lapTimer = null;
        BaseAutopilot m_autopilot;
        Battery m_battery;

        public override void OnEnableVehicle ()
        {
            m_lapTimer = FindObjectOfType<LapTimer>();
            m_autopilot = vehicle.GetComponentInChildren<BaseAutopilot>();
            m_battery = vehicle.GetComponentInChildren<Battery>();

            if (m_lapTimer != null)
            {
                m_lapTimer.onBeginLap += BeginLap;
                m_lapStarted = false;
            }
            else
            {
                m_lapStarted = true;
            }
        }

        public override void OnDisableVehicle ()
        {
            if (m_lapTimer != null)
                m_lapTimer.onBeginLap -= BeginLap;
        }

        public override void UpdateVehicle ()
        {
            m_elapsed += Time.deltaTime;
            if (m_elapsed > 1 / screenFPS)
            {
                m_elapsed = 0;

                int[] vehicleData = vehicle.data.Get(Channel.Vehicle);
                int[] custom = vehicle.data.Get(Channel.Custom);
                float speed = vehicleData[VehicleData.Speed] / 1000.0f;

                if (speed > setMinSpeed)
                {
                    m_setMinSpdTrigger = true;
                }
                else
                {
                    m_setMinSpdTrigger = false;
                    m_minSpdWindow = false;
                    m_maxSpeed = float.MinValue;
                    m_minSpeed = float.MaxValue;
                }

                // Speed in m/s & minimum speed window
                if (IsAvailable(speedMps))
                {
                    if (m_minSpeed > speed && m_setMinSpdTrigger)
                    {
                        m_minSpeed = speed;
                        StartTimer();
                        m_minSpdWindow = true;

                        if (minIndicator != null)
                            minIndicator.enabled = false;
                    }

                    if (m_maxSpeed < speed)
                    {
                        m_maxSpeed = speed;
                    }

                    if (m_minSpdWindow && enableMinSpeedDisplay)
                    {
                        float systemTime = Time.time;
                        if (systemTime - m_minSpdTime > windowSeconds)
                        {
                            m_minSpdWindow = false;
                            m_maxSpeed = float.MinValue;
                        }
                        else
                        {
                            speedMps.text = m_minSpeed.ToString("0");
                        }

                        if (systemTime - m_minSpdTime > 0.02f)
                        {
                            if (minIndicator != null)
                                minIndicator.enabled = true;
                        }
                    }
                    else
                    {
                        m_minSpeed = m_maxSpeed - 1;
                        speedMps.text = speed.ToString("0");
                        if (minIndicator != null)
                            minIndicator.enabled = false;
                    }
                }

                // Speed in kph
                if (IsAvailable(speedKph)) // km/hour
                    speedKph.text = (speed * 3.6f).ToString("0");

                // Gear
                if (IsAvailable(gear))
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

                if (IsAvailable(drsImage))
                {
                    // drsImage.color = new Color32(255, 255, 255, (byte) (m_drsPosition * 255));
                    float drsPosition = vehicle.data.Get(Channel.Custom, Perrinn424Data.DrsPosition) / 1000.0f;
                    if (drsPosition > 0f)
                        drsImage.color = new Color32(255, 255, 255, 255);
                    else
                        drsImage.color = new Color32(255, 255, 255, 0);
                }

                // AUTOPILOT signal
                if (IsAvailable(autopilotImage) && m_autopilot != null)
                {
                    byte alpha = (m_autopilot != null && m_autopilot.IsOn) ? (byte)255 : (byte)0;
                    autopilotImage.color = new Color32(255, 255, 255, alpha);
                }

                // Power and battery data
                if (m_battery != null)
                {
                    if (IsAvailable(totalElecPower))
                        totalElecPower.text = m_battery.Power.ToString("+0;-0");

                    if (IsAvailable(batterySOC))
                        batterySOC.text = string.Format("{0:0.00}", m_battery.StateOfCharge * 100f);

                    if (IsAvailable(batteryCapacity))
                        batteryCapacity.text = m_battery.NetEnergy.ToString("0.00");
                }

                // Time Difference with the Best Lap
                if (IsAvailable(timeDifference))
                {
                    if (m_lapStarted)
                    {
                        if (m_autopilot != null)
                        {
                            float deltaTime = Mathf.Clamp(m_autopilot.DeltaTime, -42.4f, 42.4f);
                            timeDifference.text = deltaTime.ToString("+0.00;-0.00");
                        }
                        else
                        {
                            timeDifference.text = "--.--";
                        }
                    }
                    else
                    {
                        timeDifference.text = " 0.00";
                    }
                }
            }
        }

        void StartTimer()
        {
            m_minSpdTime = Time.time;
        }

        void BeginLap ()
        {
            m_lapStarted = true;
        }

        string GetBalanceStr(float front, float rear)
        {
            // This also covers front == rear == 0
            if (front == rear) return "50.0";
            if (front != 0.0f && rear != 0.0f && Mathf.Sign(front) != Mathf.Sign(rear)) return "-  ";

            return (front / (front + rear) * 100).ToString("0.0");
        }

        static bool IsAvailable(Behaviour element)
        {
            return element != null && element.isActiveAndEnabled;
        }
    }

}