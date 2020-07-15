//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using EdyCommonTools;


namespace VehiclePhysics.UI
{

#if !VPP_LIMITED

public class UIHandler : MonoBehaviour
	{
	public VehicleBase vehicle;
	public VPCameraController cameraController;
	public VPReplayController replayController;
	public DrivingAidsPanel drivingAidsPanel;
	public bool drivingAidsModal = true;

	[Header("Input")]
	public bool enableInput = true;
	public float throttlePressRate = 3.0f;
	public float throttleReleaseRate = 10.0f;

	public float brakePressRate = 3.0f;
	public float brakeReleaseRate = 10.0f;

	public float steerMoveRate = 2.0f;
	public float steerCenterRate = 3.5f;

	public enum SteeringMode { Touch, Tilt };
	public SteeringMode steeringMode = SteeringMode.Touch;
	[Range(5.0f, 60.0f)]
	public float maxTiltAngle = 30.0f;
	[Range(0.01f, 0.99f)]
	public float steeringNonLinearBias = 0.5f;
	[Range(0.0f, 0.9f)]
	public float steeringNullZone = 0.1f;
	public float accelerometerFilterPass = 7.0f;

	[Header("UI")]
	public GameObject replayModeActive;
	public GameObject replayModeInactive;
    public GameObject modalPanel;
	public UnityEngine.UI.Text drivingAidsText;
	public UnityEngine.UI.Graphic drivingAidsGraphic;

	[Header("Debug")]
	public UnityEngine.UI.Text debugLabel;
	public bool pretendNoReplay = false; 		// Useful for recording videos from replays

	// Private fields

	float m_throttle = 0.0f;
	float m_brake = 0.0f;
	float m_steer = 0.0f;

	int m_cameraClickCount = 0;
	Vector3 m_acceleration;

	VPStandardInput m_input;
	VPSettingsSwitcher m_settings;


	// UI event receivers


	public bool throttlePressed { get; set; }
	public bool brakePressed { get; set; }
	public bool steerLeftPressed { get; set; }
	public bool steerRightPressed { get; set; }


	public void CameraClick ()
		{
		m_cameraClickCount++;

		if (cameraController != null)
			cameraController.NextCameraMode();
		}


	public void ReplayClick ()
		{
		if (replayController != null)
			replayController.ReplayKey();
		}


	public void DrivingAidsClick ()
		{
		if (drivingAidsModal && modalPanel != null)
			{
			if (modalPanel.activeSelf)
				{
				Time.timeScale = 1.0f;
				if (vehicle != null) vehicle.paused = false;
				modalPanel.SetActive(false);
				}
			else
				{
				Time.timeScale = 0.0f;
				if (vehicle != null) vehicle.paused = true;
				modalPanel.SetActive(true);
				}
			}

		if (drivingAidsPanel != null)
			drivingAidsPanel.gameObject.SetActive(!drivingAidsPanel.gameObject.activeSelf);
		}


	public void ExitModalClick ()
		{
		if (modalPanel != null)
			{
			Time.timeScale = 1.0f;
			if (vehicle != null) vehicle.paused = false;
			modalPanel.SetActive(false);
			}

		if (drivingAidsPanel != null)
			drivingAidsPanel.gameObject.SetActive(false);
		}


	// Component methods


	void OnEnable ()
		{
		if (vehicle != null)
			{
			m_input = vehicle.GetComponent<VPStandardInput>();
			m_settings = vehicle.GetComponent<VPSettingsSwitcher>();
			}
		else
			{
			m_input = null;
			m_settings = null;
			}

		if (drivingAidsPanel != null)
			{
			drivingAidsPanel.ConfigureDefaults();
			drivingAidsPanel.gameObject.SetActive(false);
			}

		// Switching steering mode when no gyro is available is not handled properly.
		// It may take a bit to the filter to catch up with the current acceleration.
		//
		// TO-DO: This should be deprecated and use a n-averaged value instead.

		m_acceleration = Input.acceleration;

		// Fallback to regular touch mode if no accelerometer nor gyro are present

		if (!SystemInfo.supportsAccelerometer && !SystemInfo.supportsGyroscope)
			steeringMode = SteeringMode.Touch;
		}


	void FixedUpdate ()
		{
		// Input

		if (enableInput)
			{
			m_throttle = ProcessSingleValue(m_throttle, throttlePressed, throttlePressRate, throttleReleaseRate);
			m_brake = ProcessSingleValue(m_brake, brakePressed, brakePressRate, brakeReleaseRate);
			ProcessSteering();

			if (m_input != null)
				{
				m_input.externalThrottle = m_throttle;
				m_input.externalBrake = m_brake;
				m_input.externalSteer = m_steer;
				}
			}
		}


