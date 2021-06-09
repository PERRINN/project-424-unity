//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;


namespace VehiclePhysics.Examples
{

using UnityEngine;
using VehiclePhysics;


public class CustomHandlingSetup : VehicleBehaviour
	{
	[Range(0,1)]
	public float aggressiveness = 0.0f;

	[Header("Settings")]

	// Traction Slip requires Traction Control enabled in Custom Slip mode

	public bool useTractionSlip = true;
	public float smoothTractionSlip = 0.4f;
	public float aggressiveTractionSlip = 2.1f;

	// SteeringBias use a VPDeviceInput component

	[Space(5)]
	public bool useSteeringBias = true;
	public float smoothSteeringBias = 0.2f;
	public float aggressiveSteeringBias = 0.5f;

	// Max Engine Torque requires Torque Cap enabled in the Engine settings
	// and the regular torque curve to support the aggressive torque value.

	[Space(5)]
	public bool useMaxEngineTorque = true;
	public float smoothMaxEngineTorque = 120.0f;
	public float aggressiveMaxEngineTorque = 300.0f;


	VPVehicleController m_vehicle;
	#if !VPP_ESSENTIAL
	VPDeviceInput m_deviceInput;
	#endif


	public override void OnEnableVehicle ()
		{
		m_vehicle = vehicle.GetComponent<VPVehicleController>();
		if (m_vehicle == null)
			{
			DebugLogWarning("This component requires a VPVehicleController-based vehicle. Component disabled");
			enabled = false;
			return;
			}

		#if !VPP_ESSENTIAL
		m_deviceInput = vehicle.GetComponentInChildren<VPDeviceInput>();
		#endif
		}


	public override void FixedUpdateVehicle ()
		{
		if (useTractionSlip)
			m_vehicle.tractionControl.customSlip = Mathf.Lerp(smoothTractionSlip, aggressiveTractionSlip, aggressiveness);

		#if !VPP_ESSENTIAL
		if (useSteeringBias && m_deviceInput != null)
			m_deviceInput.steeringNonLinearBias = Mathf.Lerp(smoothSteeringBias, aggressiveSteeringBias, aggressiveness);
		#endif

		if (useMaxEngineTorque)
			m_vehicle.engine.torqueCapLimit = Mathf.Lerp(smoothMaxEngineTorque, aggressiveMaxEngineTorque, aggressiveness);
		}
    }

}