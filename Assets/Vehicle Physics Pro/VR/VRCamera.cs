//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2023 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

// NOTE !IMPORTANT: OpenXR package must be 1.10.0 or lower. Newer versions crash Unity
// when exiting Play mode, at least in Unity 2021.3.45 with the HP Reberb G2.


#if UNITY_2020_3_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
#if XR_MANAGER
using UnityEngine.XR.Management;
#endif
using VersionCompatibility;


namespace VehiclePhysics
{

[DisallowMultipleComponent]
public class VRCamera : MonoBehaviour
	{
	public bool autoInitializeVR = true;
	public bool autoEnableVRCamera = false;	// NOTE: May crash Unity if enabled while domain reload
	public bool forceDeviceTrackingMode = true;
	[Space(5)]
	public UnityKey recenterKey = UnityKey.H;
	public bool saveRecenterPose = true;
	[Space(5)]
	public bool debugLog = false;


	bool m_doAutoInitializeVr = false;
	bool m_vrInitialized = false;

	Vector3 m_originalPosition;
	Quaternion m_originalRotation;

	Vector3 m_positionOffset = Vector3.zero;
	Quaternion m_rotationOffset = Quaternion.identity;


	void Awake ()
		{
		m_originalPosition = transform.localPosition;
		m_originalRotation = transform.localRotation;

		// If "Initialize XR on Startup" is enabled in the XR settings, and a device has been successfully
		// initialized, then the XR system will be reported as already intialized here in Awake.

		// NOTE: Trying to initialze VR on mac causes issues. Skipping auto-Initialization.

		#if !UNITY_EDITOR_OSX && !UNITY_STANDALONE_OSX
		if (autoInitializeVR && !IsVRInitialized())
			m_doAutoInitializeVr = true;
		#endif

		if (saveRecenterPose)
			LoadRecenterPose();
		}


	void OnDestroy ()
		{
		if (m_vrInitialized && IsVRInitialized())
			{
			if (debugLog)
				Debug.Log($"{gameObject.name}: shutting down VR...", gameObject);

			StopAndReleaseVR();
			}

		StopTrackingMonitor();

		if (!saveRecenterPose)
			DeleteRecenterPose();
		}


	void Update ()
		{
		if (m_doAutoInitializeVr && IsVRSystemReady())
			{
			// VR must be initialized after Start

			if (!IsVRInitialized())
				{
				if (debugLog)
					Debug.Log($"{gameObject.name}: initializing VR...", gameObject);

				InitializeAndStartVR();
				if (forceDeviceTrackingMode)
					StartTrackingMonitor();

				// Have we actually initialized VR?

				m_vrInitialized = IsVRInitialized();
				}

			m_doAutoInitializeVr = false;
			}

		// Monitor recenter key

		if (UnityInput.GetKeyDown(recenterKey))
			Recenter();
		}


	void OnEnable ()
		{
		if (IsVRInitialized())
			{
			if (autoEnableVRCamera && !IsVRActive())
				{
				if (debugLog)
					Debug.Log($"{gameObject.name}: start VR camera", gameObject);

				StartVR();
				}

			// Start tracking monitor if VR is present but we haven't initialized it

			if (forceDeviceTrackingMode && !m_vrInitialized)
				{
				if (debugLog)
					Debug.Log($"{gameObject.name}: start tracking monitor for externally initialized VR");

				StartTrackingMonitor();
				}
			}

		Application.onBeforeRender += OnBeforeRender;
		}


	void OnDisable ()
		{
		Application.onBeforeRender -= OnBeforeRender;

		transform.localPosition = m_originalPosition;
		transform.localRotation = m_originalRotation;

		// NOTE: If VR has been initialized by the XR Plugin Management, shutting down the app deinitializes VR before our OnDisable.
		// In that case the camera and the tracking monitor won't be stopped here (as IsVRInitialized will return false).
		// Tracking monitor is also stopped in OnDestroy, so if the input subsystems are still hanging around,
		// they will be still detached from the event handlers.

		if (IsVRInitialized())
			{
			if (autoEnableVRCamera && IsVRActive())
				{
				if (debugLog)
					Debug.Log($"{gameObject.name}: stop VR camera", gameObject);

				StopVR();
				}

			if (forceDeviceTrackingMode && !m_vrInitialized)
				{
				if (debugLog)
					Debug.Log($"{gameObject.name}: stop tracking monitor for externally initialized VR");

				StopTrackingMonitor();
				}
			}
		}


	//------------------------------------------------------------------------------------------------------
	// Public component API


	public void Recenter ()
		{
		if (IsVRInitialized() && IsVRActive())
			{
			GetHMDPose(out bool hasPosition, out Vector3 position, out bool hasRotation, out Quaternion rotation);
			if (hasPosition)
				m_positionOffset = -position;
			if (hasRotation)
				m_rotationOffset = ExtractVerticalRotation(Quaternion.Inverse(rotation));

			if (saveRecenterPose)
				SaveRecenterPose();

			if (debugLog)
				Debug.Log($"{gameObject.name}: HMD recentered to offset: Position: {hasPosition}, {m_positionOffset}. Rotation: {hasRotation}, {m_rotationOffset.eulerAngles}", gameObject);
			}
		}


