

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
	public InputButton liftAndCoast	= new InputButton("LiftAndCoast");
	}


public class Perrinn424Input : VehicleBehaviour
	{
	public string inputUserName = "No name";

	[Header("Force Feedback")]
	public bool forceFeedbackEnabled = true;

	public enum ForceFeedbackModel { V1, V2 }
	public ForceFeedbackModel forceFeedbackModel = ForceFeedbackModel.V1;
	public bool ffbTelemetry = true;
	[Header("Force Feedback V2")]
	public ForceFeedbackModelV2.Settings ffbSettingsV2 = new ForceFeedbackModelV2.Settings();
	[Header("Force Feedback V1")]
	public ForceFeedbackModelV1.Settings ffbSettingsV1 = new ForceFeedbackModelV1.Settings();
	public bool rumbleEnabled = false;
	public ForceFeedbackModelV1.RumbleSettings ffbRumbleV1 = new ForceFeedbackModelV1.RumbleSettings();

	[Header("External Input")]
	[Range(0,1)]
	public float externalThrottle;
	[Range(0,1)]
	public float externalBrake;
	[Range(-1,1)]
	public float externalSteer;
	public bool externalLiftAndCoast;


	// Private fields

	Perrinn424InputUser m_input;
	ForceFeedbackModelV1 m_ffbV1;
	ForceFeedbackModelV2 m_ffbV2;

	InputDevice.ForceFeedback m_internalFF = new InputDevice.ForceFeedback();
	bool m_ffEnabled = false;

	Steering.Settings m_steeringSettings;


	// Component implementation


	public override void OnEnableVehicle ()
		{
		m_input = new Perrinn424InputUser(inputUserName);
		InputManager.instance.RegisterUser(m_input);

		// Instantiate and initialize both models. This allows switching among them anytime.

		m_ffbV1 = new ForceFeedbackModelV1();
		m_ffbV1.settings = ffbSettingsV1;
		m_ffbV1.rumbleSettings = ffbRumbleV1;
		m_ffbV1.Initialize(vehicle);

		m_ffbV2 = new ForceFeedbackModelV2();
		m_ffbV2.settings = ffbSettingsV2;
		m_ffbV2.Initialize(vehicle);

		// Read steering settings for the steering range

		m_steeringSettings = vehicle.GetInternalObject(typeof(Steering.Settings)) as Steering.Settings;
		}


	public override void OnDisableVehicle ()
		{
		// Stop force feedback

		InputDevice.ForceFeedback forceFeedback = m_input.steer.GetForceFeedback();
		if (forceFeedback == null)
			forceFeedback = m_internalFF;

		if (m_ffEnabled)
			forceFeedback.StopAllEffects();

		m_ffEnabled = false;

		// Unregister user

		InputManager.instance.UnregisterUser(m_input);
		}


	public override void ApplyVehicleInput ()
		{
		int[] inputData = vehicle.data.Get(Channel.Input);
		int[] raceInputData = vehicle.data.Get(Channel.RaceInput);

		// Throttle, brake

		float throttleInput = Mathf.Clamp01(m_input.throttle.Value() + externalThrottle);
		float brakeInput = Mathf.Clamp01(m_input.brake.Value() + externalBrake);

		inputData[InputData.Throttle] = (int)(throttleInput * 10000);
		inputData[InputData.Brake] = (int)(brakeInput * 10000);

		// Apply steer based in the physical and logical steering wheel ranges

		float steerInput = m_input.steer.Value() + externalSteer;
		float physicalWheelRange = InputManager.instance.settings.physicalWheelRange;

		if (m_steeringSettings != null && m_steeringSettings.steeringWheelRange > 0.0f && physicalWheelRange > 0.0f)
			steerInput = Mathf.Clamp(steerInput / m_steeringSettings.steeringWheelRange * physicalWheelRange, -1.0f, 1.0f);

		inputData[InputData.Steer] = (int)(steerInput * 10000);

		// Gear mode

		if (m_input.gearUp.PressedThisFrame()) inputData[InputData.AutomaticGear]++;
		if (m_input.gearDown.PressedThisFrame()) inputData[InputData.AutomaticGear]--;

		// Lift and coast

		if (m_input.liftAndCoast.PressedThisFrame() || externalLiftAndCoast) inputData[InputData.Retarder] = 1;
		}


	public override void FixedUpdateVehicle ()
		{
		// Calculate and process force feedback.
		// FixedUpdateVehicle happens right after the vehicle simulation step, with all internal values updated.

		InputDevice.ForceFeedback forceFeedback = m_input.steer.GetForceFeedback();
		if (forceFeedback == null)
			forceFeedback = m_internalFF;

		if (m_ffEnabled != forceFeedbackEnabled)
			{
			if (!forceFeedbackEnabled)
				forceFeedback.StopAllEffects();

			m_ffEnabled = forceFeedbackEnabled;
			}

		if (m_ffEnabled)
			{
			if (forceFeedbackModel == ForceFeedbackModel.V1)
				{
				m_ffbV1.rumbleEnabled = rumbleEnabled;
				m_ffbV1.Update(vehicle, forceFeedback);
				}
			else
				{
				m_ffbV2.Update(vehicle, forceFeedback);
				}
			}
		}


	// Expose calculated force feedback values


	public InputDevice.ForceFeedback GetForceFeedback ()
		{
		InputDevice.ForceFeedback forceFeedback = m_input != null? m_input.steer.GetForceFeedback() : null;
		if (forceFeedback == null)
			forceFeedback = m_internalFF;

		return forceFeedback;
		}



	// Telemetry


	public override bool EmitTelemetry ()
		{
		return ffbTelemetry;
		}


	public override void RegisterTelemetry ()
		{
		vehicle.telemetry.Register<Perrinn424ForceFeedback>(this);
		}


	public override void UnregisterTelemetry ()
		{
		vehicle.telemetry.Unregister<Perrinn424ForceFeedback>(this);
		}


	public class Perrinn424ForceFeedback : Telemetry.ChannelGroup
		{
		Perrinn424Input m_input;


		public override int GetChannelCount ()
			{
			return 8;
			}


		public override Telemetry.PollFrequency GetPollFrequency ()
			{
			return Telemetry.PollFrequency.Normal;
			}


		public override void GetChannelInfo (Telemetry.ChannelInfo[] channelInfo, Object instance)
			{
			m_input = instance as Perrinn424Input;

			// Fill-in channel information

			channelInfo[0].SetNameAndSemantic("ForceFeedbackForceRatio", Telemetry.Semantic.SignedRatio);
			channelInfo[1].SetNameAndSemantic("ForceFeedbackDamperRatio", Telemetry.Semantic.Ratio);

			Telemetry.SemanticInfo rackLoadSemantic = new Telemetry.SemanticInfo();
			rackLoadSemantic.SetRangeAndFormat(-10000.0f, 10000.0f, "0", " N", quantization:1000);

			channelInfo[2].SetNameAndSemantic("FFBTrackroadLoadLeft", Telemetry.Semantic.Custom, rackLoadSemantic);
			channelInfo[3].SetNameAndSemantic("FFBTrackroadLoadRight", Telemetry.Semantic.Custom, rackLoadSemantic);
			channelInfo[4].SetNameAndSemantic("FFBSteeringRackLoad", Telemetry.Semantic.Custom, rackLoadSemantic);

			Telemetry.SemanticInfo steeringLoadSemantic = new Telemetry.SemanticInfo();
			steeringLoadSemantic.SetRangeAndFormat(-80.0f, 80.0f, "0.0", " Nm", quantization:10);

			channelInfo[5].SetNameAndSemantic("FFBSteeringColumnLoad", Telemetry.Semantic.Custom, steeringLoadSemantic);
			channelInfo[6].SetNameAndSemantic("FFBSteeringAssistanceLoad", Telemetry.Semantic.Custom, steeringLoadSemantic);
			channelInfo[7].SetNameAndSemantic("FFBSteeringWheelLoad", Telemetry.Semantic.Custom, steeringLoadSemantic);
			}


		public override void PollValues (float[] values, int index, Object instance)
			{
			// Final calculated FFB values

			values[index+0] = float.NaN;
			values[index+1] = float.NaN;

			InputDevice.ForceFeedback forceFeedback = m_input.GetForceFeedback();

			if (forceFeedback != null)
				{
				values[index+0] = forceFeedback.force? forceFeedback.forceMagnitude : 0.0f;
				values[index+1] = forceFeedback.damper? forceFeedback.damperCoefficient : 0.0f;
				}

			// Intermediate values from FFB Model V2

			ForceFeedbackModelV2 modelV2 = m_input.m_ffbV2;

			values[index+2] = modelV2.trackroadLoadLeft;
			values[index+3] = modelV2.trackroadLoadRight;
			values[index+4] = modelV2.steeringRackLoad;
			values[index+5] = modelV2.steeringColumnLoad;
			values[index+6] = modelV2.steeringAssistanceLoad;
			values[index+7] = modelV2.steeringWheelLoad;
			}
		}

	}

}