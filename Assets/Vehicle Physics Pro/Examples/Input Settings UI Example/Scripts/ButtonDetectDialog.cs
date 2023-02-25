//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

// ButtonDetectDialog: detect controls for button actions


using UnityEngine;
using UnityEngine.UI;
using VehiclePhysics.InputManagement;
using System;
using System.Collections.Generic;


namespace VehiclePhysics.UI
{

public class ButtonDetectDialog : InputDetectionDialogBase
	{
	public KeyCode cancelKey = KeyCode.Escape;

	[Header("UI")]
	public Text deviceText;
	public Text controlText;
	[Space(5)]
	public Image buttonImage;
	[Space(5)]
	public Button okButton;
	public Button cancelButton;
	public Button restartButton;


	InputButton m_testButton = new InputButton("Test Button");
	bool m_detected = false;


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

		m_testButton.bindings.Clear();
		m_testButton.BindingsUpdate();
		}


	void Update ()
		{
		if (InputManager.instance.DetectPressedControl(ref control, ref device))
			{
			// TODO Allow analog controls? Require detecting min/max as with pedals.

			if (control.type != ControlType.Analog && m_testButton.FindBinding(device, control) == null)
				{
				// Create a binding for the detected control

				m_testButton.bindings.Clear();
				InputManager.instance.CreateBinding(m_testButton, device, control);
				InputManager.instance.RefreshActionBindings(m_testButton);

				// Report control and name

				control.ReplaceDeviceId("");
				string deviceName = device.Match(StandardDeviceProvider.defaultDeviceDefinition)? "Keyboard" : device.name;
				UITools.SetText(deviceText, deviceName);
				UITools.SetText(controlText, control.name);

				m_detected = true;
				}

			// Control detection procedure

			InputManager.instance.TakeControlSnapshot();
			}

		// Update image

		UITools.SetImageFill(buttonImage, m_testButton.Pressed()? 1.0f : 0.0f);

		// Detect Esc

		if (Input.GetKeyDown(cancelKey))
			OnCancel();
		}


	// Listeners


	void OnRestart ()
		{
		UITools.SetText(deviceText, "Detecting...");
		UITools.SetText(controlText, "");
		UITools.SetImageFill(buttonImage, 0.0f);

		m_testButton.bindings.Clear();
		m_testButton.BindingsUpdate();
		InputManager.instance.OpenAllDevices();
		InputManager.instance.TakeControlSnapshot();

		m_detected = false;
		assigned = false;
		}


	void OnCancel ()
		{
		this.gameObject.SetActive(false);
		}


	void OnAccept ()
		{
		assigned = m_detected;
		if (m_testButton.activeBindings.Count > 0)
			settings = m_testButton.activeBindings[0].ReadSettings();

		this.gameObject.SetActive(false);
		}
	}
}