	//------------------------------------------------------------------------------------------------------
	// Event listeners


	void OnBeforeRender ()
		{
		if (IsVRInitialized() && IsVRActive())
			{
			GetHMDPose(out bool hasPosition, out Vector3 position, out bool hasRotation, out Quaternion rotation);
			if (hasPosition)
				transform.localPosition = m_rotationOffset * (position + m_positionOffset);
			if (hasRotation)
				transform.localRotation = m_rotationOffset * rotation;
			}
		}


	void OnTrackingModeChanged (XRInputSubsystem inputSub)
		{
		// Don't let the tracking mode change to anything different than Device, if supported.

		TrackingOriginModeFlags newTrackingMode = inputSub.GetTrackingOriginMode();
		if (forceDeviceTrackingMode
			&& newTrackingMode != TrackingOriginModeFlags.Device
			&& (inputSub.GetSupportedTrackingOriginModes() & (TrackingOriginModeFlags.Device | TrackingOriginModeFlags.Unknown)) != 0)
			{
			if (debugLog)
				Debug.Log($"{gameObject.name}: tracking mode tried to change to {newTrackingMode}. Resetting to Device.", gameObject);

			inputSub.TrySetTrackingOriginMode(TrackingOriginModeFlags.Device);
			}
		}


	//------------------------------------------------------------------------------------------------------
	// Private API


	void StartTrackingMonitor ()
		{
		var inputSubsystems = new List<XRInputSubsystem>();
		SubsystemManager.GetSubsystems<XRInputSubsystem>(inputSubsystems);

		foreach (var inputSub in inputSubsystems)
			{
			inputSub.TrySetTrackingOriginMode(TrackingOriginModeFlags.Device);
			inputSub.trackingOriginUpdated += OnTrackingModeChanged;
			}
		}


	void StopTrackingMonitor ()
		{
		var inputSubsystems = new List<XRInputSubsystem>();
		SubsystemManager.GetSubsystems<XRInputSubsystem>(inputSubsystems);

		foreach (var inputSub in inputSubsystems)
			inputSub.trackingOriginUpdated -= OnTrackingModeChanged;
		}


	[Serializable]
	struct RecenterPose
		{
		public Vector3 position;
		public Quaternion rotation;
		}

	const string m_savedPoseKey = "VRCamera.RecenterPose";


	void SaveRecenterPose ()
		{
		RecenterPose pose = new RecenterPose
			{
			position = m_positionOffset,
			rotation = m_rotationOffset
			};

		PlayerPrefs.SetString(m_savedPoseKey, JsonUtility.ToJson(pose));

		if (debugLog)
			Debug.Log($"{gameObject.name}: Recenter pose saved [{pose.position} {pose.rotation.eulerAngles}]");
		}


	void LoadRecenterPose ()
		{
		if (PlayerPrefs.HasKey(m_savedPoseKey))
			{
			RecenterPose pose = JsonUtility.FromJson<RecenterPose>(PlayerPrefs.GetString(m_savedPoseKey));

			m_positionOffset = pose.position;
			m_rotationOffset = pose.rotation;

			if (debugLog)
				Debug.Log($"{gameObject.name}: Recenter pose loaded [{pose.position} {pose.rotation.eulerAngles}]");
			}
		}


	void DeleteRecenterPose ()
		{
		if (PlayerPrefs.HasKey(m_savedPoseKey))
			{
			PlayerPrefs.DeleteKey(m_savedPoseKey);

			if (debugLog)
				Debug.Log($"{gameObject.name}: Recenter pose information deleted");
			}
		}


	static Quaternion ExtractVerticalRotation (Quaternion rotation)
		{
        Vector3 forward = rotation * Vector3.forward;
        forward.y = 0.0f;
        forward.Normalize();

        return Quaternion.LookRotation(forward, Vector3.up);
		}


	//------------------------------------------------------------------------------------------------------
	// Public static API
	//------------------------------------------------------------------------------------------------------


	public static bool IsVRInitialized ()
		{
		return XRSettings.enabled;
		}


	public static bool IsVRActive ()
		{
		return XRSettings.isDeviceActive;
		}


	public static bool IsVRSystemReady ()
		{
		#if XR_MANAGER
		return XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.Manager != null;
		#else
		return false;
		#endif
		}


	public static void InitializeAndStartVR ()
		{
		#if XR_MANAGER
		XRGeneralSettings.Instance.Manager.InitializeLoaderSync();
		if (XRSettings.enabled)
			XRGeneralSettings.Instance.Manager.StartSubsystems();
		#endif
		}


