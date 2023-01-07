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
            speedText.text = $"{speed * 3.6f:F0}";
        }

        protected override void WriteDiffs()
        {
            referenceDiffText.text = autopilot.DeltaTime.ToString("+0.00;-0.00"); ;
        }

        private void WriteDRS()
        {
            image.enabled = IsDrsOn();
        }
    } 
}
