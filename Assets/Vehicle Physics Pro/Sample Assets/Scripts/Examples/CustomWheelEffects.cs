//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;
using VehiclePhysics;
using EdyCommonTools;


namespace VehiclePhysics.Examples
{

public class CustomWheelEffects : VehicleBehaviour
	{
	public PhysicMaterial rumbleMaterial;

	public float noEffectsSpeed = 0.2f;		// No effects below this speed
	public float fullEffectsSpeed = 2.0f;	// Effects have full effect above this speed

	[Header("Rumble")]
	public float amplitude = 0.02f;
	public float periodLength = 1.0f;


	float[] m_wheelPos = new float[0];


	public override void OnEnableVehicle ()
		{
		m_wheelPos = new float[vehicle.wheelCount];
		}


	public override void UpdateVehicleSuspension ()
		{
		float speedFactor = Mathf.InverseLerp(noEffectsSpeed, fullEffectsSpeed, vehicle.speed);

		for (int i = 0; i < vehicle.wheelCount; i++)
			{
			VehicleBase.WheelState ws = vehicle.wheelState[i];

			if (ws.grounded && ws.lastGroundHit.physicMaterial == rumbleMaterial)
				{
				m_wheelPos[i] += ws.localWheelVelocity.magnitude * Time.deltaTime;
				RumbleEffect(ws, m_wheelPos[i], speedFactor);
				}
			}
		}


	void RumbleEffect (VehicleBase.WheelState ws, float linearPos, float factor)
		{
		// Compute a "virtual" bump height and apply a force pretending the
		// wheel is driving over it.

		float bumpHeight = amplitude * Triangle(linearPos, periodLength) * factor;
		float force = ws.wheelCol.runtimeSpringRate * bumpHeight;

		ws.wheelCol.runtimeExtraSuspensionForce = force;
		ws.wheelCol.ApplyForce(force * ws.hit.normal, ws.hit.point, ws.hit.collider.attachedRigidbody);
		}


	float Triangle (float x, float period)
		{
		// Triangle waveform with amplitude -0.5 to +0.5

		return Mathf.PingPong(x * 2.0f / period, 1.0f) - 0.5f;
		}
	}

}