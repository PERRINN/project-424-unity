//==================================================================================================
// (c) Angel Garcia "Edy" - Oviedo, Spain
// http://www.edy.es
//
// Static class providing immediate input from code, regardless of the input method/system used.
// Removes the need of configuring the Input Manager/System just for simple development tests.
//==================================================================================================
//
//	enum UnityAxis		Pre-configured input axes, specific for vehicles, camera and mouse motions.
//	enum UnityKey		Standard key set for keyboards. Replaces Input.KeyCode.
//
// Available axes:
//
//		Steer
//		ThrottleAndBrake
//		Clutch
//		Handbrake
//		GearShift
//		GearMode
//		Sideways
//		Forwards
//		Upwards
//		Horizontal
//		Vertical
//		MouseX
//		MouseY
//		MouseScrollWheel
//
// API:
//
//	float GetAxis (UnityAxis axis)			Returns the current value of the given axis (0..1 or -1..0..1)
//	float GetAxisRaw (UnityAxis axis)		Raw value without processing.
//
//	bool GetButton (UnityAxis axis)			Returns true while the axis is pushed (away from zero).
//	bool GetButtonDown (UnityAxis axis)		True when the axis has been pushed this frame.
//	bool GetButtonUp (UnityAxis axis)		True when the axis has returned to zero this frame.
//
//	bool GetMouseButton (int button)		Returns true while the given mouse button (0, 1, 2) is pressed.
//	bool GetMouseButtonDown (int button)	True when the mouse button has been pressed this frame.
//	bool GetMouseButtonUp (int button)		True when the mouse button has been released this frame.
//
//	bool GetKey (UnityKey key)				Returns true while the key is pressed.
//	bool GetKeyDown (UnityKey key)			True when the key has been pressed this frame.
//	bool GetKeyUp (UnityKey key)			True when the key has been released this frame.
//
//	bool shiftKeyPressed					True while any shift key is pressed.
//	bool ctrlKeyPressed						True while any control key is pressed.
//	bool altKeyPressed						True while any alt key is pressed.
//
//	bool anyKey								Any key or mouse button is pressed.
//	bool anyKeyDown							True when the key or mouse button was pressed this frame.
//	bool anyMouseButton						Any mouse button is pressed
//	bool anyMouseButtonDown					True when the mouse button was pressed this frame.
//
//	Vector2 mousePosition					Current mouse position in screen coordinates
//	Vector2 mousePositionDelta				Mouse movement
//	float mouseScrollDelta					Scroll wheel movement (raw, without sensitivity)
//
// Axis may also be accessed directly:
//
//		UnityInput.steerAxis.value
//		UnityInput.clutchAxis.pressed
//		UnityInput.handbrakeAxis.pressedThisFrame
//		UnityInput.gearShiftAxis.releasedThisFrame
//		...
//
// Axis are pre-configured for vehicles, but the settings may be configured in runtime:
//
//		UnityKey negativeButton
//		UnityKey positiveButton
//		UnityKey altNegativeButton
//		UnityKey altPositiveButton
//		float sensitivity
//		float gravity
//		bool snap
//
// Example:
//
//		UnityInput.steerAxis.sensitivity = 4.0f;
//
// Settings are reset each time the assembly is reloaded. For persistence, any custom settings should be
// applied at OnEnable.
//
// Mouse may also be accessed directly:
//
//		UnityInput.mouse.position
//		UnityInput.mouse.delta
//		UnityInput.mouse.scrollDelta		With sensitivity, same as GetAxis(UnityAxis.MouseScrollWheel)
//		UnityInput.mouse.rawScrollDelta		Without sensitivity, same as UnityInput.mouseScrollDelta
//
// And these settings are available for the mouse:
//
//		float xSensitivity
//		float ySensitivity
//		float scrollSensitivity
//
// Example:
//
//		UnityInput.mouse.scrollSensitivity = 1.0f;		// Default is 0.1f. Applies to GetAxis(UnityAxis.MouseScrollWheel)
//
// LEGACY MODE (Legacy Input Manager only)
//
// Axes may be configured to read the input from the Legacy Input Manager with its settings, instead of
// using the predefined setup of UnityInput. Not available when using the new Input System.
//
// Example:
//
//		UnityInput.steer.legacyMode = true;
//		UnityInput.steer.legacyAxis = "Horizontal";
//
// The legacy mode allows reading the input from joysticks, if configured in the Input Manager.
//
//==================================================================================================


using System;
using UnityEngine;
using UnityEngine.LowLevel;
#if !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif


namespace VersionCompatibility
{

// Axis based on the Input Manager, with specific settings for vehicles.

public enum UnityAxis
	{
	None,

	// Vehicle control
	Steer,
	ThrottleAndBrake,
	Clutch,
	Handbrake,
	GearShift,
	GearMode,

	// Uniform 3D motion (e.g. for cameras)
	Sideways,
	Forwards,
	Upwards,

	// Generic uniform motion
	Horizontal,
	Vertical,

