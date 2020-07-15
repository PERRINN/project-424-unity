//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;


namespace VehiclePhysics.Utility
{

[AddComponentMenu("Vehicle Physics/Effects/Blow Shadow", 22)]
public class BlowShadow : MonoBehaviour
	{
	public float forwardAngle = 0.0f;

	Vector3 m_localPosition;
	Quaternion m_localRotation;


	void OnEnable ()
		{
		m_localPosition = transform.localPosition;
		m_localRotation = transform.localRotation;
		}


	void OnDisable ()
		{
		transform.localPosition = m_localPosition;
		transform.localRotation = m_localRotation;
		}


	void Update ()
		{
		Vector3 forward = transform.parent.forward;
		forward.y = 0.0f;

		Quaternion rot = Quaternion.LookRotation(forward);
		transform.position = transform.parent.position + rot * m_localPosition;
		transform.rotation = Quaternion.AngleAxis(forwardAngle, Vector3.up) * Quaternion.LookRotation(-Vector3.up, forward);
		}
	}

}