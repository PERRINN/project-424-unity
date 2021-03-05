
// Script that enables driving aids in the 424 automatically
// when no steering wheel device is found.


using UnityEngine;
using VehiclePhysics;


public class AutoEnableDrivingAids : VehicleBehaviour
	{
	VPDeviceInput m_deviceInput;
	Perrinn424CarController m_controller;


	public override void OnEnableVehicle ()
		{
		m_controller = vehicle.GetComponent<Perrinn424CarController>();
		m_deviceInput = vehicle.GetComponentInChildren<VPDeviceInput>();

		if (m_controller == null || m_deviceInput == null)
			enabled = false;
		}


	public override void UpdateAfterFixedUpdate ()
		{
		if (m_deviceInput.isActiveAndEnabled)
			{
			m_controller.steeringAids.helpMode = SteeringAids.HelpMode.Disabled;
			m_controller.steeringAids.limitMode = SteeringAids.LimitMode.Disabled;
			m_controller.tractionControl.enabled = false;
			}
		else
			{
			m_controller.steeringAids.helpMode = SteeringAids.HelpMode.AssistedSteerAngle;
			m_controller.steeringAids.limitMode = SteeringAids.LimitMode.CustomSlipAngle;
			m_controller.tractionControl.enabled = true;
			}
		}
	}