	// Mouse control
	MouseX,
	MouseY,
	MouseScrollWheel,
	}


// Key names based on the Key enum in Packages > Input System (1.14) > Devices > Commands > Keyboard.
// No need to be in sync. This is *our* available key set.
//
// Explict values assigned to the equivalent KeyCodes, so serialized properties upgrade seamlessly.
// https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Input/KeyCode.cs

public enum UnityKey
	{
	None = 0, // KeyCode.None,
	// ---- Printable keys ----
	Space = 32, // KeyCode.Space,
	Enter = 13, // KeyCode.Return,
	Tab = 9, // KeyCode.Tab,
	Backquote = 96, // KeyCode.BackQuote,
	Quote = 39, // KeyCode.Quote,
	Semicolon = 59, // KeyCode.Semicolon,
	Comma = 44, // KeyCode.Comma,
	Period = 46, // KeyCode.Period,
	Slash = 47, // KeyCode.Slash,
	Backslash = 92, // KeyCode.Backslash,
	LeftBracket = 91, // KeyCode.LeftBracket,
	RightBracket = 93, // KeyCode.RightBracket,
	Minus = 45, // KeyCode.Minus,
	Equals = 61, // KeyCode.Equals,
	A = 97, // KeyCode.A,
	B = 98, // KeyCode.B,
	C = 99, // KeyCode.C,
	D = 100, // KeyCode.D,
	E = 101, // KeyCode.E,
	F = 102, // KeyCode.F,
	G = 103, // KeyCode.G,
	H = 104, // KeyCode.H,
	I = 105, // KeyCode.I,
	J = 106, // KeyCode.J,
	K = 107, // KeyCode.K,
	L = 108, // KeyCode.L,
	M = 109, // KeyCode.M,
	N = 110, // KeyCode.N,
	O = 111, // KeyCode.O,
	P = 112, // KeyCode.P,
	Q = 113, // KeyCode.Q,
	R = 114, // KeyCode.R,
	S = 115, // KeyCode.S,
	T = 116, // KeyCode.T,
	U = 117, // KeyCode.U,
	V = 118, // KeyCode.V,
	W = 119, // KeyCode.W,
	X = 120, // KeyCode.X,
	Y = 121, // KeyCode.Y,
	Z = 122, // KeyCode.Z,
	Digit1 = 49, // KeyCode.Alpha1,
	Digit2 = 50, // KeyCode.Alpha2,
	Digit3 = 51, // KeyCode.Alpha3,
	Digit4 = 52, // KeyCode.Alpha4,
	Digit5 = 53, // KeyCode.Alpha5,
	Digit6 = 54, // KeyCode.Alpha6,
	Digit7 = 55, // KeyCode.Alpha7,
	Digit8 = 56, // KeyCode.Alpha8,
	Digit9 = 57, // KeyCode.Alpha9,
	Digit0 = 48, // KeyCode.Alpha0,
	// ---- Non-printable keys ----
	LeftShift = 304, // KeyCode.LeftShift,
	RightShift = 303, // KeyCode.RightShift,
	LeftAlt = 308, // KeyCode.LeftAlt,
	RightAlt = 307, // KeyCode.RightAlt,
	LeftCtrl = 306, // KeyCode.LeftControl,
	RightCtrl = 305, // KeyCode.RightControl,
	LeftMeta = 310, // KeyCode.LeftMeta,
	RightMeta = 309, // KeyCode.RightMeta,
	ContextMenu = 319, // KeyCode.Menu,
	Escape = 27, // KeyCode.Escape,
	LeftArrow = 276, // KeyCode.LeftArrow,
	RightArrow = 275, // KeyCode.RightArrow,
	UpArrow = 273, // KeyCode.UpArrow,
	DownArrow = 274, // KeyCode.DownArrow,
	Backspace = 8, // KeyCode.Backspace,
	PageDown = 281, // KeyCode.PageDown,
	PageUp = 280, // KeyCode.PageUp,
	Home = 278, // KeyCode.Home,
	End = 279, // KeyCode.End,
	Insert = 277, // KeyCode.Insert,
	Delete = 127, // KeyCode.Delete,
	CapsLock = 301, // KeyCode.CapsLock,
	NumLock = 300, // KeyCode.Numlock,
	PrintScreen = 316, // KeyCode.Print,
	ScrollLock = 302, // KeyCode.ScrollLock,
	Pause = 19, // KeyCode.Pause,
	NumpadEnter = 271, // KeyCode.KeypadEnter,
	NumpadDivide = 267, // KeyCode.KeypadDivide,
	NumpadMultiply = 268, // KeyCode.KeypadMultiply,
	NumpadPlus = 270, // KeyCode.KeypadPlus,
	NumpadMinus = 269, // KeyCode.KeypadMinus,
	NumpadPeriod = 266, // KeyCode.KeypadPeriod,
	NumpadEquals = 272, // KeyCode.KeypadEquals,
	Numpad0 = 256, // KeyCode.Keypad0,
	Numpad1 = 257, // KeyCode.Keypad1,
	Numpad2 = 258, // KeyCode.Keypad2,
	Numpad3 = 259, // KeyCode.Keypad3,
	Numpad4 = 260, // KeyCode.Keypad4,
	Numpad5 = 261, // KeyCode.Keypad5,
	Numpad6 = 262, // KeyCode.Keypad6,
	Numpad7 = 263, // KeyCode.Keypad7,
	Numpad8 = 264, // KeyCode.Keypad8,
	Numpad9 = 265, // KeyCode.Keypad9,
	F1 = 282, // KeyCode.F1,
	F2 = 283, // KeyCode.F2,
	F3 = 284, // KeyCode.F3,
	F4 = 285, // KeyCode.F4,
	F5 = 286, // KeyCode.F5,
	F6 = 287, // KeyCode.F6,
	F7 = 288, // KeyCode.F7,
	F8 = 289, // KeyCode.F8,
	F9 = 290, // KeyCode.F9,
	F10 = 291, // KeyCode.F10,
	F11 = 292, // KeyCode.F11,
	F12 = 293, // KeyCode.F12,
	// ---- OEM keys (not defined in KeyCode, but present in InputSystem.Key) ----
	OEM1 = 160, // IntlBackslash in some ISO keyboards
	OEM2 = 161,
	OEM3 = 162,
	OEM4 = 163,
	// ---- Mouse buttons ----
	Mouse0 = 323, // KeyCode.Mouse0,
	Mouse1 = 324, // KeyCode.Mouse1,
	Mouse2 = 325, // KeyCode.Mouse2,
	}


public class UnityAxisControl
	{
	// Settings

