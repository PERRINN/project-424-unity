//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;


namespace VehiclePhysics.Examples
{

public class WheelSuspensionAnimator : VehicleBehaviour
	{
	// References to your 3D model / animator / etc here


	public override void FixedUpdateVehicle ()
		{
		for (int i = 0; i < vehicle.wheelState.Length; i++)
			{
			// VehicleBase.WheelState ws = vehicle.wheelState[i];

			// Here, for this wheel [i] you can access this data:

			// ws.contactDepth            -> distance compressed in m. 0 = no compressed.
			// ws.suspensionCompression   -> 0 = no compression (extended), 1 = fully compressed

			// You may get the distance from the maximum compression by substracting ws.contactDepth
			// from the suspension distance:

			// float wheelDistance = ws.wheelCol.suspensionDistance - ws.contactDepth;
			}

		}

	}

}