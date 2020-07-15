//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;


namespace VehiclePhysics.Examples
{

public class CustomInputLogic : VehicleBehaviour
	{
	bool m_hasStarted;


	public override void OnEnableVehicle ()
		{
		m_hasStarted = false;

		vehicle.data.Set(Channel.Input, InputData.Key, 0); // Acc-On

		vehicle.onBeforeUpdateBlocks += ProcessInputHook;
		}


	public override void OnDisableVehicle ()
		{
		vehicle.onBeforeUpdateBlocks -= ProcessInputHook;
		}


	public void ProcessInputHook ()
		{
		int[] inputData = vehicle.data.Get(Channel.Input);

		if (!IsStarted())
			{
			// Vehicle not started

			// Apply handbrake

			inputData[InputData.Handbrake] = 10000;

			// Set gear P if it has never started

			if (!m_hasStarted)
				{
				inputData[InputData.AutomaticGear] = 1;	// P
				}

			// Pressing brakes starts the engine

			if (inputData[InputData.Brake] > 5000)
				inputData[InputData.Key] = 1;	// Start
			else
				inputData[InputData.Key] = 0;	// Acc-On
			}
		else
			{
			// Vehicle started

			m_hasStarted = true;

			// Release ignition key and handbrake;

			inputData[InputData.Key] = 0;
			inputData[InputData.Handbrake] = 0;

			// Handle regular driving vs. reverse

			int currentGear = vehicle.data.Get(Channel.Vehicle, VehicleData.GearboxGear);

			if (currentGear < 0)
				{
				// Reverse:
				// - Clutch --> throttle in reverse
				// - Throttle, Brake --> brake

				inputData[InputData.Brake] = Mathf.Max(inputData[InputData.Brake], inputData[InputData.Throttle]);
				inputData[InputData.Throttle] = inputData[InputData.Clutch];
				}

			// Clutch engages (or tries to engage) R. No clutch input is sent to the vehicle.

			if (inputData[InputData.Clutch] > 1000)
				{
				inputData[InputData.AutomaticGear] = 2;	// R
				inputData[InputData.Clutch] = 0;
				}
			else
			if (inputData[InputData.Brake] > 1000)
				{
				inputData[InputData.AutomaticGear] = 4;	// D
				}
			}
		}


	bool IsStarted ()
		{
		return vehicle.data.Get(Channel.Vehicle, VehicleData.EngineWorking) != 0;
		}
	}

}