//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;


namespace VehiclePhysics.Examples
{

public class SimpleTrackControllerInput : VehicleBehaviour
	{
	public float centerThreshold = 0.4f;


	SimpleTrackController m_vehicle;


	public override void OnEnableVehicle ()
		{
		// This component requires a SimpleTrackController explicitly
		m_vehicle = vehicle.GetComponent<SimpleTrackController>();
		if (m_vehicle == null)
			{
			DebugLogWarning("A vehicle based on SimpleTrackController is required. Component disabled.");
			enabled = false;
			}
		}


	public override void UpdateVehicle ()
		{
		float vertical = Input.GetAxis("Vertical");
		float horizontal = Input.GetAxis("Horizontal");

		int forwards = Mathf.Abs(vertical) < centerThreshold ? 0 : (int)Mathf.Sign(vertical);
		int sideways = Mathf.Abs(horizontal) < centerThreshold ? 0 : (int)Mathf.Sign(horizontal);

		int leftTrack = 0;
		int rightTrack = 0;

		// Control matrix

		if (forwards > 0)
			{
			if (sideways > 0)
				{
				leftTrack = +1;
				rightTrack = 0;
				}
			else
			if (sideways < 0)
				{
				leftTrack = 0;
				rightTrack = +1;
				}
			else
				{
				leftTrack = +1;
				rightTrack = +1;
				}
			}
		else
		if (forwards < 0)
			{
			if (sideways > 0)
				{
				leftTrack = -1;
				rightTrack = 0;
				}
			else
			if (sideways < 0)
				{
				leftTrack = 0;
				rightTrack = -1;
				}
			else
				{
				leftTrack = -1;
				rightTrack = -1;
				}
			}
		else
			{
			if (sideways > 0)
				{
				leftTrack = +1;
				rightTrack = -1;
				}
			else
			if (sideways < 0)
				{
				leftTrack = -1;
				rightTrack = +1;
				}
			}

		// Send the result to the corresponding tracks

		m_vehicle.leftTrackInput = leftTrack;
		m_vehicle.rightTrackInput = rightTrack;
		}
	}
}