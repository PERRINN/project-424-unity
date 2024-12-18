
using UnityEngine;
using VehiclePhysics;
using EdyCommonTools;


namespace Perrinn424
{

public class VisualPedalRotation : VehicleBehaviour
	{
	// Transforms will be rotated around the local X axis

	public Transform throttlePedal;
	public Transform brakePedal;

	public float throttleMinAngle = 0.0f;
	public float throttleMaxAngle = 16.0f;

	public float brakeMinAngle = 0.0f;
	public float brakeMaxAngle = 8.0f;


	Quaternion m_originalThrottleRotation;
	Quaternion m_originalBrakeRotation;
	InterpolatedFloat m_throttleInput = new InterpolatedFloat();
	InterpolatedFloat m_brakeInput = new InterpolatedFloat();

	Perrinn424CarController m_controller;


	public override void OnEnableVehicle ()
		{
		m_controller = vehicle as Perrinn424CarController;
		if (m_controller == null)
			{
			enabled = false;
			return;
			}

		if (throttlePedal != null)
			m_originalThrottleRotation = throttlePedal.localRotation;

		if (brakePedal != null)
			m_originalBrakeRotation = brakePedal.localRotation;

		(float throttleInput, float brakeInput) = GetPedalInputs();
		m_throttleInput.Reset(throttleInput);
		m_brakeInput.Reset(brakeInput);
		}


	public override void OnDisableVehicle ()
		{
		if (throttlePedal != null)
			throttlePedal.localRotation = m_originalThrottleRotation;

		if (brakePedal != null)
			brakePedal.localRotation = m_originalBrakeRotation;
		}


	public override void FixedUpdateVehicle ()
		{
		(float throttleInput, float brakeInput) = GetPedalInputs();
		m_throttleInput.Set(throttleInput);
		m_brakeInput.Set(brakeInput);
		}


	public override void UpdateVehicle ()
		{
		if (throttlePedal != null)
			{
			float angle = Mathf.Lerp(throttleMinAngle, throttleMaxAngle, m_throttleInput.GetInterpolated());
			throttlePedal.localRotation = m_originalThrottleRotation * Quaternion.AngleAxis(angle, Vector3.right);
			}

		if (brakePedal != null)
			{
			float angle = Mathf.Lerp(brakeMinAngle, brakeMaxAngle, m_brakeInput.GetInterpolated());
			brakePedal.localRotation = m_originalBrakeRotation * Quaternion.AngleAxis(angle, Vector3.right);
			}
		}


	(float, float) GetPedalInputs ()
		{
		return (m_controller.throttlePosition, m_controller.brakePosition);
		}
	}

}