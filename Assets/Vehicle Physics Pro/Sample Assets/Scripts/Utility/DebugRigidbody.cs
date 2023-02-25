//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;
using EdyCommonTools;

namespace VehiclePhysics.Utility
{

public class DebugRigidbody : MonoBehaviour
	{
	public bool showLabel = true;
	public bool showInertiaTensor = false;

	#if UNITY_EDITOR

	Rigidbody m_rigidbody;
	Vector3 m_drawPosition;


	void OnEnable ()
		{
		m_rigidbody = GetComponentInParent<Rigidbody>();
		if (m_rigidbody == null)
			{
			enabled = false;
			return;
			}

		m_drawPosition = transform.TransformPoint(m_rigidbody.centerOfMass);
		}


	void OnDisable ()
		{
		m_rigidbody = null;
		}


	void Update ()
		{
		m_drawPosition = transform.TransformPoint(m_rigidbody.centerOfMass);
		}


	[ContextMenu ("Recompute Inertia")]
	public void RecomputeInertia()
		{
		if (m_rigidbody != null)
			m_rigidbody.ResetInertiaTensor();
		}


	public void OnDrawGizmos ()
		{
		if (!isActiveAndEnabled) return;

		if (m_rigidbody != null)
			{
			if (showLabel)
				{
				UnityEditor.Handles.Label(m_drawPosition, m_rigidbody.name
					+ "\nM: " + m_rigidbody.mass
					+ "\nI: " + m_rigidbody.inertiaTensor + " - " + m_rigidbody.inertiaTensor.magnitude.ToString("0.0")
					+ "\nR: " + m_rigidbody.inertiaTensorRotation.eulerAngles
					+ $"\nV: {m_rigidbody.velocity.ToString("0.000")}"
					);
				}

			if (showInertiaTensor)
				{
				Gizmos.matrix = Matrix4x4.TRS(
					m_drawPosition,
					transform.rotation * m_rigidbody.inertiaTensorRotation,
					Vector3.one);

				Vector3 normalizedInertia = m_rigidbody.inertiaTensor / m_rigidbody.mass;
				float maxDim = Mathf.Max(normalizedInertia.x, Mathf.Max(normalizedInertia.y, normalizedInertia.z));

				Vector3 inertiaX = new Vector3(maxDim, normalizedInertia.x, normalizedInertia.x);
				Vector3 inertiaY = new Vector3(normalizedInertia.y, maxDim, normalizedInertia.y);
				Vector3 inertiaZ = new Vector3(normalizedInertia.z, normalizedInertia.z, maxDim);

				Gizmos.color = GColor.Alpha(GColor.accentRed, 0.7f);
				Gizmos.DrawWireCube(Vector3.zero, inertiaX);

				Gizmos.color = GColor.Alpha(GColor.accentGreen, 0.7f);
				Gizmos.DrawWireCube(Vector3.zero, inertiaY);

				Gizmos.color = GColor.Alpha(GColor.accentBlue, 0.7f);
				Gizmos.DrawWireCube(Vector3.zero, inertiaZ);
				}
			}
		}

	#endif
	}

}