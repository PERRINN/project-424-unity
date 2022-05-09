using Perrinn424.AutopilotSystem;
using UnityEngine;
using UnityEngine.UI;
using VehiclePhysics.Timing;


namespace VehiclePhysics.UI
{
    public class SteeringScreen : VehicleBehaviour
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

        // TODO: Replace these static properties with proper component querying (example: LapTimer)
        //public static float bestTime { get; set; } //Autopilot.cs
        //public static bool autopilotState { get; set; } //Autopilot.cs
        public static float batSOC { get; set; } //BatteryModel.cs
        public static float batCapacity { get; set; } //BatteryModel.cs

        float elapsed;
        LapTimer m_lapTimer = null;
        private BaseAutopilot autopilot;

        public override void OnEnableVehicle ()
        {
            m_lapTimer = FindObjectOfType<LapTimer>();
            autopilot = vehicle.GetComponentInChildren<BaseAutopilot>();
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
                        minIndicator.gameObject.SetActive(false);
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

                        if (systemTime - minSpdTime > 0.02f) { minIndicator.gameObject.SetActive(true); }
                    }
                    else
                    {
                        minSpeed = maxSpeed - 1;
                        speedMps.text = speed.ToString("0");
                        minIndicator.gameObject.SetActive(false);
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
                    //if (autopilotState)
                    //{
                    //    autopilotImage.color = new Color32(255, 255, 255, 255);
                    //}
                }


                //// Front MGU rpm
                //if (frontMGURpm != null)
                //{
                //    float frontRpm = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.Rpm] / 1000.0f;

                //    frontMGURpm.text = $"{frontRpm,5:0.}";
                //}

                //// Rear MGU rpm
                //if (rearMGURpm != null)
                //{
                //    float rearRpm = custom[Perrinn424Data.RearMguBase + Perrinn424Data.Rpm] / 1000.0f;

                //    rearMGURpm.text = $"{rearRpm,5:0.}";
                //}

                // Total Electrical Power
                if (totalElecPower != null)
                {
                    float frontPower = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.ElectricalPower] / 1000.0f;
                    float rearPower = custom[Perrinn424Data.RearMguBase + Perrinn424Data.ElectricalPower] / 1000.0f;

                    float total = frontPower + rearPower;
                    totalElecPower.text = total >= 0 ? total.ToString("+" + "0") : total.ToString("0");
                }

                //// Electrical Torque Balance
                //if (elecTorqueBal != null)
                //{
                //    float frontElectrical = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.ElectricalTorque] / 1000.0f;
                //    float rearElectrical = custom[Perrinn424Data.RearMguBase + Perrinn424Data.ElectricalTorque] / 1000.0f;

                //    elecTorqueBal.text = $"{GetBalanceStr(frontElectrical, rearElectrical),5}";
                //}

                //// Mechanical Torque Balance
                //if (mechTorqueBal != null)
                //{
                //    float frontWheels = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.WheelsTorque] / 1000.0f;
                //    float rearWheels = custom[Perrinn424Data.RearMguBase + Perrinn424Data.WheelsTorque] / 1000.0f;

                //    mechTorqueBal.text = $"{ GetBalanceStr(frontWheels, rearWheels),5}";
                //}

                // Time Difference with the Best Lap
                if (timeDifference != null && m_lapTimer != null && autopilot != null)
                {
                    float compare = m_lapTimer.currentLapTime - autopilot.CalculatePlayingTime();

                    timeDifference.text = Mathf.Sign(compare) == -1 ? Mathf.Abs(compare).ToString("-0.00") : compare.ToString("+0.00");
                }

                // Battery SOC
                if (batterySOC != null)
                {
                    batterySOC.text = batSOC.ToString("0.00");
                }

                // Battery Capacity
                if (batteryCapacity != null)
                {
                    batteryCapacity.text = (55 - batCapacity).ToString("0.00");
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