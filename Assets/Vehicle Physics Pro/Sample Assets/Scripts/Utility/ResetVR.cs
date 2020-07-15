//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;


namespace VehiclePhysics.Utility
{

public class ResetVR : MonoBehaviour
	{
	public KeyCode recenterKey = KeyCode.H;

	void Update ()
		{
		// Surprisingly...
		#if !UNITY_XBOXONE
		if (Input.GetKeyDown(recenterKey))
			UnityEngine.XR.InputTracking.Recenter();
		#endif
		}
	}

}