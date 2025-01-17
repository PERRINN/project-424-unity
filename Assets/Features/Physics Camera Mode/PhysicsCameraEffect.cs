
using UnityEngine;
using VehiclePhysics;

using EdyCommonTools;


namespace Perrinn424.CameraSystem
{

public class PhysicsCameraEffect : VehicleBehaviour
	{
	[Range(-180,180)]
	public float rollAngle = 0.0f;
	[Range(-10,10)]
	public float pitchAngle = 0.0f;
	[Range(-10,10)]
	public float yawAngle = 0.0f;

	Vector3 m_originalLocalPosition;
	Quaternion m_originalLocalRotation;


	Rigidbody m_vehicleRb;
	Transform m_baseTransform;

	Vector3 m_camOffset;
	Vector3 m_camForward;
	Vector3 m_camUp;


	public override void OnEnableVehicle ()
		{
		// Store current local position

		m_originalLocalPosition = transform.localPosition;
		m_originalLocalRotation = transform.localRotation;

		// Local cam pose relative to the rigidbody

		m_vehicleRb = vehicle.cachedRigidbody;
		m_baseTransform = vehicle.cachedRigidbody.transform;

		m_camOffset = m_baseTransform.InverseTransformPoint(transform.position) - m_vehicleRb.centerOfMass;
		m_camForward = m_baseTransform.InverseTransformDirection(transform.forward);
		m_camUp = m_baseTransform.InverseTransformDirection(transform.up);
		}


	void LateUpdate ()
		{
		Quaternion baseRotation = m_baseTransform.rotation;
		Quaternion roll = Quaternion.AngleAxis(rollAngle, Vector3.forward);

		transform.position = m_vehicleRb.worldCenterOfMass + baseRotation * roll * m_camOffset;

		transform.rotation = Quaternion.LookRotation(baseRotation * roll * m_camForward, baseRotation * roll * m_camUp);

		DebugUtility.DrawCrossMark(transform.position, transform, GColor.red);
		Debug.DrawLine(transform.position, transform.position + transform.forward, GColor.red);
		}


	public override void OnDisableVehicle ()
		{
		// Restore local position

		transform.localPosition = m_originalLocalPosition;
		transform.localRotation = m_originalLocalRotation;
		}
	}

}