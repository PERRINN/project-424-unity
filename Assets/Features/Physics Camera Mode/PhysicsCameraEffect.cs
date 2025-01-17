
using UnityEngine;
using VehiclePhysics;

using EdyCommonTools;


namespace Perrinn424.CameraSystem
{

public class PhysicsCameraEffect : VehicleBehaviour
	{
	[Range(-180,180)]
	public float rollAngle = 0.0f;
	[Range(-180,180)]
	public float pitchAngle = 0.0f;
	[Range(-180,180)]
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
		Quaternion roll = baseRotation * Quaternion.AngleAxis(rollAngle, Vector3.forward);
		Quaternion pitch = baseRotation * Quaternion.AngleAxis(pitchAngle, Vector3.right);
		Quaternion yaw = baseRotation * Quaternion.AngleAxis(yawAngle, Vector3.up);

		// transform.position = m_vehicleRb.worldCenterOfMass + roll * m_camOffset;
		// transform.position = m_vehicleRb.worldCenterOfMass + yaw * m_camOffset;
		transform.position = m_vehicleRb.worldCenterOfMass + pitch * m_camOffset;

		// transform.rotation = Quaternion.LookRotation(roll * m_camForward, roll * m_camUp);
		// transform.rotation = Quaternion.LookRotation(yaw * m_camForward, yaw * m_camUp);
		transform.rotation = Quaternion.LookRotation(pitch * m_camForward, pitch * m_camUp);

		DebugUtility.DrawCrossMark(transform.position, transform, GColor.red);
		DebugUtility.DrawCrossMark(m_vehicleRb.worldCenterOfMass, m_baseTransform, GColor.white);
		Debug.DrawLine(transform.position, transform.position + transform.forward, GColor.red);
		Debug.DrawLine(m_vehicleRb.worldCenterOfMass, transform.position);
		}


	public override void OnDisableVehicle ()
		{
		// Restore local position

		transform.localPosition = m_originalLocalPosition;
		transform.localRotation = m_originalLocalRotation;
		}
	}

}