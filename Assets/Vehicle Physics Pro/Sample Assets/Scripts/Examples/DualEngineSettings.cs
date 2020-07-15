//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

// This example shows how to store two engine setups and change among them.
// This might be used, for example, as base for implementing a turbocharger. In this case
// one of the setups would be the engine with the turbocharger active.

using UnityEngine;


namespace VehiclePhysics.Examples
{

public class DualEngineSettings : VehicleBehaviour
	{
	public Engine.Settings engineSetupA;
	public Engine.Settings engineSetupB;

	public bool useSetupB = false;

	Engine m_engine;


	public override void OnEnableVehicle ()
		{
		m_engine = vehicle.GetInternalObject(typeof(Engine)) as Engine;
		if (m_engine == null)
			{
			Debug.Log("This vehicle has no Engine block. Component disabled.");
			enabled = false;
			}
		}


	public override void FixedUpdateVehicle ()
		{
		if (useSetupB)
			m_engine.settings = engineSetupB;
		else
			m_engine.settings = engineSetupA;
		}
	}
}
