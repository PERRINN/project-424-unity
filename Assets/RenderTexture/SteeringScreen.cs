using System;
using UnityEngine;
using UnityEngine.UI;


namespace VehiclePhysics.UI
{

    public class SteeringScreen : MonoBehaviour
    {
        public VehicleBase target;
        [Header("UI")]
        public Text speedMps;
        public Text speedKph;
        public Text gear;
        public Text frontWheelRpm;
        public Text backWheelRpm;
        public Text enginePower;
        public Image drsImage;

        void Update()
        {
            if (target == null) return;

            int[] vehicleData = target.data.Get(Channel.Vehicle);
            int[] inputData = target.data.Get(Channel.Input);

            // Speed

            float speed = vehicleData[VehicleData.Speed] / 1000.0f;

            if (speedMps != null) // m/s
                speedMps.text = speed.ToString("0");

            if (speedKph != null) // km/h
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

            // Vehicle's input
            float brake = inputData[InputData.Brake] / 10000.0f;
            float handBrake = inputData[InputData.Handbrake] / 10000.0f;

            // Front wheel rpm
            if (frontWheelRpm != null)
            {
                VehicleBase.WheelState frontWheelState = target.wheelState[0];
                if (speed == 0 && (brake > 0 || handBrake > 0))
                {
                    frontWheelRpm.text = "0.";
                }
                else
                {
                    frontWheelRpm.text = ((int)Math.Round(frontWheelState.angularVelocity * 9.549f)).ToString();
                }
            }

            // Back wheel rpm
            if (backWheelRpm != null)
            {
                VehicleBase.WheelState backWheelState = target.wheelState[3];
                if (speed == 0 && (brake > 0 || handBrake > 0))
                {
                    backWheelRpm.text = "0.";
                }
                else
                {
                    backWheelRpm.text = ((int)Math.Round(backWheelState.angularVelocity * 9.549f)).ToString();
                }
            }

            // EnginePower

            if (enginePower != null)
            {
                int powerValue = vehicleData[VehicleData.EnginePower] / 1000;

                if (powerValue > 0)
                    enginePower.text = "+" + powerValue.ToString();
                else
                    enginePower.text = powerValue.ToString();
            }
        }
    }

}