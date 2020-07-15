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

public class SimpleTurbocharger : VehicleBehaviour
	{
	public float regularPeakTorque = 180.0f;
	public float turbochargedTorque = 250.0f;

	[Range(0,1)]
	public float turbochargeRatio = 0.0f;


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
		// --- Do your turbocharger logic here ---
		//
		// You may read the engine rpms and load and come up with a value for turbochargeRatio here.
		// (see Databus reference: http://vehiclephysics.com/advanced/databus-reference)
		// ----

		m_engine.settings.peakRpmTorque = Mathf.Lerp(regularPeakTorque, turbochargedTorque, turbochargeRatio);
		}
	}
}
