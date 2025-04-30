//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2023 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

// Move a kinematic rigidbody at a constant velocity.
// May be applicable to vehicles to perform tests at invariant speed.


using UnityEngine;
using EdyCommonTools;


namespace VehiclePhysics.Utility
{

public class KinematicMotion : MonoBehaviour
	{
	public Vector3 velocity = Vector3.forward;


	Transform m_transform;
	Rigidbody m_rigidbody;
	bool m_wasKinematic;


	void OnEnable ()
		{
		m_rigidbody = GetComponentInParent<Rigidbody>();
		if (m_rigidbody == null)
			{
			enabled = false;
			return;
			}

		m_wasKinematic = m_rigidbody.isKinematic;
		m_rigidbody.isKinematic = true;
		}


	void OnDisable ()
		{
		if (m_rigidbody != null)
			m_rigidbody.isKinematic = m_wasKinematic;
		}


	void FixedUpdate ()
		{
		m_rigidbody.MovePosition(m_rigidbody.position + velocity * Time.deltaTime);
		}
	}

}