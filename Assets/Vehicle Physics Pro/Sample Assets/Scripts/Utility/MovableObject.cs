//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2023 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;
using VersionCompatibility;


namespace VehiclePhysics.Utility
{

[AddComponentMenu("Vehicle Physics/Utility/Movable Object", 43)]
public class MovableObject : MonoBehaviour
	{
	public enum AutoMovement { None, Sine, Triangle };

	public float position = 0.0f;
	public float max = 0.25f;
	public float min = -0.25f;
	public Vector3 direction = Vector3.up;

	public float rate = 0.2f;
	public AutoMovement autoMode = AutoMovement.None;
	public float autoAmplitude = 0.2f;
	public float autoFrequency = 1.0f;
	public bool autoPositiveOnly = false;

	public UnityKey keyIncrement = UnityKey.NumpadPlus;
	public UnityKey keyDecrement = UnityKey.NumpadMinus;


	Rigidbody m_rb;
	Vector3 m_basePos;
	float m_offset;


	void OnEnable ()
		{
		m_rb = GetComponent<Rigidbody>();
		m_basePos = transform.position;
		m_offset = 0.0f;
		}


	void OnDisable ()
		{
		if (m_rb)
			m_rb.MovePosition(m_basePos);
		transform.position = m_basePos;
		}


	void FixedUpdate ()
		{
		if (UnityInput.GetKey(keyIncrement)) position += rate * Time.deltaTime;
		if (UnityInput.GetKey(keyDecrement)) position -= rate * Time.deltaTime;

		switch (autoMode)
			{
			case AutoMovement.Sine:
				{
				m_offset = Mathf.Sin(2.0f*Mathf.PI * autoFrequency * Time.time) * autoAmplitude/2.0f;
				break;
				}

			case AutoMovement.Triangle:
				{
				m_offset = -autoAmplitude/2 + Mathf.PingPong(Time.time*autoAmplitude*2*autoFrequency, autoAmplitude);
				break;
				}

			default:
				m_offset = 0.0f;
				break;
			}

		if (autoPositiveOnly)
			m_offset = Mathf.Abs(m_offset);

		position = Mathf.Clamp(position, min, max);
		Vector3 newPos = m_basePos + direction * (position + m_offset);

		if (m_rb)
			m_rb.MovePosition(newPos);
		else
			transform.position = newPos;
		}
	}

}