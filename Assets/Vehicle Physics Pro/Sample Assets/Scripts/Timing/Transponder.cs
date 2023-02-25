//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;
using System;
using EdyCommonTools;


namespace VehiclePhysics.Timing
{

public class Transponder : VehicleBehaviour
	{
	public Transform detectionPoint;
	public PhysicMaterial[] trackMaterials = new PhysicMaterial[0];

	[Space(5)]
	public bool debugGizmos = false;


	// LapSectorMark uses these for preventing double-hits

	[NonSerialized]
	public Collider lastColliderHit = null;
	[NonSerialized]
	public float lastHitTime = -10.0f;


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


	public override void UpdateVehicle ()
		{
		if (debugGizmos)
			{
			Vector3 point = detectionPoint != null?	point = detectionPoint.position : transform.position;
			DebugUtility.DrawCrossMark(point, transform, GColor.orange);
			}
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
		// No valid materials specified = all materials valid
		if (trackMaterials.Length == 0) return true;

		for (int i=0, c=trackMaterials.Length; i<c; i++)
			{
			if (trackMaterials[i] == material)
				return true;
			}

		return false;
		}
	}

}