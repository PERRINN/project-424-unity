using System;
using UnityEngine;
using UnityEngine.UI;
using VehiclePhysics;

namespace Perrinn424.UI
{
    public class OnBoardDashboard : BaseDashboard
    {
        [SerializeField]
        private Text speedText;

        [SerializeField]
        private Image image;

        protected override void UpdateValues()
        {
            WriteSpeedInfo();
            WriteDiffs();
            WriteGear();
            WriteDRS();
        }

        private void WriteSpeedInfo()
        {
            float speed = vehicle.data.Get(Channel.Vehicle, VehicleData.Speed) / 1000.0f;
            speedText.text = $"{speed:F0}";
        }

        protected override void WriteDiffs()
        {
            float value = Mathf.Clamp(autopilot.DeltaTime, -99.99f, 99.99f);
            referenceDiffText.text = value.ToString("+0.00;-0.00"); ;
        }

        private void WriteDRS()
        {
            image.enabled = IsDrsOn();
        }
    } 
}
