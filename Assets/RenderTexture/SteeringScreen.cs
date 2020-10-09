﻿using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;


namespace VehiclePhysics.UI
{
    public class SteeringScreen : MonoBehaviour
    {
        VehicleBase target;
        public float windowSeconds = 4.0f;
        public float setMinSpeed = 10.0f;
        [Header("Display Channel")]
        public Text speedMps;
        public Text speedKph;
        public Text gear;
        public Text frontMGURpm;
        public Text rearMGURpm;
        public Text totalElecPower;
        public Image drsImage;
        public Text minIndicator;
        public Text elecTorqueBal;
        public Text mechTorqueBal;

        float maxSpeed = float.MinValue;
        float minSpeed = float.MaxValue;
        float minSpdTime;
        bool minSpdWindow = false;
        bool setMinSpdTrigger = false;

        void OnEnable()
        {
            target = GetComponentInParent<VehicleBase>();
        }

        void Update()
        {
            if (target == null) return;

            int[] vehicleData = target.data.Get(Channel.Vehicle);
            int[] custom = target.data.Get(Channel.Custom);
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
            if (drsImage != null)
            {
                drsImage.color = new Color32(255, 255, 255, 0);

                if (speed * 2.237f > 100)
                {
                    drsImage.color = new Color32(255, 255, 255, 255);
                }
            }


            // Front MGU rpm
            if (frontMGURpm != null)
            {
                float frontRpm = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.Rpm] / 1000.0f;

                frontMGURpm.text = $"{frontRpm,5:0.}";
            }

            // Rear MGU rpm
            if (rearMGURpm != null)
            {
                float rearRpm = custom[Perrinn424Data.RearMguBase + Perrinn424Data.Rpm] / 1000.0f;

                rearMGURpm.text = $"{rearRpm,5:0.}";
            }

            // Total Electrical Power
            if (totalElecPower != null)
            {
                float frontPower = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.ElectricalPower] / 1000.0f;
                float rearPower = custom[Perrinn424Data.RearMguBase + Perrinn424Data.ElectricalPower] / 1000.0f;

                totalElecPower.text = $"{frontPower + rearPower,6:0.}";
            }

            // Electrical Torque Balance
            if (elecTorqueBal != null)
            {
                float frontElectrical = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.ElectricalTorque] / 1000.0f;
                float rearElectrical = custom[Perrinn424Data.RearMguBase + Perrinn424Data.ElectricalTorque] / 1000.0f;

                elecTorqueBal.text = $"{GetBalanceStr(frontElectrical, rearElectrical),5}";
            }

            // Mechanical Torque Balance
            if (mechTorqueBal != null)
            {
                float frontWheels = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.WheelsTorque] / 1000.0f;
                float rearWheels = custom[Perrinn424Data.RearMguBase + Perrinn424Data.WheelsTorque] / 1000.0f;

                mechTorqueBal.text = $"{ GetBalanceStr(frontWheels, rearWheels),5}";
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