//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

// InputSetupDialog: show assignable controls and alows configuring them


using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using VehiclePhysics.InputManagement;
using EdyCommonTools;
using Perrinn424;


namespace VehiclePhysics.UI
{

public class InputSetupDialog : MonoBehaviour
	{
	public KeyCode closeKey = KeyCode.Escape;
	public string carInputUser = "PlayerCar";

	[Header("UI")]
	public Transform steeringWheelIcon;
	public Image brakeBar;
	public Image throttleBar;
	public Slider gearSlider;
	public Image drsBar;

	[Space(5)]
	public ButtonLabelGroup steeringSetup;
	public SliderValueGroup steeringRangeSetup;
	public ButtonLabelGroup throttleSetup;
	public ButtonLabelGroup brakeSetup;
	public ButtonLabelGroup gearUpSetup;
	public ButtonLabelGroup gearDownSetup;
	public ButtonLabelGroup drsSetup;
	[Space(5)]
	public Color defaultControlColor = GColor.ParseColorHex("#CCCCCC");
	public Color activeControlColor = GColor.lightGreenA700;
	public Color inactiveControlColor = GColor.ParseColorHex("#888888");

	[Space(5)]
	public Button closeButton;

	[Header("Dialogs")]
	public WheelDetectDialog wheelDetectDialog;
	public PedalDetectDialog pedalDetectDialog;
	public ButtonDetectDialog buttonDetectDialog;


	Perrinn424InputUser m_inputUser;
	GraphicRaycaster m_raycaster;


	void Awake ()
		{
		// Ensure the modal dialogs are hidden even if this one hasn't been enabled

		DisableModalDialogs();
		}


	void OnEnable ()
		{
		// Initialize internal elements

		m_inputUser = new Perrinn424InputUser(carInputUser);
		InputManager.instance.RegisterUser(m_inputUser);
		UITools.SetValue(steeringRangeSetup, InputManager.instance.settings.physicalWheelRange / 10);

		// Initialize UI

		UpdateControlLabels();
		UpdateControlIcons();

		UITools.AddListener(closeButton, OnClose);
		UITools.AddListener(steeringSetup, OnDetectSteering);
		UITools.AddListener(throttleSetup, OnDetectThrottle);
		UITools.AddListener(brakeSetup, OnDetectBrake);
		UITools.AddListener(gearUpSetup, OnDetectGearUp);
		UITools.AddListener(gearDownSetup, OnDetectGearDown);
		UITools.AddListener(drsSetup, OnDetectDrs);

		// Ensure this dialog can be interacted with

		UITools.EnableRaycaster(this.gameObject);
		}


	void OnDisable ()
		{
		// Save customizable mapping to the input manager

		InputMapping inputMapping = new InputMapping();
		inputMapping.FromUser(m_inputUser);
		InputManager.instance.customizableMapping = inputMapping;
		InputManager.instance.ResetAllMappings();

		// Ensure to hide the other dialogs (i.e. if this one is disabled while other is open)

		DisableModalDialogs();

		// Finalize

		InputManager.instance.UnregisterUser(m_inputUser);

		UITools.RemoveListener(closeButton, OnClose);
		UITools.RemoveListener(steeringSetup, OnDetectSteering);
		UITools.RemoveListener(throttleSetup, OnDetectThrottle);
		UITools.RemoveListener(brakeSetup, OnDetectBrake);
		UITools.RemoveListener(gearUpSetup, OnDetectGearUp);
		UITools.RemoveListener(gearDownSetup, OnDetectGearDown);
		UITools.RemoveListener(drsSetup, OnDetectDrs);
		}


	void Update ()
		{
		// Update control icons (steering wheel, pedals, etc)

		UpdateControlIcons();

		// Update control labels (control name, color when active)

		UpdateControlLabels();

		// Apply settings so effects may be observed lively

		int wheelRange = InputManager.instance.settings.physicalWheelRange;
		InputManager.instance.settings.physicalWheelRange = Mathf.RoundToInt(UITools.GetValue(steeringRangeSetup, defaultValue: wheelRange/10) * 10);

		// Detect Esc

		if (Input.GetKeyDown(closeKey)
			&& !UITools.IsEnabled(wheelDetectDialog)
			&& !UITools.IsEnabled(pedalDetectDialog)
			&& !UITools.IsEnabled(buttonDetectDialog))
			OnClose();
		}


	// Updaters


