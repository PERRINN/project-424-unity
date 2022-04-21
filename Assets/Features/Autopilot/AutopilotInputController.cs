using UnityEngine;
using VehiclePhysics;

namespace Perrinn424.AutopilotSystem
{
    [RequireComponent(typeof(BaseAutopilot))]
    public class AutopilotInputController : VehicleBehaviour
    {
        [SerializeField]
        private BaseAutopilot autopilot;

        VPDeviceInput m_deviceInput;
        float m_ffbForceIntensity;
        float m_ffbDamperCoefficient;

        public override void OnEnableVehicle()
        {
            m_deviceInput = vehicle.GetComponentInChildren<VPDeviceInput>();
            if (m_deviceInput != null)
            {
                m_ffbForceIntensity = m_deviceInput.forceIntensity;
                m_ffbDamperCoefficient = m_deviceInput.damperCoefficient;
            }
        }



        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                autopilot.ToggleStatus();
                if (m_deviceInput != null)
                {
                    if (autopilot.IsOn)
                    {
                        m_deviceInput.forceIntensity = 0.0f;
                        m_deviceInput.damperCoefficient = 0.0f;
                    }
                    else
                    {
                        m_deviceInput.forceIntensity = m_ffbForceIntensity;
                        m_deviceInput.damperCoefficient = m_ffbDamperCoefficient;
                    }
                }

                //if (autopilotON)
                //{
                //    autopilotON = false;
                //    SteeringScreen.autopilotState = false;
                //    if (m_deviceInput != null)
                //    {
                //        m_deviceInput.forceIntensity = m_ffbForceIntensity;
                //        m_deviceInput.damperCoefficient = m_ffbDamperCoefficient;
                //    }
                //}
                //else
                //{
                //    autopilotON = true;
                //    SteeringScreen.autopilotState = true;
                //    if (m_deviceInput != null)
                //    {
                //        m_deviceInput.forceIntensity = 0.0f;
                //        m_deviceInput.damperCoefficient = 0.0f;
                //    }
                //}
            }
        }

        private void Reset()
        {
            if (autopilot == null)
            {
                autopilot = this.GetComponent<BaseAutopilot>();
            }
        }
    }
}