	public static void StopAndReleaseVR ()
		{
		#if XR_MANAGER
		XRGeneralSettings.Instance.Manager.StopSubsystems();
		XRGeneralSettings.Instance.Manager.DeinitializeLoader();
		#endif
		}


	public static void StartVR ()
		{
		// Start display

		var displaySubsystems = new List<XRDisplaySubsystem>();
		SubsystemManager.GetSubsystems<XRDisplaySubsystem>(displaySubsystems);
		foreach (var displayInstance in displaySubsystems)
			{
			if (!displayInstance.running)
				displayInstance.Start();
			}

		// Start tracking

		var inputSubsystems = new List<XRInputSubsystem>();
		SubsystemManager.GetSubsystems<XRInputSubsystem>(inputSubsystems);
		foreach (var inputInstance in inputSubsystems)
			{
			if (!inputInstance.running)
				inputInstance.Start();
			}
		}


	public static void StopVR ()
		{
		// Stop display

		var displaySubsystems = new List<XRDisplaySubsystem>();
		SubsystemManager.GetSubsystems<XRDisplaySubsystem>(displaySubsystems);
		foreach (var displayInstance in displaySubsystems)
			{
			if (displayInstance.running)
				displayInstance.Stop();
			}

		// Stop tracking

		var inputSubsystems = new List<XRInputSubsystem>();
		SubsystemManager.GetSubsystems<XRInputSubsystem>(inputSubsystems);
		foreach (var inputInstance in inputSubsystems)
			{
			if (inputInstance.running)
				inputInstance.Stop();
			}
		}


	public static bool GetHMDPose (out bool hasPosition, out Vector3 position, out bool hasRotation, out Quaternion rotation)
		{
		hasPosition = false;
		hasRotation = false;
		position = Vector3.zero;
		rotation = Quaternion.identity;

		var trackingNodes = new List<XRNodeState>();
		InputTracking.GetNodeStates(trackingNodes);

		foreach (var node in trackingNodes)
			{
			if (node.nodeType == XRNode.CenterEye)
				{
				hasPosition = node.TryGetPosition(out position);
				hasRotation = node.TryGetRotation(out rotation);
				break;
				}
			}

		return hasPosition || hasRotation;
		}


	public static bool SetHMDTrackingMode (TrackingOriginModeFlags trackingMode = TrackingOriginModeFlags.Device)
		{
		bool result = false;

		var inputSubsystems = new List<XRInputSubsystem>();
		SubsystemManager.GetSubsystems<XRInputSubsystem>(inputSubsystems);
		foreach (var input in inputSubsystems)
			{
			// Check for head-mounted devices in this subsystem.
			// If there are, try setting Device mode and reset center in the subsystem.

			var inputDevices = new List<InputDevice>();
			if (input.TryGetInputDevices(inputDevices))
				{
				foreach (var device in inputDevices)
					{
					if ((device.characteristics & InputDeviceCharacteristics.HeadMounted) != 0
						&& (device.characteristics & InputDeviceCharacteristics.TrackedDevice) != 0)
						{
						result = input.TrySetTrackingOriginMode(trackingMode);
						break;
						}
					}
				}
			}

		return result;
		}


	public static bool RecenterHMD ()
		{
		bool result = false;

		var inputSubsystems = new List<XRInputSubsystem>();
		SubsystemManager.GetSubsystems<XRInputSubsystem>(inputSubsystems);
		foreach (var input in inputSubsystems)
			{
			// Check for head-mounted devices in this subsystem.
			// If there are, try setting Device mode and reset center in the subsystem.

			var inputDevices = new List<InputDevice>();
			if (input.TryGetInputDevices(inputDevices))
				{
				foreach (var device in inputDevices)
					{
					if ((device.characteristics & InputDeviceCharacteristics.HeadMounted) != 0
						&& (device.characteristics & InputDeviceCharacteristics.TrackedDevice) != 0)
						{
						// Based in the CameraOffset method of the XR legacy input helpers package.

						if (input.GetTrackingOriginMode() == TrackingOriginModeFlags.Device
							|| input.TrySetTrackingOriginMode(TrackingOriginModeFlags.Device))
							{
							// TryRecenter doesn't seem to have effect on the Reberb G2.
							// We don't use this.

							result = input.TryRecenter();
							}

						break;
						}
					}
				}
			}

		return result;
		}


	//------------------------------------------------------------------------------------------------------
	// Context menu accessors


	[ContextMenu("Initialize VR")]
	public void InitializeAndStartVRMenu () => InitializeAndStartVR();

	[ContextMenu("Release VR")]
	public void StopAndReleaseVRMenu () => StopAndReleaseVR();

	[ContextMenu("Start VR")]
	public void StartVRMenu () => StartVR();

	[ContextMenu("Stop VR")]
	public void StopVRMenu () => StopVR();

	[ContextMenu("Recenter HMD")]
	public void RecenterHMDMenu () => RecenterHMD();
	}

}
#endif