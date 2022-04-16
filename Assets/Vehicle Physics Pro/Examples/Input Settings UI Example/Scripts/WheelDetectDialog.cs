//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

// WheelDetectDialog: detect controls for axis actions


using UnityEngine;
using UnityEngine.UI;
using VehiclePhysics.InputManagement;
using System;
using System.Collections.Generic;


namespace VehiclePhysics.UI
{

public class WheelDetectDialog : InputDetectionDialogBase
	{
	public KeyCode cancelKey = KeyCode.Escape;

	[Header("UI")]
	public Text deviceText;
	public Text controlText;
	public Text minValueText;
	public Text maxValueText;
	[Space(5)]
	public Image wheelRightImage;
	public Image wheelLeftImage;
	[Space(5)]
	public Button okButton;
	public Button cancelButton;
	public Button restartButton;


	InputAxis testAxis = new InputAxis("Test Axis");
	DeviceDefinition m_tmpDevice = new DeviceDefinition();
	ControlDefinition m_tmpControl = new ControlDefinition();

	bool m_firstControlDetected = false;
	bool m_controlDetected = false;
	int m_minValue = 0;
	int m_maxValue = 0;
	int m_direction = 0;


	void OnEnable ()
		{
		UITools.AddListener(okButton, OnAccept);
		UITools.AddListener(cancelButton, OnCancel);
		UITools.AddListener(restartButton, OnRestart);
		OnRestart();
		}


	void OnDisable ()
		{
		UITools.RemoveListener(okButton, OnAccept);
		UITools.RemoveListener(cancelButton, OnCancel);
		UITools.RemoveListener(restartButton, OnRestart);

		// Our temporary binding doesn't count against this as the action
		// is not part of any registered user.

		InputManager.instance.CloseUnusedDevices();

		// Clear it anyways to remove innecessary references

		testAxis.bindings.Clear();
		testAxis.BindingsUpdate();
		}


	void Update ()
		{
		if (InputManager.instance.DetectPressedControl(ref m_tmpControl, ref m_tmpDevice))
			{
			if (testAxis.FindBinding(m_tmpDevice, m_tmpControl) == null)
				{
				switch (m_tmpControl.type)
					{
					case ControlType.Analog:
						{
						testAxis.bindings.Clear();
						InputManager.instance.CreateBinding(testAxis, m_tmpDevice, m_tmpControl);
						InputManager.instance.RefreshActionBindings(testAxis);

						device = m_tmpDevice;
						control = m_tmpControl;
						m_controlDetected = true;
						m_firstControlDetected = false;

						// Initial values

						m_minValue = RawValue();
						m_direction = 0;
						break;
						}

					case ControlType.Dpad:
						{
						// Accept new dpad control if (OR, in order):
						//	- No control currently detected
						//	- Currently detected control is not dpad
						//	- Currently detected control is dpad but from other device
						//	- Currently detected control is dpad in same device, but either is a different dpad or its direction doesn't overlap with current dpad direction

						if (!m_controlDetected || control.type != ControlType.Dpad || !device.Match(m_tmpDevice) || !control.Match(m_tmpControl, compareBothDirs:true))
							{
							testAxis.bindings.Clear();
							InputManager.instance.CreateBinding(testAxis, m_tmpDevice, m_tmpControl);
							InputManager.instance.RefreshActionBindings(testAxis);
							SetBindingValue("sensitivity", 2.0f);
							SetBindingValue("gravity", 3.5f);
							SetBindingValue("snap", false);

							device = m_tmpDevice;
							control = m_tmpControl;
							m_controlDetected = true;
							m_firstControlDetected = false;
							}
						break;
						}

					case ControlType.Binary:
						{
						// Accept new binary control if (OR, in order):
						//	- No control currently detected (may have detected first control already)
						//	- Currently detected control is not binary
						//	- Currently detected control is binary but from other device
						//	- Currently detected control is binary in same device with 2 ids, but the new id doesn't match any of them

						if (!m_controlDetected || control.type != ControlType.Binary || !device.Match(m_tmpDevice)
							|| m_controlDetected && m_tmpControl.id0 != control.id0 && m_tmpControl.id0 != control.id1)
							{
							// Detecting either first control, or second control in different device:
							// -> assume it's the first control for direction RIGHT.

							if (!m_firstControlDetected || !device.Match(m_tmpDevice))
								{
								testAxis.bindings.Clear();
								InputManager.instance.CreateBinding(testAxis, m_tmpDevice, m_tmpControl);
								InputManager.instance.RefreshActionBindings(testAxis);
								SetBindingValue("sensitivity", 2.0f);
								SetBindingValue("gravity", 3.5f);
								SetBindingValue("snap", false);

								device = m_tmpDevice;
								control = m_tmpControl;
								control.ReplaceDeviceId("");
								control.name = $"{control.name},";

								m_controlDetected = false;
								m_firstControlDetected = true;
								}
							else
								{
								// Second control in same device: use as second id in currently detected binary control.
								// Detection completed (binary with 2 id).

								if (device.Match(m_tmpDevice))
									{
									m_tmpControl.ReplaceDeviceId("");
									control.id1 = m_tmpControl.id0;
									control.name = $"{control.name}{m_tmpControl.name}";
									SetBindingControl(control);

									m_firstControlDetected = false;
									m_controlDetected = true;
									}
								}
							}
						break;
						}
					}

				// Show device and control names

				if (m_controlDetected || m_firstControlDetected)
					{
					control.ReplaceDeviceId("");
					string deviceName = device.Match(StandardDeviceProvider.defaultDeviceDefinition)? "Keyboard" : device.name;
					UITools.SetText(deviceText, deviceName);
					UITools.SetText(controlText, control.name);
					}
				}

			// Control detection procedure

			InputManager.instance.TakeControlSnapshot();
			}

		if (m_controlDetected || m_firstControlDetected)
			{
			UpdateBinding();

			// Monitor min / max values in analog controls

			bool isAnalog = control.type == ControlType.Analog;
			if (isAnalog)
				{
				if (m_direction == 0)
					{
					// Detect direction

					float minDiff = 32767 * InputManager.analogPressedThreshold;

					m_maxValue = RawValue();
					if (Mathf.Abs(m_maxValue - m_minValue) > minDiff)
						m_direction = m_maxValue > m_minValue? 1 : -1;
					}
				else
					{
					int rawValue = RawValue();

					if (m_direction > 0)
						{
						if (rawValue > m_maxValue) m_maxValue = rawValue;
						if (rawValue < m_minValue) m_minValue = rawValue;
						SetBindingValue("min", m_minValue);
						SetBindingValue("max", m_maxValue);
						SetBindingValue("invert", false);
						}
					else
						{
						if (rawValue < m_maxValue) m_maxValue = rawValue;
						if (rawValue > m_minValue) m_minValue = rawValue;
						SetBindingValue("min", m_maxValue);
						SetBindingValue("max", m_minValue);
						SetBindingValue("invert", true);
						}

					UITools.SetText(minValueText, m_minValue.ToString());
					UITools.SetText(maxValueText, m_maxValue.ToString());
					}
				}

			UITools.SetVisible(minValueText, isAnalog && m_direction != 0);
			UITools.SetVisible(maxValueText, isAnalog && m_direction != 0);

			// Update image

			if (isAnalog && m_direction == 0)
				{
				UITools.SetImageFill(wheelRightImage, 0.0f);
				UITools.SetImageFill(wheelLeftImage, 0.0f);
				}
			else
				{
				float value = testAxis.Value();
				if (value >= 0.0f)
					{
					UITools.SetImageFill(wheelRightImage, testAxis.Value());
					UITools.SetImageFill(wheelLeftImage, 0.0f);
					}
				else
					{
					UITools.SetImageFill(wheelRightImage, 0.0f);
					UITools.SetImageFill(wheelLeftImage, -testAxis.Value());
					}
				}
			}

		// Detect Esc

		if (Input.GetKeyDown(cancelKey))
			OnCancel();
		}