	void Update ()
		{
		// Debug info

		if (debugLabel != null)
			{
			string debugText = string.Format("Throttle: {0,4:0.00}\nBrake:    {1,4:0.00}\nSteer:    {2,4:0.00}\nCamera:   {3}", m_throttle, m_brake, m_steer, m_cameraClickCount);
			debugLabel.text = debugText;
			}

		// Show / hide elements on replay active or inactive.
		// For example, use for modifying the icon in the replay button

		if (replayController != null && replayController.replay != null)
			{
			bool replayMode = replayController.replay.state == VPReplay.State.Playback
				|| replayController.replay.state == VPReplay.State.Paused;

			if (pretendNoReplay) replayMode = false;

			if (replayModeActive != null) replayModeActive.SetActive(replayMode);
			if (replayModeInactive != null) replayModeInactive.SetActive(!replayMode);
			}

		// Update driving aids label

		VPSettingsSwitcher.SettingsGroup currentGroup = m_settings != null? m_settings.currentGroup : null;

		if (currentGroup != null)
			{
			if (drivingAidsText != null)
				drivingAidsText.text = currentGroup.name;

			if (drivingAidsGraphic != null)
				drivingAidsGraphic.color = currentGroup.uiColor;
			}
		else
			{
			if (drivingAidsText != null)
				drivingAidsText.text = "n/a";

			if (drivingAidsGraphic != null)
				drivingAidsGraphic.color = Color.gray;
			}
		}


	void ProcessSteering ()
		{
		if (steeringMode == SteeringMode.Tilt)
			{
			if (!Input.gyro.enabled) Input.gyro.enabled = true;

			// compensateSensors must be true so the steering is correctly computed no matter
			// the screen orientation (Screen.orientation).

			Input.compensateSensors = true;

			float angle = ComputeTiltAngle(GetAcceleration());
			float steerInput = Mathf.Clamp(angle / maxTiltAngle, -1.0f, +1.0f);

			// Steer: Apply non-linearity based on a non-linear coefficient.
			// TO-DO: Use the same non-linearity algorythm as in XboxController.
			// TO-DO: Inspector shows the curve of non-linearity.

			m_steer = BiasedRatio.BiasedLerpRaw(NullZone(MathUtility.FastAbs(steerInput)), steeringNonLinearBias) * Mathf.Sign(steerInput);
			}
		else
			{
			m_steer = ProcessSymmetricValue(m_steer, steerRightPressed, steerLeftPressed, steerMoveRate, steerCenterRate);
			}
		}


	float NullZone (float x)
		{
		return Mathf.InverseLerp(steeringNullZone, 1.0f, x);
		}


	float ComputeTiltAngle (Vector3 gravityDir)
		{
		// Vector3.right assumes Input.compensateSensors is true.

		return Vector3.Angle(gravityDir, Vector3.ProjectOnPlane(gravityDir, Vector3.right))
			* Mathf.Sign(Vector3.Dot(gravityDir, Vector3.right));
		}


	Vector3 GetAcceleration ()
		{
		if (SystemInfo.supportsGyroscope)
			{
			return Input.gyro.gravity;
			}
		else
		if (SystemInfo.supportsAccelerometer)
			{
			m_acceleration = Vector3.Lerp(m_acceleration, Input.acceleration, accelerometerFilterPass * Time.deltaTime);
			return m_acceleration;
			}
		else
			{
			// Final fallback. At least the steering wheel will stay at the center position.
			// -Vector3.up assumes Input.compensateSensors is true.

			return -Vector3.up;
			}
		}


	// Utility methods


	static float ProcessSingleValue (float value, bool pressed, float pressRate, float releaseRate)
		{
		float rate;
		float target;

		if (pressed)
			{
			rate = pressRate;
			target = 1.0f;
			}
		else
			{
			rate = releaseRate;
			target = 0.0f;
			}

		return Mathf.MoveTowards(value, target, rate * Time.deltaTime);
		}


	static float ProcessSymmetricValue (float value, bool positivePressed, bool negativePressed, float moveRate, float centerRate)
		{
		bool steerPressed = positivePressed || negativePressed;

		float target = 0.0f;
		float rate = 0.0f;

		if (steerPressed)
			{
			// Both directions pressed: keep current value.
			// Otherwise select direction.

			if (positivePressed && negativePressed)
				target = value;
			else
				target = positivePressed? +1.0f : -1.0f;

			// Counter-steering applies the sum of both rates until reaching the center.

			rate = Mathf.Sign(target - value) == Mathf.Sign(value)? moveRate : centerRate + moveRate;
			}
		else
			{
			target = 0.0f;
			rate = centerRate;
			}

		return Mathf.MoveTowards(value, target, rate * Time.deltaTime);
		}
	}

#endif
}