	public UnityKey negativeButton = UnityKey.None;
	public UnityKey positiveButton = UnityKey.None;
	public UnityKey altNegativeButton = UnityKey.None;
	public UnityKey altPositiveButton = UnityKey.None;

	public float sensitivity = 1.0f;
	public float gravity = 1.0f;
	public bool snap = false;

	// Legacy mode available on legacy input manager only

	public bool legacyMode = false;
	public string legacyAxis = "";

	// API

	public float value => m_value;
	public float rawValue => m_rawValue;

	public bool pressed => FastAbs(m_value) >= ButtonThreshold;
	public bool pressedThisFrame => FastAbs(m_value) >= ButtonThreshold && FastAbs(m_prevValue) < ButtonThreshold;
	public bool releasedThisFrame => FastAbs(m_value) < ButtonThreshold && FastAbs(m_prevValue) >= ButtonThreshold;

	// Implementation

	const float ButtonThreshold = 0.1f;

	float m_value = 0.0f;
	float m_prevValue = 0.0f;
	float m_rawValue = 0.0f;


	// Call on each Update cycle

	public void Update ()
		{
		#if ENABLE_LEGACY_INPUT_MANAGER
		if (legacyMode && !string.IsNullOrEmpty(legacyAxis))
			{
			m_prevValue = m_value;
			m_value = Input.GetAxis(legacyAxis);
			m_rawValue = Input.GetAxisRaw(legacyAxis);
			return;
			}
		#endif

		bool negativePressed = UnityInput.GetKey(negativeButton) || UnityInput.GetKey(altNegativeButton);
		bool positivePressed = UnityInput.GetKey(positiveButton) || UnityInput.GetKey(altPositiveButton);

		m_prevValue = m_value;
		m_value = GetBidirectionalButton(m_value, positivePressed, negativePressed, sensitivity, gravity, snap);
		m_rawValue = negativePressed == positivePressed? 0.0f : positivePressed? 1.0f : negativePressed? -1.0f : 0.0f;
		}

	// Private utility methods avoiding external dependencies

	static float FastAbs (float value) => value >= 0.0f? value : -value;

	static float GetBidirectionalButton (float currentValue, bool positivePressed, bool negativePressed, float moveRate, float centerRate, bool snap = false)
		{
		float target = 0.0f;
		float rate = centerRate;

		if (positivePressed || negativePressed)
			{
			// Both directions pressed: keep current value

			if (positivePressed && negativePressed)
				return currentValue;

			// Single direction pressed: define target and apply snap if configured

			target = positivePressed? +1.0f : -1.0f;
			if (snap && currentValue != 0.0f && Mathf.Sign(currentValue) != Mathf.Sign(target))
				currentValue = 0.0f;

			// Compute rate: counter-acting applies the sum of both rates until reaching the center

			if (currentValue == 0.0f)
			 	{
				rate = moveRate;
				}
			else
			if (Mathf.Sign(target) == Mathf.Sign(currentValue))
				{
				rate = moveRate;
				}
			else
				{
				rate = centerRate + moveRate;

				// Ensure to hit the center at large rates

				if (rate * Time.deltaTime > FastAbs(currentValue))
					{
					currentValue = 0.0f;
					rate = moveRate;
					}
				}
			}

		return Mathf.MoveTowards(currentValue, target, rate * Time.deltaTime);
		}
	}


public class UnityMouseControl
	{
	// Settings

	// NOTE: position sensitivity (x, y) don't have effect on Legacy Input Manager on Unity < 6.
	// This case uses the sensitivity configured in the Legacy Input Manager.

	public float xSensitivity = 0.1f;
	public float ySensitivity = 0.1f;
	public float scrollSensitivity = 0.1f;

	// API

	public Vector2 position => m_position;
	public Vector2 delta => m_delta;
	public Vector2 rawDelta => m_rawDelta;

	public float scrollDelta => m_scrollDelta;
	public float rawScrollDelta => m_rawScrollDelta;

	public bool moved => m_motionX || m_motionY;
	public bool movedThisFrame => m_motionX && !m_prevMotionX || m_motionY && !m_prevMotionY;
	public bool stoppedThisFrame => !m_motionX && m_prevMotionX || !m_motionY && m_prevMotionY;

	public bool scrollMoved => m_motionScroll;
	public bool scrollMovedThisFrame => m_motionScroll && !m_prevMotionScroll;
	public bool scrollStoppedThisFrame => !m_motionScroll && m_prevMotionScroll;

	public bool mousePresent => m_mousePresent;

	// Implementation

	Vector2 m_position = Vector2.zero;
	Vector2 m_delta = Vector2.zero;
	Vector2 m_rawDelta = Vector2.zero;
	float m_scrollDelta = 0.0f;
	float m_rawScrollDelta = 0.0f;
	bool m_mousePresent = true;