	// Utility


	int RawValue ()
		{
		if (testAxis.activeBindings.Count > 0)
			return testAxis.activeBindings[0].RawValue();
		else
			return 0;
		}


	void UpdateBinding ()
		{
		if (testAxis.activeBindings.Count > 0)
			testAxis.activeBindings[0].Update();
		}


	void SetBindingValue (string name, float value)
		{
		if (testAxis.activeBindings.Count > 0)
			testAxis.activeBindings[0].SetFloat(name, value);
		}


	void SetBindingValue (string name, int value)
		{
		if (testAxis.activeBindings.Count > 0)
			testAxis.activeBindings[0].SetInt(name, value);
		}


	void SetBindingValue (string name, bool value)
		{
		if (testAxis.activeBindings.Count > 0)
			testAxis.activeBindings[0].SetBool(name, value);
		}


	void SetBindingControl (ControlDefinition control)
		{
		if (testAxis.activeBindings.Count > 0)
			testAxis.activeBindings[0].control = control;
		}


	// Listeners


	void OnRestart ()
		{
		UITools.SetText(deviceText, "Detecting...");
		UITools.SetText(controlText, "");
		UITools.SetImageFill(wheelRightImage, 0.0f);
		UITools.SetImageFill(wheelLeftImage, 0.0f);
		UITools.SetVisible(maxValueText, false);
		UITools.SetVisible(minValueText, false);

		testAxis.bindings.Clear();
		testAxis.BindingsUpdate();
		InputManager.instance.OpenAllDevices();
		InputManager.instance.TakeControlSnapshot();

		m_direction = 0;
		assigned = false;
		m_firstControlDetected = false;
		m_controlDetected = false;
		}


	void OnCancel ()
		{
		this.gameObject.SetActive(false);
		}


	void OnAccept ()
		{
		assigned = m_controlDetected;
		if (testAxis.activeBindings.Count > 0)
			settings = testAxis.activeBindings[0].ReadSettings();

		this.gameObject.SetActive(false);
		}
	}
}