	void UpdateControlIcons ()
		{
		// Update steering wheel

		if (steeringWheelIcon != null)
			{
			float angle = -m_inputUser.steer.Value() * UITools.GetValue(steeringRangeSetup, defaultValue:18.0f) / 2 * 10;
			steeringWheelIcon.rotation = Quaternion.Euler(0, 0, angle);
			}

		// Update pedals

		UITools.SetImageFill(brakeBar, m_inputUser.brake.Value());
		UITools.SetImageFill(throttleBar, m_inputUser.throttle.Value());

		// Update gear shift

		int shiftUp = m_inputUser.gearUp.Pressed()? 1 : 0;
		int shiftDown = m_inputUser.gearDown.Pressed()? -1 : 0;
		UITools.SetValue(gearSlider, shiftUp + shiftDown);

		// Update DRS

		UITools.SetImageFill(drsBar, m_inputUser.drsEnable.Pressed()? 1.0f : 0.0f);
		}


	void UpdateControlLabels ()
		{
		// Update texts

		UpdateActionLabel(steeringSetup, m_inputUser.steer);
		UpdateActionLabel(throttleSetup, m_inputUser.throttle);
		UpdateActionLabel(brakeSetup, m_inputUser.brake);
		UpdateActionLabel(gearUpSetup, m_inputUser.gearUp);
		UpdateActionLabel(gearDownSetup, m_inputUser.gearDown);
		UpdateActionLabel(drsSetup, m_inputUser.drsEnable);

		// Update colors

		HightlightLabel(steeringSetup, m_inputUser.steer);
		HightlightLabel(throttleSetup, m_inputUser.throttle);
		HightlightLabel(brakeSetup, m_inputUser.brake);
		HightlightLabel(gearUpSetup, m_inputUser.gearUp);
		HightlightLabel(gearDownSetup, m_inputUser.gearDown);
		HightlightLabel(drsSetup, m_inputUser.drsEnable);
		}


	void UpdateActionLabel (ButtonLabelGroup group, InputAction action)
		{
		string text;

		if (action == null)
			{
			text = "";
			}
		else
		if (action.bindings.Count == 0)
			{
			text = "None";
			}
		else
			{
			text = action.bindings[0].control.name;
			}

		UITools.SetText(group, text);
		}


	void HightlightLabel (ButtonLabelGroup group, InputAction action)
		{
		if (group == null || group.label == null)
			return;

		if (action.bindings.Count > 0)
			{
			if (action.bindings[0].device != InputManager.nullDevice)
				{
				bool pressed = action.bindings[0].Pressed();
				group.label.color = pressed? activeControlColor : defaultControlColor;
				}
			else
				{
				group.label.color = inactiveControlColor;
				}
			}
		else
			{
			group.label.color = defaultControlColor;
			}
		}


	void DisableModalDialogs ()
		{
		UITools.Disable(wheelDetectDialog);
		UITools.Disable(pedalDetectDialog);
		UITools.Disable(buttonDetectDialog);
		}


	// Listeners


	void OnClose ()
		{
		this.gameObject.SetActive(false);
		}


	void OnDetectSteering ()
		{
		StartCoroutine(DetectControl(wheelDetectDialog, m_inputUser.steer));
		}


	void OnDetectThrottle ()
		{
		StartCoroutine(DetectControl(pedalDetectDialog, m_inputUser.throttle));
		}


	void OnDetectBrake ()
		{
		StartCoroutine(DetectControl(pedalDetectDialog, m_inputUser.brake));
		}


	void OnDetectGearUp ()
		{
		StartCoroutine(DetectControl(buttonDetectDialog, m_inputUser.gearUp));
		}


	void OnDetectGearDown ()
		{
		StartCoroutine(DetectControl(buttonDetectDialog, m_inputUser.gearDown));
		}


	void OnDetectDrs ()
		{
		StartCoroutine(DetectControl(buttonDetectDialog, m_inputUser.drsEnable));
		}


	// Launcher


	IEnumerator DetectControl (InputDetectionDialogBase dialog, InputAction action)
		{
		// Make detect dialog modal

		UITools.DisableRaycaster(this.gameObject);

		// Show detect dialog

		UITools.Enable(dialog);
		yield return new WaitWhile(() => UITools.IsEnabled(dialog));

		// Store configured binding

		if (dialog.assigned)
			{
			InputManager.instance.ClearActionBindings(action);
			InputManager.instance.CreateBinding(action, dialog.device, dialog.control, dialog.settings);
			InputManager.instance.RefreshActionBindings(action);
			}

		// Restore interaction after modal is closed

		UITools.EnableRaycaster(this.gameObject);
		}
	}
}