	const float MotionThresholdTime = 0.15f;

	bool m_motionX = false;
	bool m_prevMotionX = false;
	float m_motionXTime = 0.0f;
	bool m_motionY = false;
	bool m_prevMotionY = false;
	float m_motionYTime = 0.0f;
	bool m_motionScroll = false;
	bool m_prevMotionScroll = false;
	float m_motionScrollTime = 0.0f;

	#if ENABLE_LEGACY_INPUT_MANAGER && !UNITY_6000_0_OR_NEWER
	bool m_positionDeltaAvailable = true;
	#endif


	// Call on each Update cycle

	public void Update ()
		{
		// Read mouse values

		#if ENABLE_LEGACY_INPUT_MANAGER
		m_mousePresent = Input.mousePresent;
		if (m_mousePresent)
			{
			m_position = Input.mousePosition;
			m_rawScrollDelta = Input.mouseScrollDelta.y;
			m_scrollDelta = m_rawScrollDelta * scrollSensitivity;

			#if UNITY_6000_0_OR_NEWER
			m_rawDelta = Input.mousePositionDelta;
			m_delta = new Vector2(m_rawDelta.x * xSensitivity, m_rawDelta.y * ySensitivity);
			#else
			if (m_positionDeltaAvailable)
				{
				try
					{
					// Delta comes with the sensitivity from the Input Manager applied (default 0.1).
					m_delta.x = Input.GetAxisRaw("Mouse X");
					m_delta.y = Input.GetAxisRaw("Mouse Y");
					m_rawDelta = m_delta;
					}
				catch (System.Exception)
					{
					m_delta = Vector2.zero;
					m_positionDeltaAvailable = false;
					}
				}
			#endif
			}
		else
			{
			m_position = Vector2.zero;
			m_delta = Vector2.zero;
			m_rawDelta = Vector2.zero;
			m_scrollDelta = 0.0f;
			m_rawScrollDelta = 0.0f;
			}
		#else
		Mouse mouse = Mouse.current;
		m_mousePresent = mouse != null;
		if (m_mousePresent)
			{
			m_position = mouse.position.ReadValue();

			// Delta retuns the pixel movement since last frame.
			// Adjusting by 1/2 gives us roughly the same sensitivity as the legacy system.

			m_rawDelta = mouse.delta.ReadValue() * 0.5f;
			m_delta = new Vector2(m_rawDelta.x * xSensitivity, m_rawDelta.y * ySensitivity);

			// Scroll returns the value configured in the OS, which on Windows is 120. Just take the sign.

			m_rawScrollDelta = mouse.scroll.ReadValue().y;
			if (m_rawScrollDelta != 0.0f)
				m_rawScrollDelta = m_rawScrollDelta > 0.0f? 1.0f : -1.0f;

			m_scrollDelta = m_rawScrollDelta * scrollSensitivity;
			}
		else
			{
			m_position = Vector2.zero;
			m_delta = Vector2.zero;
			m_rawDelta = Vector2.zero;
			m_scrollDelta = 0.0f;
			m_rawScrollDelta = 0.0f;
			}
		#endif

		// Detect motion

		m_prevMotionX = m_motionX;
		m_prevMotionY = m_motionY;
		m_prevMotionScroll = m_motionScroll;

		m_motionX = m_delta.x != 0.0f;
		m_motionY = m_delta.y != 0.0f;
		m_motionScroll = m_scrollDelta != 0.0f;

		if (m_motionX)
			m_motionXTime = Time.unscaledTime;
		else
			m_motionX = (Time.unscaledTime - m_motionXTime) < MotionThresholdTime;

		if (m_motionY)
			m_motionYTime = Time.unscaledTime;
		else
			m_motionY = (Time.unscaledTime - m_motionYTime) < MotionThresholdTime;

		if (m_motionScroll)
			m_motionScrollTime = Time.unscaledTime;
		else
			m_motionScroll = (Time.unscaledTime - m_motionScrollTime) < MotionThresholdTime;
		}
	}


// Static UnityInput class replaces legacy UnityEngine.Input


public static class UnityInput
	{
	// Private state vars updated from the player loop

	static bool m_anyKey = false;
	static bool m_anyKeyDown = false;
	static bool m_anyMouseButton = false;
	static bool m_anyMouseButtonDown = false;

	static bool m_shiftKeyPressed = false;
	static bool m_ctrlKeyPressed = false;
	static bool m_altKeyPressed = false;


	// Public axis

	public static readonly UnityAxisControl steerAxis = new UnityAxisControl()
		{
		negativeButton = UnityKey.A,
		positiveButton = UnityKey.D,
		altNegativeButton = UnityKey.LeftArrow,
		altPositiveButton = UnityKey.RightArrow,
		sensitivity = 2.0f,
		gravity = 3.5f,
		};

	public static readonly UnityAxisControl throttleAndBrakeAxis = new UnityAxisControl()
		{
		negativeButton = UnityKey.S,
		positiveButton = UnityKey.W,
		altNegativeButton = UnityKey.DownArrow,
		altPositiveButton = UnityKey.UpArrow,
		sensitivity = 3.0f,
		gravity = 100.0f,
		snap = true,
		};

	public static readonly UnityAxisControl clutchAxis = new UnityAxisControl()
		{
		positiveButton = UnityKey.LeftShift,
		altPositiveButton = UnityKey.RightShift,
		sensitivity = 5.0f,
		gravity = 2.0f,
		};

