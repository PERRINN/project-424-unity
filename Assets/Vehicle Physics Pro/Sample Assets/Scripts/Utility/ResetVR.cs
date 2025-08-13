//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;
using EdyCommonTools;
using VersionCompatibility;


namespace VehiclePhysics.Utility
{

public class ResetVR : MonoBehaviour
	{
	public UnityKey recenterKey = UnityKey.H;

	void Update ()
		{
		if (UnityInput.GetKeyDown(recenterKey))
			VrUtility.Recenter();
		}
	}

}