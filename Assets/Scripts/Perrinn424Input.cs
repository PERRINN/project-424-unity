

using UnityEngine;
using VehiclePhysics;
using VehiclePhysics.InputManagement;
using EdyCommonTools;


namespace Perrinn424
{

class Perrinn424InputUser : InputUser
	{
	public Perrinn424InputUser (string idName) : base(idName) { }

	public InputAxis steer 			= new InputAxis("Steer");
	public InputSlider throttle		= new InputSlider("Throttle");
	public InputSlider brake 		= new InputSlider("Brake");
	public InputButton gearUp		= new InputButton("GearShiftUp");
	public InputButton gearDown		= new InputButton("GearShiftDown");
	public InputButton drsEnable	= new InputButton("DrsEnable");
	}


public class Perrinn424Input : VehicleBehaviour
	{
	public string inputUserName = "No name";


	// Private fields

	bool m_ffEnabled = false;
	Perrinn424InputUser m_input;
	Steering.Settings m_steeringSettings;


	// Component implementation


	public override void OnEnableVehicle ()
		{
		m_input = new Perrinn424InputUser(inputUserName);
		InputManager.instance.RegisterUser(m_input);

		m_steeringSettings = vehicle.GetInternalObject(typeof(Steering.Settings)) as Steering.Settings;
		}


	public override void OnDisableVehicle ()
		{
		// Stop force feedback

		InputDevice.ForceFeedback forceFeedback = m_input.steer.ForceFeedback();
		if (m_ffEnabled && forceFeedback != null)
			forceFeedback.StopAllEffects();

		m_ffEnabled = false;

		// Unregister user

		InputManager.instance.UnregisterUser(m_input);
		}


	public override void FixedUpdateVehicle ()
		{
		int[] inputData = vehicle.data.Get(Channel.Input);

		// Throttle, brake

		inputData[InputData.Throttle] = (int)(m_input.throttle.Value() * 10000);
		inputData[InputData.Brake] = (int)(m_input.brake.Value() * 10000);

		// Apply steer based in the physical and logical steering wheel ranges

		float steerInput = m_input.steer.Value();
		float physicalWheelRange = InputManager.instance.settings.physicalWheelRange;

		if (m_steeringSettings != null && m_steeringSettings.steeringWheelRange > 0.0f && physicalWheelRange > 0.0f)
			steerInput = Mathf.Clamp(steerInput / m_steeringSettings.steeringWheelRange * physicalWheelRange, -1.0f, 1.0f);

		inputData[InputData.Steer] = (int)(steerInput * 10000);

		// Gear mode

		if (m_input.gearUp.PressedThisFrame())
			{
			inputData[InputData.AutomaticGear]++;
			Debug.Log("Gear Up");
			}

		if (m_input.gearDown.PressedThisFrame())
			{
			inputData[InputData.AutomaticGear]--;
			Debug.Log("Gear Down");
			}

		// TODO DRS

		// TODO Process force feedback if available

		/*
		InputDevice.ForceFeedback forceFeedback = m_input.steer.ForceFeedback();

		if (m_ffEnabled != forceFeedbackEnabled)
			{
			if (!forceFeedbackEnabled && forceFeedback != null)
				forceFeedback.StopAllEffects();

			m_ffEnabled = forceFeedbackEnabled;
			}

		if (m_ffEnabled && forceFeedback != null)
			{
			ProcessForceFeedback(forceFeedback);
			}
		*/

		}


	}

}