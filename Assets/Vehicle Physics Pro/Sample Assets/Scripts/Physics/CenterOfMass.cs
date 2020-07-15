//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

// Intended for Rigidbodies associated or not with vehicles (i.e. cargo, attachments, track add-ons)
//
// Vehicles shouldn't use this component. Use the centerOfMass property in the vehicle controller instead.

using UnityEngine;
using EdyCommonTools;


namespace VehiclePhysics.Utility
{

[AddComponentMenu("Vehicle Physics/Utility/Center Of Mass", 3)]
[RequireComponent(typeof(Rigidbody))]
public class CenterOfMass : MonoBehaviour
	{
	public Transform centerOfMass;
	public bool showGizmo = true;


	Transform m_transform;
	Rigidbody m_rigidbody;


	void OnEnable ()
		{
		m_transform = transform;
		m_rigidbody = GetComponent<Rigidbody>();
		}


	void FixedUpdate ()
		{
		if (centerOfMass != null)
			{
			Vector3 newCenterOfMass = m_transform.InverseTransformPoint(centerOfMass.position);

			// InverseTransformPoint may induce some very small variations.
			// Using a threshold ensures the center of mass to be modified when it has really changed.
			// The threshold is so small because we're comparing with sqrMagnitude (faster), not magnitude.

			if ((m_rigidbody.centerOfMass - newCenterOfMass).sqrMagnitude > 0.0000001f)
				m_rigidbody.centerOfMass = newCenterOfMass;
			}
		}


	void Update ()
		{
		if (showGizmo)
			DebugUtility.DrawCrossMark(m_transform.TransformPoint(m_rigidbody.centerOfMass), m_transform, GColor.white);
		}

	}

}