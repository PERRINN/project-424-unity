//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;


namespace VehiclePhysics.Utility
{

public class MatchInertia : MonoBehaviour
	{
	public Rigidbody otherRigidbody;
	[Range(0.1f,3.0f)]
	public float matchInertiaFactor = 1.0f;

	public enum ApplyTo { ThisRigidbody, OtherRigidbody }
	public ApplyTo applyInertiaTo = ApplyTo.ThisRigidbody;
	public bool resetInertiaOnDisable = true;


	Rigidbody m_thisRigidbody = null;


	void OnEnable ()
		{
		if (otherRigidbody == null || !otherRigidbody.gameObject.activeInHierarchy)
			{
			Debug.LogWarning(this.ToString() + " Other rigidbody missing or inactive. Component disabled.", this);
			enabled = false;
			return;
			}

		m_thisRigidbody = GetComponentInParent<Rigidbody>();
		if (m_thisRigidbody == null)
			{
			Debug.LogWarning(this.ToString() + " No rigidbody found. Component disabled.", this);
			enabled = false;
			return;
			}

		if (applyInertiaTo == ApplyTo.ThisRigidbody)
			Match(m_thisRigidbody, otherRigidbody);
		else
			Match(otherRigidbody, m_thisRigidbody);
		}


	void Match (Rigidbody toRigidbody, Rigidbody fromRigidbody)
		{
		float massRatio = toRigidbody.mass / fromRigidbody.mass;
		float inertiaMagnitude = fromRigidbody.inertiaTensor.magnitude * massRatio * matchInertiaFactor;
		toRigidbody.inertiaTensor = toRigidbody.inertiaTensor.normalized * Mathf.Max(inertiaMagnitude, Vector3.one.magnitude);
		}


	void OnDisable ()
		{
		if (resetInertiaOnDisable)
			{
			if (applyInertiaTo == ApplyTo.ThisRigidbody)
				{
				if (m_thisRigidbody != null)
					m_thisRigidbody.ResetInertiaTensor();
				}
			else
				{
				if (otherRigidbody != null)
					otherRigidbody.ResetInertiaTensor();
				}
			}

		m_thisRigidbody = null;
		}
	}

}