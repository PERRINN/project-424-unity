
using UnityEngine;
using VehiclePhysics;
using EdyCommonTools;


namespace Perrinn424.CameraSystem
{

public class PhysicsCameraEffect : VehicleBehaviour
	{
	public bool enableRoll = true;
	public bool enablePitch = true;
	public bool enableYaw = true;


	Vector3 m_originalLocalPosition;
	Quaternion m_originalLocalRotation;

	Rigidbody m_vehicleRb;
	Transform m_baseTransform;

	Vector3 m_camOffset;
	Vector3 m_camForward;
	Vector3 m_camUp;

	InterpolatedFloat m_rollAngle = new InterpolatedFloat();
	InterpolatedFloat m_pitchAngle = new InterpolatedFloat();
	InterpolatedFloat m_yawAngle = new InterpolatedFloat();


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


	public override void FixedUpdateVehicle ()
		{
		int[] customData = vehicle.data.Get(Channel.Custom);

		if (enableRoll)
			{
			float fronRollAngle = customData[Perrinn424Data.FrontRollAngle] / 1000.0f;
			float rearRollAngle = customData[Perrinn424Data.RearRollAngle] / 1000.0f;
			m_rollAngle.Set(-(fronRollAngle + rearRollAngle) / 2.0f);
			}
		else
			{
			m_rollAngle.Set(0.0f);
			}

		if (enablePitch)
			{
			float groundAngle = customData[Perrinn424Data.GroundAngle] / 1000.0f;
			float pitch = vehicle.cachedTransform.rotation.eulerAngles.x;
			if (pitch > 180) pitch -= 360;
			m_pitchAngle.Set(-(pitch + groundAngle));
			}
		else
			{
			m_pitchAngle.Set(0.0f);
			}

		if (enableYaw)
			m_yawAngle.Set(vehicle.speed > 1.0f ? vehicle.speedAngle : 0.0f);
		else
			m_yawAngle.Set(0.0f);
		}


	void LateUpdate ()
		{
		float frameRatio = InterpolatedFloat.GetFrameRatio();

		Vector3 basePosition = m_baseTransform.TransformPoint(m_vehicleRb.centerOfMass);
		Quaternion baseRotation = m_baseTransform.rotation;
		Quaternion roll = Quaternion.AngleAxis(m_rollAngle.GetInterpolated(frameRatio), Vector3.forward);
		Quaternion pitch = Quaternion.AngleAxis(m_pitchAngle.GetInterpolated(frameRatio), Vector3.right);
		Quaternion yaw = Quaternion.AngleAxis(m_yawAngle.GetInterpolated(frameRatio), Vector3.up);
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