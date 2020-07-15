//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;


namespace VehiclePhysics.Examples
{

public class SimpleVehicleControllerInput : VehicleBehaviour
	{
	SimpleVehicleController m_vehicle;


	public override void OnEnableVehicle ()
		{
		// This component requires a SimpleVehicleController explicitly
		m_vehicle = vehicle.GetComponent<SimpleVehicleController>();
		if (m_vehicle == null)
			{
			DebugLogWarning("A vehicle based on SimpleVehicleController is required. Component disabled.");
			enabled = false;
			}
		}


	public override void UpdateVehicle ()
		{
		float steerInput = Mathf.Clamp(Input.GetAxis("Horizontal"), -1.0f, 1.0f);

		float throttleAndBrakeAxisValue = Input.GetAxis("Vertical");
		float throttleInput = Mathf.Clamp01(throttleAndBrakeAxisValue);
		float brakeInput = Mathf.Clamp01(-throttleAndBrakeAxisValue);

		if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
			{
			throttleInput = -brakeInput;
			brakeInput = 0.0f;
			}

		m_vehicle.steerInput = steerInput;
		m_vehicle.driveInput = throttleInput;
		m_vehicle.brakeInput = brakeInput;
		}
	}
}