	public static readonly UnityAxisControl handbrakeAxis = new UnityAxisControl()
		{
		positiveButton = UnityKey.Space,
		sensitivity = 10.0f,
		gravity = 100.0f,
		};

	public static readonly UnityAxisControl gearShiftAxis = new UnityAxisControl()
		{
		negativeButton = UnityKey.CapsLock,
		positiveButton = UnityKey.Tab,
		sensitivity = 1000.0f,
		gravity = 1000.0f,
		};

	public static readonly UnityAxisControl gearModeAxis = new UnityAxisControl()
		{
		negativeButton = UnityKey.PageUp,
		positiveButton = UnityKey.PageDown,
		sensitivity = 1000.0f,
		gravity = 1000.0f,
		};

	public static readonly UnityAxisControl sidewaysAxis = new UnityAxisControl()
		{
		negativeButton = UnityKey.Numpad4,
		positiveButton = UnityKey.Numpad6,
		sensitivity = 100.0f,
		gravity = 100.0f,
		};

	public static readonly UnityAxisControl forwardsAxis = new UnityAxisControl()
		{
		negativeButton = UnityKey.Numpad2,
		positiveButton = UnityKey.Numpad8,
		altNegativeButton = UnityKey.Numpad5,
		sensitivity = 100.0f,
		gravity = 100.0f,
		};

	public static readonly UnityAxisControl upwardsAxis = new UnityAxisControl()
		{
		negativeButton = UnityKey.Numpad3,
		positiveButton = UnityKey.Numpad9,
		altNegativeButton = UnityKey.Numpad7,
		sensitivity = 100.0f,
		gravity = 100.0f,
		};

	public static readonly UnityAxisControl horizontalAxis = new UnityAxisControl()
		{
		negativeButton = UnityKey.A,
		positiveButton = UnityKey.D,
		altNegativeButton = UnityKey.LeftArrow,
		altPositiveButton = UnityKey.RightArrow,
		sensitivity = 3.0f,
		gravity = 5.0f,
		};

	public static readonly UnityAxisControl verticalAxis = new UnityAxisControl()
		{
		negativeButton = UnityKey.S,
		positiveButton = UnityKey.W,
		altNegativeButton = UnityKey.DownArrow,
		altPositiveButton = UnityKey.UpArrow,
		sensitivity = 3.0f,
		gravity = 5.0f,
		};

	public static readonly UnityMouseControl mouse = new UnityMouseControl()
		{
		xSensitivity = 0.1f,
		ySensitivity = 0.1f,
		scrollSensitivity = 0.1f,
		};


	// Public API

	public static bool anyKey => m_anyKey;
	public static bool anyKeyDown => m_anyKeyDown;
	public static bool anyMouseButton => m_anyMouseButton;
	public static bool anyMouseButtonDown => m_anyMouseButtonDown;

	public static bool shiftKeyPressed => m_shiftKeyPressed;
	public static bool ctrlKeyPressed => m_ctrlKeyPressed;
	public static bool altKeyPressed => m_altKeyPressed;

	public static bool mousePresent => mouse.mousePresent;
	public static Vector2 mousePosition => mouse.position;
	public static Vector2 mousePositionDelta => mouse.delta;
	public static float mouseScrollDelta => mouse.rawScrollDelta;


	public static float GetAxis (UnityAxis axis)
		{
		switch (axis)
			{
			case UnityAxis.None: return 0.0f;
			case UnityAxis.Steer: return steerAxis.value;
			case UnityAxis.ThrottleAndBrake: return throttleAndBrakeAxis.value;
			case UnityAxis.Clutch: return clutchAxis.value;
			case UnityAxis.Handbrake: return handbrakeAxis.value;
			case UnityAxis.GearShift: return gearShiftAxis.value;
			case UnityAxis.GearMode: return gearModeAxis.value;
			case UnityAxis.Sideways: return sidewaysAxis.value;
			case UnityAxis.Forwards: return forwardsAxis.value;
			case UnityAxis.Upwards: return upwardsAxis.value;
			case UnityAxis.Horizontal: return horizontalAxis.value;
			case UnityAxis.Vertical: return verticalAxis.value;
			case UnityAxis.MouseX: return mouse.delta.x;
			case UnityAxis.MouseY: return mouse.delta.y;
			case UnityAxis.MouseScrollWheel: return mouse.scrollDelta;
			}

		return 0.0f;
		}

	public static float GetAxisRaw (UnityAxis axis)
		{
		switch (axis)
			{
			case UnityAxis.None: return 0.0f;
			case UnityAxis.Steer: return steerAxis.rawValue;
			case UnityAxis.ThrottleAndBrake: return throttleAndBrakeAxis.rawValue;
			case UnityAxis.Clutch: return clutchAxis.rawValue;
			case UnityAxis.Handbrake: return handbrakeAxis.rawValue;
			case UnityAxis.GearShift: return gearShiftAxis.rawValue;
			case UnityAxis.GearMode: return gearModeAxis.rawValue;
			case UnityAxis.Sideways: return sidewaysAxis.rawValue;
			case UnityAxis.Forwards: return forwardsAxis.rawValue;
			case UnityAxis.Upwards: return upwardsAxis.rawValue;
			case UnityAxis.Horizontal: return horizontalAxis.rawValue;
			case UnityAxis.Vertical: return verticalAxis.rawValue;
			case UnityAxis.MouseX: return mouse.rawDelta.x;
			case UnityAxis.MouseY: return mouse.rawDelta.y;
			case UnityAxis.MouseScrollWheel: return mouse.rawScrollDelta;
			}

		return 0.0f;
		}

