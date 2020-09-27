//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;
using EdyCommonTools;


namespace VehiclePhysics.Utility
{

public class ResetVR : MonoBehaviour
	{
	public KeyCode recenterKey = KeyCode.H;

	void Update ()
		{
		if (Input.GetKeyDown(recenterKey))
			VrUtility.Recenter();
		}
	}

}