//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

// Using this in a VPVehicleController component requires its center of mass parameter to be null

using UnityEngine;
using EdyCommonTools;


namespace VehiclePhysics.Utility
{

[RequireComponent(typeof(Rigidbody))]
public class VariableCargo : MonoBehaviour
	{
	public float unloadedMass = 500.0f;
	public float loadedMass = 2000.0f;
	public Transform unloadedCOM;
	public Transform loadedCOM;
	[Range(0,1)]
	public float load = 0.0f;

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
		m_rigidbody.mass = Mathf.Lerp(unloadedMass, loadedMass, load);

		if (unloadedCOM != null && loadedCOM != null)
			{
			Vector3 localUnloadedCOM = m_transform.InverseTransformPoint(unloadedCOM.position);
			Vector3 localLoadedCOM = m_transform.InverseTransformPoint(loadedCOM.position);

			SetCenterOfMass(Vector3.Lerp(localUnloadedCOM, localLoadedCOM, load));
			}
		else
		if (unloadedCOM != null)
			{
			SetCenterOfMass(m_transform.InverseTransformPoint(unloadedCOM.position));
			}
		else
		if (loadedCOM != null)
			{
			SetCenterOfMass(m_transform.InverseTransformPoint(loadedCOM.position));
			}
		}


	void SetCenterOfMass (Vector3 localCenterOfMass)
		{
		// Using a threshold ensures the center of mass to be modified when it has really changed.
		// The threshold is so small because we're comparing with sqrMagnitude (faster), not magnitude.

		if ((m_rigidbody.centerOfMass - localCenterOfMass).sqrMagnitude > 0.0000001f)
			m_rigidbody.centerOfMass = localCenterOfMass;
		}


	void Update ()
		{
		if (showGizmo)
			DebugUtility.DrawCrossMark(m_transform.TransformPoint(m_rigidbody.centerOfMass), m_transform, GColor.white);
		}
	}

}