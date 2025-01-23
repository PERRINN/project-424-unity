
using UnityEngine;
using VehiclePhysics;

using EdyCommonTools;


namespace Perrinn424.CameraSystem
{

public class PhysicsCameraEffect : VehicleBehaviour
	{
	public bool roll = true;
	public bool pitch = true;
	public bool yaw = false;

	[Range(-30,30)]
	public float rollAngle = 0.0f;
	[Range(-30,30)]
	public float pitchAngle = 0.0f;
	[Range(-30,30)]
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

	public override void UpdateAfterFixedUpdate ()
		{
		int[] customData = vehicle.data.Get(Channel.Custom);

		if (roll)
			{
			float fronRollAngle = customData[Perrinn424Data.FrontRollAngle] / 1000.0f;
			float rearRollAngle = customData[Perrinn424Data.RearRollAngle] / 1000.0f;
			rollAngle = -(fronRollAngle + rearRollAngle) / 2.0f;
			}
		else
			{
			rollAngle = 0.0f;
			}

		if (pitch)
			{
			float groundAngle = customData[Perrinn424Data.GroundAngle] / 1000.0f;
			float pitch = vehicle.cachedTransform.rotation.eulerAngles.x;
			if (pitch > 180) pitch -= 360;
			pitchAngle = -(pitch + groundAngle);
			}
		else
			{
			pitchAngle = 0.0f;
			}

		if (yaw)
			yawAngle = vehicle.speed > 1.0f ? vehicle.speedAngle : 0.0f;
		else
			yawAngle = 0.0f;
		}


	void LateUpdate ()
		{
		Vector3 basePosition = m_baseTransform.TransformPoint(m_vehicleRb.centerOfMass);
		Quaternion baseRotation = m_baseTransform.rotation;
		Quaternion roll = Quaternion.AngleAxis(rollAngle, Vector3.forward);
		Quaternion pitch = Quaternion.AngleAxis(pitchAngle, Vector3.right);
		Quaternion yaw = Quaternion.AngleAxis(yawAngle, Vector3.up);
		Quaternion rotation = baseRotation * roll * pitch * yaw;

		transform.position = basePosition + rotation * m_camOffset;
		transform.rotation = Quaternion.LookRotation(rotation * m_camForward, rotation * m_camUp);

		DebugUtility.DrawCrossMark(transform.position, transform, GColor.red);
		DebugUtility.DrawCrossMark(basePosition, m_baseTransform, GColor.white);
		Debug.DrawLine(transform.position, transform.position + transform.forward, GColor.red);
		Debug.DrawLine(basePosition, transform.position);
		}


	public override void OnDisableVehicle ()
		{
		// Restore local position

		transform.localPosition = m_originalLocalPosition;
		transform.localRotation = m_originalLocalRotation;
		}
	}

}