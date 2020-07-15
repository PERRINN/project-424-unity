//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;


namespace VehiclePhysics.Utility
{

[RequireComponent(typeof(Rigidbody))]
public class CopyMomentOfInertia : MonoBehaviour
	{
	public Rigidbody otherRigidbody;
	public bool proportionalToMass;
	public bool debugLabel;

	Rigidbody m_thisRigidbody = null;
	Vector3 m_originalInertiaTensor;
	Quaternion m_originalInertiaTensorRotation;


	void OnEnable ()
		{
		if (otherRigidbody != null)
			{
			m_thisRigidbody = GetComponent<Rigidbody>();
			m_originalInertiaTensor = m_thisRigidbody.inertiaTensor;
			m_originalInertiaTensorRotation = m_thisRigidbody.inertiaTensorRotation;

			Vector3 otherInertiaTensor = otherRigidbody.inertiaTensor;

			if (proportionalToMass)
				otherInertiaTensor *= m_thisRigidbody.mass / otherRigidbody.mass;

			m_thisRigidbody.inertiaTensor = otherInertiaTensor;
			m_thisRigidbody.inertiaTensorRotation = otherRigidbody.inertiaTensorRotation;
			}
		}


	void OnDisable ()
		{
		if (m_thisRigidbody != null)
			{
			m_thisRigidbody.inertiaTensor = m_originalInertiaTensor;
			m_thisRigidbody.inertiaTensorRotation = m_originalInertiaTensorRotation;
			m_thisRigidbody = null;
			}
		}


	#if UNITY_EDITOR
	public void OnDrawGizmos ()
		{
		if (Application.isPlaying && debugLabel && m_thisRigidbody != null)
			{
			UnityEditor.Handles.Label(m_thisRigidbody.transform.position, "I: " + m_thisRigidbody.inertiaTensor + "\nR: " + m_thisRigidbody.inertiaTensorRotation.eulerAngles);
			}
		}
	#endif

	}

}