	public static bool GetButton (UnityAxis axis)
		{
		switch (axis)
			{
			case UnityAxis.None: return false;
			case UnityAxis.Steer: return steerAxis.pressed;
			case UnityAxis.ThrottleAndBrake: return throttleAndBrakeAxis.pressed;
			case UnityAxis.Clutch: return clutchAxis.pressed;
			case UnityAxis.Handbrake: return handbrakeAxis.pressed;
			case UnityAxis.GearShift: return gearShiftAxis.pressed;
			case UnityAxis.GearMode: return gearModeAxis.pressed;
			case UnityAxis.Sideways: return sidewaysAxis.pressed;
			case UnityAxis.Forwards: return forwardsAxis.pressed;
			case UnityAxis.Upwards: return upwardsAxis.pressed;
			case UnityAxis.Horizontal: return horizontalAxis.pressed;
			case UnityAxis.Vertical: return verticalAxis.pressed;
			case UnityAxis.MouseX: return mouse.moved;
			case UnityAxis.MouseY: return mouse.moved;
			case UnityAxis.MouseScrollWheel: return mouse.scrollMoved;
			}

		return false;
		}

	public static bool GetButtonDown (UnityAxis axis)
		{
		switch (axis)
			{
			case UnityAxis.None: return false;
			case UnityAxis.Steer: return steerAxis.pressedThisFrame;
			case UnityAxis.ThrottleAndBrake: return throttleAndBrakeAxis.pressedThisFrame;
			case UnityAxis.Clutch: return clutchAxis.pressedThisFrame;
			case UnityAxis.Handbrake: return handbrakeAxis.pressedThisFrame;
			case UnityAxis.GearShift: return gearShiftAxis.pressedThisFrame;
			case UnityAxis.GearMode: return gearModeAxis.pressedThisFrame;
			case UnityAxis.Sideways: return sidewaysAxis.pressedThisFrame;
			case UnityAxis.Forwards: return forwardsAxis.pressedThisFrame;
			case UnityAxis.Upwards: return upwardsAxis.pressedThisFrame;
			case UnityAxis.Horizontal: return horizontalAxis.pressedThisFrame;
			case UnityAxis.Vertical: return verticalAxis.pressedThisFrame;
			case UnityAxis.MouseX: return mouse.movedThisFrame;
			case UnityAxis.MouseY: return mouse.movedThisFrame;
			case UnityAxis.MouseScrollWheel: return mouse.scrollMovedThisFrame;
			}

		return false;
		}

	public static bool GetButtonUp (UnityAxis axis)
		{
		switch (axis)
			{
			case UnityAxis.None: return false;
			case UnityAxis.Steer: return steerAxis.releasedThisFrame;
			case UnityAxis.ThrottleAndBrake: return throttleAndBrakeAxis.releasedThisFrame;
			case UnityAxis.Clutch: return clutchAxis.releasedThisFrame;
			case UnityAxis.Handbrake: return handbrakeAxis.releasedThisFrame;
			case UnityAxis.GearShift: return gearShiftAxis.releasedThisFrame;
			case UnityAxis.GearMode: return gearModeAxis.releasedThisFrame;
			case UnityAxis.Sideways: return sidewaysAxis.releasedThisFrame;
			case UnityAxis.Forwards: return forwardsAxis.releasedThisFrame;
			case UnityAxis.Upwards: return upwardsAxis.releasedThisFrame;
			case UnityAxis.Horizontal: return horizontalAxis.releasedThisFrame;
			case UnityAxis.Vertical: return verticalAxis.releasedThisFrame;
			case UnityAxis.MouseX: return mouse.stoppedThisFrame;
			case UnityAxis.MouseY: return mouse.stoppedThisFrame;
			case UnityAxis.MouseScrollWheel: return mouse.scrollStoppedThisFrame;
			}

		return false;
		}

	public static bool GetMouseButton (int button)
		{
		switch (button)
			{
			case 0: return GetKey(UnityKey.Mouse0);
			case 1: return GetKey(UnityKey.Mouse1);
			case 2: return GetKey(UnityKey.Mouse2);
			}

		return false;
		}

	public static bool GetMouseButtonDown (int button)
		{
		switch (button)
			{
			case 0: return GetKeyDown(UnityKey.Mouse0);
			case 1: return GetKeyDown(UnityKey.Mouse1);
			case 2: return GetKeyDown(UnityKey.Mouse2);
			}

		return false;
		}

	public static bool GetMouseButtonUp (int button)
		{
		switch (button)
			{
			case 0: return GetKeyUp(UnityKey.Mouse0);
			case 1: return GetKeyUp(UnityKey.Mouse1);
			case 2: return GetKeyUp(UnityKey.Mouse2);
			}

		return false;
		}


	// Debug information (slow)

