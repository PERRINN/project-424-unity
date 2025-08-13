//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;
using VersionCompatibility;


namespace VehiclePhysics.Utility
{

[RequireComponent(typeof(Rigidbody))]
public class EndlessMovingSurface : MonoBehaviour
	{
	public bool move = false;
	public float velocity = 1.0f;
	public float deltaVelocityOnKey = 1.0f;
	public float maxVelocity = 20.0f;
	public float positionLimit = 400.0f;


	Rigidbody m_rigidbody;


	void OnEnable ()
		{
		m_rigidbody = GetComponent<Rigidbody>();
		}


	void Update ()
		{
		if (UnityInput.GetKeyDown(UnityKey.NumpadPlus))
			velocity += deltaVelocityOnKey;

		if (UnityInput.GetKeyDown(UnityKey.NumpadMinus))
			velocity -= deltaVelocityOnKey;

		if (UnityInput.GetKeyDown(UnityKey.NumpadMultiply))
			move = !move;
		}


	void FixedUpdate ()
		{
		if (move)
			{
			if (transform.localPosition.z > positionLimit)
				{
				Vector3 localPos = transform.localPosition;
				localPos.z = -positionLimit;
				transform.localPosition = localPos;
				}

			velocity = Mathf.Clamp(velocity, -maxVelocity, maxVelocity);

			m_rigidbody.MovePosition(m_rigidbody.position + velocity * Time.deltaTime * Vector3.forward);
			}
		}
	}

}