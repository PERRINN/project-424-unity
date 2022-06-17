//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

// PedalDetectDialog: detect controls for slider actions


using UnityEngine;
using UnityEngine.UI;
using VehiclePhysics.InputManagement;
using System;
using System.Collections.Generic;


namespace VehiclePhysics.UI
{

public class PedalDetectDialog : InputDetectionDialogBase
	{
	public KeyCode cancelKey = KeyCode.Escape;

	[Header("UI")]
	public Text deviceText;
	public Text controlText;
	public Text minValueText;
	public Text maxValueText;
	[Space(5)]
	public Image pedalImage;
	[Space(5)]
	public Button okButton;
	public Button cancelButton;
	public Button restartButton;


	InputSlider m_testSlider = new InputSlider("Test Slider");
	bool m_detected = false;
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

		m_testSlider.bindings.Clear();
		m_testSlider.BindingsUpdate();
		}


	void Update ()
		{
		if (InputManager.instance.DetectPressedControl(ref control, ref device))
			{
			if (m_testSlider.FindBinding(device, control) == null)
				{
				// Create a binding for the detected control

				m_testSlider.bindings.Clear();
				InputManager.instance.CreateBinding(m_testSlider, device, control);
				InputManager.instance.RefreshActionBindings(m_testSlider);

				// Report control and name

				control.ReplaceDeviceId("");
				string deviceName = device.Match(StandardDeviceProvider.defaultDeviceDefinition)? "Keyboard" : device.name;
				UITools.SetText(deviceText, deviceName);
				UITools.SetText(controlText, control.name);

				// This input is a slider. Set sensitivity/gravity if used with non-analog controls.

				if (control.type != ControlType.Analog)
					{
					SetBindingValue("sensitivity", 3.0f);
					SetBindingValue("gravity", 1000.0f);
					}

				// Initial values

				m_minValue = RawValue();
				m_direction = 0;
				m_detected = true;
				}

			// Control detection procedure

			InputManager.instance.TakeControlSnapshot();
			}

		if (m_detected)
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
						}
					else
						{
						if (rawValue < m_maxValue) m_maxValue = rawValue;
						if (rawValue > m_minValue) m_minValue = rawValue;
						}

					SetBindingValue("min", m_minValue);
					SetBindingValue("max", m_maxValue);

					UITools.SetText(minValueText, m_minValue.ToString());
					UITools.SetText(maxValueText, m_maxValue.ToString());
					}
				}

			UITools.SetVisible(minValueText, isAnalog && m_direction != 0);
			UITools.SetVisible(maxValueText, isAnalog && m_direction != 0);

			// Update image

			if (isAnalog && m_direction == 0)
				UITools.SetImageFill(pedalImage, 0.0f);
			else
				UITools.SetImageFill(pedalImage, m_testSlider.Value());
			}

		// Detect Esc

		if (Input.GetKeyDown(cancelKey))
			OnCancel();
		}


	// Utility


	int RawValue ()
		{
		if (m_testSlider.activeBindings.Count > 0)
			return m_testSlider.activeBindings[0].RawValue();
		else
			return 0;
		}


	void UpdateBinding ()
		{
		if (m_testSlider.activeBindings.Count > 0)
			m_testSlider.activeBindings[0].Update();
		}


	void SetBindingValue (string name, float value)
		{
		if (m_testSlider.activeBindings.Count > 0)
			m_testSlider.activeBindings[0].SetFloat(name, value);
		}


	void SetBindingValue (string name, int value)
		{
		if (m_testSlider.activeBindings.Count > 0)
			m_testSlider.activeBindings[0].SetInt(name, value);
		}


	void SetBindingValue (string name, bool value)
		{
		if (m_testSlider.activeBindings.Count > 0)
			m_testSlider.activeBindings[0].SetBool(name, value);
		}


	// Listeners


	void OnRestart ()
		{
		UITools.SetText(deviceText, "Detecting...");
		UITools.SetText(controlText, "");
		UITools.SetImageFill(pedalImage, 0.0f);
		UITools.SetVisible(maxValueText, false);
		UITools.SetVisible(minValueText, false);

		m_testSlider.bindings.Clear();
		m_testSlider.BindingsUpdate();
		InputManager.instance.OpenAllDevices();
		InputManager.instance.TakeControlSnapshot();

		m_direction = 0;
		assigned = false;
		m_detected = false;
		}


	void OnCancel ()
		{
		this.gameObject.SetActive(false);
		}


	void OnAccept ()
		{
		assigned = m_detected;
		if (m_testSlider.activeBindings.Count > 0)
			settings = m_testSlider.activeBindings[0].ReadSettings();

		this.gameObject.SetActive(false);
		}
	}
}