	public static string DebugString ()
		{
		string debugString = $"\nMousePos:  {mousePosition}\nDelta:   {(mouse.moved? "*" : " ")} {mousePositionDelta}\nScroll:  {(mouse.scrollMoved? "*" : " ")} {mouseScrollDelta.ToString("+0.0;-0.0; 0.0")}  (Axis:{GetAxis(UnityAxis.MouseScrollWheel).ToString("+0.0;-0.0; 0.0")})";

		debugString += $"\nAnyKey:  {(anyKey? "*" : " ")}\nAnyMBtn: {(anyMouseButton? "*" : " ")}";

		debugString += "\nKeys: ";
	    foreach (UnityKey unityKey in System.Enum.GetValues(typeof(UnityKey)))
	    	{
			if (GetKey(unityKey))
				debugString += $"{unityKey} ";
			}

		debugString += "\nAxes: ";
	    foreach (UnityAxis unityAxis in System.Enum.GetValues(typeof(UnityAxis)))
	    	{
			if (GetButton(unityAxis))
				debugString += $"{unityAxis} ({GetAxis(unityAxis):0.00}) ";
			}

		debugString += $"\nGyro: {gyroEnabled} {gyroGravity}";

		#if ENABLE_LEGACY_INPUT_MANAGER
		// Detect legacy/undocumented key codes
		debugString += "\n\nKeyCodes: ";
		for (int key = 0; key < 500; key++)
			{
			if (Input.GetKey((KeyCode)key))
				debugString += $"{(KeyCode)key} ({key})  ";
			}
		#endif

		return debugString;
		}


	#if ENABLE_LEGACY_INPUT_MANAGER

	public static bool GetKey (UnityKey key) => Input.GetKey((KeyCode)key);
	public static bool GetKeyDown (UnityKey key) => Input.GetKeyDown((KeyCode)key);
	public static bool GetKeyUp (UnityKey key) => Input.GetKeyUp((KeyCode)key);

