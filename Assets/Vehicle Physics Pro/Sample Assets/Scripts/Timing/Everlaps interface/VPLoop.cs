//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2025 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;
using VersionCompatibility;


namespace VehiclePhysics.Timing.EverLaps
{

public class VPLoop : MonoBehaviour
	{
	public int id;
	public bool insidePit;


	VPDecoder m_decoder;


	void OnEnable ()
		{
		m_decoder = GetComponentInParent<VPDecoder>();
		if (m_decoder == null)
			{
			Debug.LogError("VPProbe must be child of a VPDecoder object. Component disabled");
			enabled = false;
			}
		}


	void OnTriggerEnter (Collider other)
		{
		if (!enabled) return;

		UnityRigidbody rb = other.attachedRigidbody;

		VPTransponder transponder = rb.transform.GetComponent<VPTransponder>();
		if (transponder != null)
			{
			Vector3 velocity = rb.linearVelocity;
			velocity.y = 0.0f;

			m_decoder.Pass(this, transponder, Time.time, velocity.magnitude);
			}
		}
	}

}
