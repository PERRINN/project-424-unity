using Perrinn424.AutopilotSystem;
using Perrinn424.Utilities;
using UnityEngine;
using UnityEngine.UI;
using VehiclePhysics;

namespace Perrinn424.UI
{
    public abstract class BaseDashboard : MonoBehaviour
    {
        [SerializeField]
        protected VehicleBase vehicle;

        [SerializeField]
        private RefreshHelper refreshHelper;

        protected BaseAutopilot autopilot;

        [SerializeField]
        protected Text referenceDiffText;

        [SerializeField]
        protected Text gearText;

        public void OnEnable()
        {
            autopilot = vehicle.GetComponentInChildren<BaseAutopilot>();
        }

        public void Update()
        {
            if (refreshHelper.Update(Time.deltaTime))
            {
                UpdateValues();
            }
        }

        protected abstract void UpdateValues();
        protected abstract void WriteDiffs();

        protected void WriteGear()
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

        protected bool IsDrsOn()
        {
            float drsPosition = vehicle.data.Get(Channel.Custom, Perrinn424Data.DrsPosition) / 1000.0f;
            return drsPosition > 0;
        }
    }
}