	static void UpdateCollectiveControls ()
		{
		m_anyKey = Input.anyKey;
		m_anyKeyDown = Input.anyKeyDown;
		m_anyMouseButton = Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.Mouse1) || Input.GetKey(KeyCode.Mouse2);
		m_anyMouseButtonDown = Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.Mouse2);
		}

	// Sensors

	public static Vector3 acceleration => Input.acceleration;

	public static bool gyroEnabled
		{
		get => Input.gyro.enabled;
		set => Input.gyro.enabled = value;
		}

	public static Vector3 gyroGravity => Input.gyro.gravity;

	public static bool compensateSensors
		{
		get => Input.compensateSensors;
		set => Input.compensateSensors = value;
		}

	#else

	public static bool GetKey (UnityKey key)
		{
		if (key >= UnityKey.Mouse0)
			{
			if (Mouse.current == null)
				return false;

			switch (key)
				{
				case UnityKey.Mouse0: { return Mouse.current.leftButton.isPressed; }
				case UnityKey.Mouse1: { return Mouse.current.rightButton.isPressed; }
				case UnityKey.Mouse2: { return Mouse.current.middleButton.isPressed; }
				default: return false;
				}
			}
		else
			{
			// Key.None throws an Out Of Range exception in Keyboard.current[]
			Key keyCode = m_keys[(int)key];
			return keyCode != Key.None && Keyboard.current != null? Keyboard.current[keyCode].isPressed : false;
			}
		}

	public static bool GetKeyDown (UnityKey key)
		{
		if (key >= UnityKey.Mouse0)
			{
			if (Mouse.current == null)
				return false;

			switch (key)
				{
				case UnityKey.Mouse0: { return Mouse.current.leftButton.wasPressedThisFrame; }
				case UnityKey.Mouse1: { return Mouse.current.rightButton.wasPressedThisFrame; }
				case UnityKey.Mouse2: { return Mouse.current.middleButton.wasPressedThisFrame; }
				default: return false;
				}
			}
		else
			{
			// Key.None throws an Out Of Range exception in Keyboard.current[]
			Key keyCode = m_keys[(int)key];
			return keyCode != Key.None && Keyboard.current != null? Keyboard.current[keyCode].wasPressedThisFrame : false;
			}
		}

	public static bool GetKeyUp (UnityKey key)
		{
		if (key >= UnityKey.Mouse0)
			{
			if (Mouse.current == null)
				return false;

			switch (key)
				{
				case UnityKey.Mouse0: { return Mouse.current.leftButton.wasReleasedThisFrame; }
				case UnityKey.Mouse1: { return Mouse.current.rightButton.wasReleasedThisFrame; }
				case UnityKey.Mouse2: { return Mouse.current.middleButton.wasReleasedThisFrame; }
				default: return false;
				}
			}
		else
			{
			// Key.None throws an Out Of Range exception in Keyboard.current[]
			Key keyCode = m_keys[(int)key];
			return keyCode != Key.None && Keyboard.current != null? Keyboard.current[keyCode].wasReleasedThisFrame : false;
			}
		}

	static void UpdateCollectiveControls ()
		{
		Mouse mouse = Mouse.current;
		if (mouse != null)
			{
			ButtonControl leftButton = mouse.leftButton;
			ButtonControl rightButton = mouse.rightButton;
			ButtonControl middleButton = mouse.middleButton;

			m_anyMouseButton = leftButton.isPressed || rightButton.isPressed || middleButton.isPressed;
			m_anyMouseButtonDown = leftButton.wasPressedThisFrame || rightButton.wasPressedThisFrame || middleButton.wasPressedThisFrame;
			}
		else
			{
			m_anyMouseButton = false;
			m_anyMouseButtonDown = false;
			}

		// "AnyKey" includes any mouse button, as they're included in the UnityKey list

		Keyboard keyboard = Keyboard.current;
		if (keyboard != null)
			{
			ButtonControl anyKey = Keyboard.current.anyKey;
			m_anyKey = m_anyMouseButton || anyKey.isPressed;
			m_anyKeyDown = m_anyMouseButtonDown || anyKey.wasPressedThisFrame;
			}
		else
			{
			m_anyKey = m_anyMouseButton;
			m_anyKeyDown = m_anyMouseButtonDown;
			}
		}

	static Key[] m_keys = CreateKeyArrayFromUnityKey();
	static Key[] CreateKeyArrayFromUnityKey()
		{
	    // Calculate array size

	    int maxIndex = 0;
	    foreach (UnityKey key in Enum.GetValues(typeof(UnityKey)))
	    	{
	        int val = (int)key;
	        if (val > maxIndex) maxIndex = val;
	    	}

	    // Create array and, for each UnityKey value, assign the equivalent Key value (by name)

	    Key[] result = new Key[maxIndex + 1];

	    foreach (UnityKey unityKey in Enum.GetValues(typeof(UnityKey)))
	    	{
	        string name = unityKey.ToString();
	        int index = (int)unityKey;

	        if (Enum.TryParse<Key>(name, out Key key))
	            result[index] = key;
	        else
			if (unityKey < UnityKey.Mouse0)
	            Debug.LogWarning($"[UnityInput] Couldn't find a Key value with name '{name}' (UnityKey.{name})");
	    	}

	    return result;
		}

	// Sensors

	public static Vector3 acceleration
		{
		get
			{
			Accelerometer accelerometer = Accelerometer.current;
			if (accelerometer == null)
				return Vector3.zero;

			if (!accelerometer.enabled)
				InputSystem.EnableDevice(accelerometer);

			return accelerometer.acceleration.ReadValue();
			}
		}

	public static bool gyroEnabled
		{
		get => GravitySensor.current != null && GravitySensor.current.enabled;

		set
			{
			if (GravitySensor.current != null)
				{
				if (value)
					InputSystem.EnableDevice(GravitySensor.current);
				else
					InputSystem.DisableDevice(GravitySensor.current);
				}
			}
		}

	public static Vector3 gyroGravity
		{
		get
			{
			GravitySensor gravitySensor = GravitySensor.current;
			if (gravitySensor == null)
				return Vector3.zero;

			return gravitySensor.gravity.ReadValue();
			}
		}

	public static bool compensateSensors
		{
		get => InputSystem.settings.compensateForScreenOrientation;
		set => InputSystem.settings.compensateForScreenOrientation = value;
		}

	#endif


	//------------------------------------------------------------------------------------------------------
	// Low-level: hook into Unity's PlayerLoop so we can update our axis from the input
	// before Update is called.
	//
	// The player loop is re-initialized every domain reload, so we don't need to remove our system from it.


	static UnityInput ()
		{
		// NOTE: The constructor is not called if there's no code referring the UnityInput class.
		InjectBeforeMonoBehaviourUpdate();
		}


	static void InjectBeforeMonoBehaviourUpdate()
		{
		PlayerLoopSystem loop = PlayerLoop.GetCurrentPlayerLoop();

		for (int i = 0; i < loop.subSystemList.Length; i++)
			{
			if (loop.subSystemList[i].type == typeof(UnityEngine.PlayerLoop.Update))
				{
				PlayerLoopSystem[] updateList = loop.subSystemList[i].subSystemList;
				PlayerLoopSystem[] newList = new PlayerLoopSystem[updateList.Length + 1];

				int insertIndex = 0;

				// Find where ScriptRunBehaviourUpdate is

				for (int j = 0; j < updateList.Length; j++)
					{
					if (updateList[j].type == typeof(UnityEngine.PlayerLoop.Update.ScriptRunBehaviourUpdate))
						{
						insertIndex = j;
						break;
						}
					}

				// Define the injected system

				PlayerLoopSystem unityInputSystem = new PlayerLoopSystem
					{
					type = typeof(UnityInput),
					updateDelegate = OnUpdate
					};

				// Insert our system just before ScriptRunBehaviourUpdate

				System.Array.Copy(updateList, 0, newList, 0, insertIndex);
				newList[insertIndex] = unityInputSystem;
				System.Array.Copy(updateList, insertIndex, newList, insertIndex + 1, updateList.Length - insertIndex);

				// Replace update list and set loop

				loop.subSystemList[i].subSystemList = newList;
				PlayerLoop.SetPlayerLoop(loop);
				return;
				}
			}

		Debug.LogError("[UnityInput] Failed to inject into PlayerLoop - UnityInput won't work");
		}


	static void OnUpdate()
		{
		// This runs just before all MonoBehaviour.Update() calls

		horizontalAxis.Update();
		verticalAxis.Update();
		steerAxis.Update();
		throttleAndBrakeAxis.Update();
		clutchAxis.Update();
		handbrakeAxis.Update();
		gearShiftAxis.Update();
		gearModeAxis.Update();
		sidewaysAxis.Update();
		forwardsAxis.Update();
		upwardsAxis.Update();
		mouse.Update();

		UpdateCollectiveControls();

		m_shiftKeyPressed = GetKey(UnityKey.LeftShift) || GetKey(UnityKey.RightShift);
		m_ctrlKeyPressed = GetKey(UnityKey.LeftCtrl) || GetKey(UnityKey.RightCtrl);
		m_altKeyPressed = GetKey(UnityKey.LeftAlt) || GetKey(UnityKey.RightAlt);
		}
	}

}
