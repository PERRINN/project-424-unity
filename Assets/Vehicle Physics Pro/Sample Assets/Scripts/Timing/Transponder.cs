//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;


namespace VehiclePhysics.Timing
{

public class Transponder : VehicleBehaviour
	{
	public Transform detectionPoint;
	public PhysicMaterial[] trackMaterials = new PhysicMaterial[0];


	LapTimer m_lapTimer;


	public override void OnEnableVehicle ()
		{
		m_lapTimer = FindObjectOfType<LapTimer>();
		}


	public override void FixedUpdateVehicle ()
		{
		if (m_lapTimer != null && !VerifyOnTrack())
			m_lapTimer.InvalidateLap();
		}


	public void GetPointAndVelocity (out Vector3 point, out Vector3 velocity)
		{
		if (detectionPoint != null)
			point = detectionPoint.position;
		else
			point = transform.position;

		velocity = vehicle.cachedRigidbody.GetPointVelocity(point);
		}


	bool VerifyOnTrack ()
		{
		// Is out of track if three or more wheels are grounded
		// outside the track.

		int outOfTrackWheels = 0;

		foreach (VehicleBase.WheelState ws in vehicle.wheelState)
			{
			if (ws.grounded && !IsValidMaterial(ws.lastGroundHit.physicMaterial))
				outOfTrackWheels++;
			}

		return outOfTrackWheels < 3;
		}


	bool IsValidMaterial (PhysicMaterial material)
		{
		for (int i=0, c=trackMaterials.Length; i<c; i++)
			{
			if (trackMaterials[i] == material)
				return true;
			}

		return false;
		}
	}

}