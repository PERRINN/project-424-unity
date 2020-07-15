//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class ResetVehicleLegacy : MonoBehaviour
	{
	public KeyCode resetKey = KeyCode.Return;
	public float deltaHeight = 1.6f;


	bool m_doReset = false;
	Rigidbody m_rigidbody;


	void OnEnable ()
		{
		m_rigidbody = GetComponent<Rigidbody>();
		}


	void Update ()
		{
        if (Input.GetKeyDown(resetKey)) m_doReset = true;
		}


	void FixedUpdate ()
		{
		if (m_doReset)
			{
			DoReset();
			m_doReset = false;
			}
		}


	public void DoReset ()
		{
		if (isActiveAndEnabled)
			{
			Vector3 eulerAngles = transform.localEulerAngles;
			m_rigidbody.MoveRotation(Quaternion.Euler(0, eulerAngles.y, 0));
			m_rigidbody.MovePosition(m_rigidbody.position + Vector3.up * deltaHeight);

			m_rigidbody.velocity = Vector3.zero;
			m_rigidbody.angularVelocity = Vector3.zero;
			}
		}
	}
