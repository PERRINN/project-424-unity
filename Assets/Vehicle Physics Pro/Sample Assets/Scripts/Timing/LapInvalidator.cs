//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;


namespace VehiclePhysics.Timing
{

public class LapInvalidator : MonoBehaviour
	{
	public float maxAllowedSpeed = 0.0f;
	public LayerMask vehicleLayers = Physics.AllLayers;
	public bool debugInfo = false;

	LapTimer m_lapTimer;


	void OnEnable ()
		{
		m_lapTimer = FindObjectOfType<LapTimer>();

		if (m_lapTimer == null)
			{
			Debug.LogWarning(this.ToString() + ": LapTimer object not found. Component disabled", this);
			enabled = false;
			}
		}


	void OnTriggerEnter (Collider other)
		{
		if (!enabled) return;

		Rigidbody otherRigidbody = other.attachedRigidbody;

		if (otherRigidbody != null
			&& (vehicleLayers & (1<<otherRigidbody.gameObject.layer)) != 0
			&& otherRigidbody.velocity.magnitude > maxAllowedSpeed)
			{
			m_lapTimer.InvalidateLap();

			if (debugInfo)
				Debug.Log(this.ToString() + ": LAP INVALIDATED!");
			}
		}


	void OnCollisionEnter (Collision collision)
		{
		OnTriggerEnter(collision.collider);
		}
	}

}