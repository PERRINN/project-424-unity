//------------------------------------------------------------------------------------------------
// Edy's Vehicle Physics
// (c) Angel Garcia "Edy" - Oviedo, Spain
// http://www.edy.es
//------------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using VersionCompatibility;


[RequireComponent(typeof(Rigidbody))]
public class RigidbodyImpulse : MonoBehaviour
	{
	public enum Mode { Local, World };

	public float velocity = 6.0f;
	public Vector3 direction = Vector3.up;
	public Mode mode = Mode.World;
	public UnityKey key = UnityKey.E;

	Rigidbody m_rigidbody;


	void OnEnable ()
		{
		m_rigidbody = GetComponent<Rigidbody>();
		}


	void Update ()
		{
		if (UnityInput.GetKeyDown(key))
			{
			if (mode == Mode.World)
				m_rigidbody.AddForce(direction.normalized * velocity, ForceMode.VelocityChange);
			else
				m_rigidbody.AddRelativeForce(direction.normalized * velocity, ForceMode.VelocityChange);
			}
		}
	}
