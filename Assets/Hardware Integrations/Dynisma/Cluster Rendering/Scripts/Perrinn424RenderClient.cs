
// Client renderer for cluster rendering setups

// TODO: Sync ground effects. NetworkBehaviour in Ground Materials:
//	- Create lists of effect components in the ground materials.
//	- Assign the list indexes to the ID fields in VPGroundParticleEmitter and VPGroundMarksRenderer.
//	- Server:
//		- Subscribes to the events in each VPGroundParticleEmitter and VPGroundMarksRenderer available.
//		- When the event is raised, RPC the client with the ID and the parameters.
//	- Client:
//		- Receives RPC and do the action in the corresponding ID with the parameters received.


using UnityEngine;
using UnityEngine.UI;
using VehiclePhysics;
using VehiclePhysics.Timing;
using Mirror;
using Perrinn424.UI;
using VersionCompatibility;


namespace Perrinn424
{

public class Perrinn424RenderClient : NetworkBehaviour
	{
	public Perrinn424CarController vehicle;
	[UnityEngine.Serialization.FormerlySerializedAs("head")]
	public Transform view;
	public CaveLapTimeUI caveLapTimeUI;
	public bool applyServerEyePointData = false;

	[Header("Client Overlay")]
	public GameObject clientOverlay;
	public Text packetsPerSecondText;
	public bool enableOverlayToggleKey = false;
	public UnityKey overlayToggleKey = UnityKey.R;

	[Header("Top Panel")]
	public GameObject clientTopPanel;
	public UnityKey topPanelToggleKey = UnityKey.P;

	[Header("Other")]
	public Transform miniDashboard;

	[Space(5)]
	public Behaviour[] disableOnClients = new Behaviour[0];


	// Convientent structs and methods to gather the visual states and apply them to their networked counterpart


	public struct VisualPose
		{
		public Vector3 position;
		public Quaternion rotation;

		public void SetFrom (Transform transform)
			{
			if (transform != null)
				{
				position = transform.position;
				rotation = transform.rotation;
				}
			}

		public void ApplyTo (Transform transform)
			{
			if (transform != null)
				transform.SetPositionAndRotation(position, rotation);
			}
		}


	public struct WheelPose
		{
		public VisualPose suspension;
		public VisualPose caliper;
		public VisualPose wheel;

		public void SetFrom (WheelColliderBehaviour wheelCol)
			{
			wheelCol.GetVisualTransforms(out Transform wheelTransform, out Transform caliperTransform, out Transform suspensionTransform);

			if (suspensionTransform != null)
				suspension.SetFrom(suspensionTransform);

			if (caliperTransform != null)
				caliper.SetFrom(caliperTransform);

			if (wheelTransform != null)
				wheel.SetFrom(wheelTransform);
			}

		public void ApplyTo (WheelColliderBehaviour wheelCol)
			{
			wheelCol.GetVisualTransforms(out Transform wheelTransform, out Transform caliperTransform, out Transform suspensionTransform);

			if (suspensionTransform != null)
				suspension.ApplyTo(suspensionTransform);

			if (caliperTransform != null)
				caliper.ApplyTo(caliperTransform);

			if (wheelTransform != null)
				wheel.ApplyTo(wheelTransform);
			}
		}


	public struct TextState
		{
		public bool enabled;
		public string text;

		public void SetFrom (Text uiText)
			{
			if (uiText != null)
				{
				text = uiText.text;
				enabled = uiText.enabled;
				}
			}

		public void ApplyTo (Text uiText)
			{
			if (uiText != null)
				{
				uiText.text = text;
				uiText.enabled = enabled;
				}
			}
		}


	public struct ImageState
		{
		public bool enabled;
		public Color color;

		public void SetFrom (Image uiImage)
			{
			if (uiImage != null)
				{
				color = uiImage.color;
				enabled = uiImage.enabled;
				}
			}

		public void ApplyTo (Image uiImage)
			{
			if (uiImage != null)
				{
				uiImage.color = color;
				uiImage.enabled = enabled;
				}
			}
		}


	public struct LapTimeState
		{
		public int timeMs;
 		public int[] sectorMs;

		public void SetZero ()
			{
			timeMs = 0;
			sectorMs = new int[0];
			}

		public void SetFrom (LapTime lapTime)
			{
			timeMs = lapTime.timeMs;
			sectorMs = lapTime.sectorMs;

			// null is not serialized! Causes connection error.
			if (sectorMs == null)
				sectorMs = new int[0];
			}

		public LapTime GetLapTime ()
			{
			LapTime lapTime = new LapTime(sectors: sectorMs.Length);
			lapTime.timeMs = timeMs;
			lapTime.SetSectorsMs(sectorMs);
			return lapTime;
			}
		}


	// Complete visual state sent to clients


	public struct VisualState
		{
		// Vehicle and view poses

		public VisualPose vehicle;
		public VisualPose view;

		// Wheel poses

		public WheelPose wheelFL;
		public WheelPose wheelFR;
		public WheelPose wheelRL;
		public WheelPose wheelRR;

		// Steering wheel pose

		public bool steeringWheelVisible;
		public VisualPose steeringWheel;
		public VisualPose miniDashboard;

		// Dashboard display states

		public TextState speedMps;
		public TextState speedKph;
		public TextState gear;
		public TextState power;
		public TextState minCaption;
		public TextState deltaTime;
		public TextState batterySOC;
		public TextState batteryCapacity;
		public ImageState drs;
		public ImageState autopilot;

		// Dynamic eyepoint

		public Vector3 eyePointPos;
		public Vector3 eyePointRot;

		// Ideal lap time

		public LapTimeState idealLapTime;

		// Client overlay Visibility

		public bool clientOverlayVisible;
		public bool clientTopPanelVisible;
		}


	bool m_firstUpdate;
	int m_packets;
	float m_timer;
	float m_packetsPerSecond;

	Transform m_vehicleTransform;
	Transform m_viewTransform;
	VPVisualEffects m_visualEffects;
	DashboardDisplay m_dashboardDisplay;
	VisualState m_state;
	LapTimer m_lapTimer;


	// Order of execution & flags
	//
	// Server:  OnEnable, OnStartServer, Update                             isServer: true, isClient: false
	// Client:  OnEnable, OnStartClient, Update                             isServer: false, isClient: true
	// Host:    OnEnable, OnStartServer, Update, OnStartClient, Update      isServer: true, isClient: true
	//
	// When the scene is loaded there may be an additional OnEnable just before Mirror disables the component.


	void OnEnable ()
		{
        if (NetworkManager.DebugInfoLevel >= 2) Debug.Log("RenderClient OnEnable");

		if (vehicle == null)
			{
			Debug.LogWarning("Vehicle not configured. Component disabled.");
			enabled = false;
			return;
			}

		syncInterval = 0.016f;

		m_visualEffects = vehicle.GetComponentInChildren<VPVisualEffects>();
		m_dashboardDisplay = vehicle.GetComponentInChildren<DashboardDisplay>();
		m_vehicleTransform = vehicle.cachedTransform;
		m_viewTransform = view != null? view.transform : null;

		// Client overlay starts hidden
		if (clientOverlay != null)
			clientOverlay.SetActive(false);

		m_state = new VisualState();
		m_firstUpdate = false;
		}


	public override void OnStartServer ()
		{
		if (NetworkManager.DebugInfoLevel >= 2)
			Debug.Log($"RenderClient SERVER - IsServer: {isServer} IsClient: {isClient} IsServerOnly: {isServerOnly} IsClientOnly: {isClientOnly}");
		m_firstUpdate = true;

		m_packets = 0;
		m_timer = Time.unscaledTime;
		}


	public override void OnStartClient ()
		{
		if (NetworkManager.DebugInfoLevel >= 2)
			Debug.Log($"RenderClient CLIENT - IsServer: {isServer} IsClient: {isClient} IsServerOnly: {isServerOnly} IsClientOnly: {isClientOnly}");
		m_firstUpdate = true;

		m_packets = 0;
		m_timer = Time.unscaledTime;

		// Host mode. Ignore client initialization.

		if (isServer) return;

		// FixedUpdate at normal rate. Configure physics.

		Time.fixedDeltaTime = 0.02f;
		Physics.autoSyncTransforms = false;

		// Disable vehicle dynamics

		vehicle.cachedRigidbody.isKinematic = true;
		vehicle.cachedRigidbody.interpolation = RigidbodyInterpolation.None;
		vehicle.enabled = false;

		// Disable components in client

		foreach (Behaviour b in disableOnClients)
			{
			if (b != null)
				b.enabled = false;
			}
		}


	void Update ()
		{
		if (enableOverlayToggleKey && UnityInput.GetKeyDown(overlayToggleKey) && clientOverlay != null)
			{
			clientOverlay.SetActive(!clientOverlay.activeSelf);
			}

		if (UnityInput.GetKeyDown(topPanelToggleKey) && clientTopPanel != null)
			{
			clientTopPanel.SetActive(!clientTopPanel.activeSelf);
			}

		// Update packets per second

		if (Time.unscaledTime > m_timer + 1.0f)
			{
			m_packetsPerSecond = m_packets;
			m_packets = 0;
			m_timer = Time.unscaledTime;
			}

		if (packetsPerSecondText != null && packetsPerSecondText.isActiveAndEnabled)
			{
			packetsPerSecondText.text = $"Network: {m_packetsPerSecond:F0} PPS";
			}
		}


	// Server LateUpdate executed _after_ the default time to get the latest position from the camera
	// and the latest visual values.

	void LateUpdate ()
		{
		if (m_firstUpdate && NetworkManager.DebugInfoLevel >= 2)
			Debug.Log("RenderClient First LateUpdate");

		if (m_firstUpdate)
			{
			// FindObjectOfType doesn't work from OnEnable or OnStartServer in NetworkBehaviours.
			// Must be called here.
			m_lapTimer = FindObjectOfType<LapTimer>();
			}

		m_firstUpdate = false;

		if (isServer)
			{
			// Retrieve vehicle and view poses

			m_state.vehicle.SetFrom(m_vehicleTransform);
			m_state.view.SetFrom(m_viewTransform);

			// Retrieve wheel poses

			m_state.wheelFL.SetFrom(vehicle.frontAxle.leftWheel);
			m_state.wheelFR.SetFrom(vehicle.frontAxle.rightWheel);
			m_state.wheelRL.SetFrom(vehicle.rearAxle.leftWheel);
			m_state.wheelRR.SetFrom(vehicle.rearAxle.rightWheel);

			// Retrieve steering wheel and mini dashboard poses

			m_state.steeringWheel.SetFrom(m_visualEffects.steeringWheel);
			if (m_visualEffects.steeringWheel != null)
				m_state.steeringWheelVisible = m_visualEffects.steeringWheel.gameObject.activeSelf;

			if (miniDashboard != null)
				m_state.miniDashboard.SetFrom(miniDashboard);

			// Retrieve dashboard states

			m_state.speedMps.SetFrom(m_dashboardDisplay.speedMps);
			m_state.speedKph.SetFrom(m_dashboardDisplay.speedKph);
			m_state.gear.SetFrom(m_dashboardDisplay.gear);
			m_state.power.SetFrom(m_dashboardDisplay.totalElecPower);
			m_state.minCaption.SetFrom(m_dashboardDisplay.minIndicator);
			m_state.deltaTime.SetFrom(m_dashboardDisplay.timeDifference);
			m_state.batterySOC.SetFrom(m_dashboardDisplay.batterySOC);
			m_state.batteryCapacity.SetFrom(m_dashboardDisplay.batteryCapacity);
			m_state.drs.SetFrom(m_dashboardDisplay.drsImage);
			m_state.autopilot.SetFrom(m_dashboardDisplay.autopilotImage);

			// Retrieve latest eyepoint data

			m_state.eyePointPos = VIOSOCamera.eyePointPos;
			m_state.eyePointRot = VIOSOCamera.eyePointRot;

			// Retrieve ideal lap time

			if (m_lapTimer != null)
				m_state.idealLapTime.SetFrom(m_lapTimer.idealLapTime);
			else
				m_state.idealLapTime.SetZero();

			// Client overlay

			m_state.clientOverlayVisible = clientOverlay != null? clientOverlay.activeSelf : false;
			m_state.clientTopPanelVisible = clientTopPanel != null? clientTopPanel.activeSelf : false;

			// Send state to clients

			RpcUpdateVisualState(m_state);
			m_packets++;
			}
		}


	//------------------------------------------------------------------------------------------------------


	// Apply the render state received from the server.
	// Vehicle controller is disabled here.

	[ClientRpc]
	void RpcUpdateVisualState (VisualState state)
		{
		m_packets++;

		// Apply vehicle and view poses

		state.vehicle.ApplyTo(m_vehicleTransform);
		state.view.ApplyTo(m_viewTransform);

		// Apply wheel poses

		state.wheelFL.ApplyTo(vehicle.frontAxle.leftWheel);
		state.wheelFR.ApplyTo(vehicle.frontAxle.rightWheel);
		state.wheelRL.ApplyTo(vehicle.rearAxle.leftWheel);
		state.wheelRR.ApplyTo(vehicle.rearAxle.rightWheel);

		// Apply steering wheel amd mini dashboard poses

		state.steeringWheel.ApplyTo(m_visualEffects.steeringWheel);
		if (m_visualEffects.steeringWheel != null)
			m_visualEffects.steeringWheel.gameObject.SetActive(state.steeringWheelVisible);

		if (miniDashboard != null)
			state.miniDashboard.ApplyTo(miniDashboard);

		// Apply dashboard states

		state.speedMps.ApplyTo(m_dashboardDisplay.speedMps);
		state.speedKph.ApplyTo(m_dashboardDisplay.speedKph);
		state.gear.ApplyTo(m_dashboardDisplay.gear);
		state.power.ApplyTo(m_dashboardDisplay.totalElecPower);
		state.minCaption.ApplyTo(m_dashboardDisplay.minIndicator);
		state.deltaTime.ApplyTo(m_dashboardDisplay.timeDifference);
		state.batterySOC.ApplyTo(m_dashboardDisplay.batterySOC);
		state.batteryCapacity.ApplyTo(m_dashboardDisplay.batteryCapacity);
		state.drs.ApplyTo(m_dashboardDisplay.drsImage);
		state.autopilot.ApplyTo(m_dashboardDisplay.autopilotImage);

		// Apply dynamic eyepoint

		if (applyServerEyePointData)
			{
			VIOSOCamera.eyePointPos = state.eyePointPos;
			VIOSOCamera.eyePointRot = state.eyePointRot;
			}

		// Apply ideal lap time

		if (caveLapTimeUI != null)
			caveLapTimeUI.SetLapTime(state.idealLapTime.GetLapTime());

		// Apply client elements visibility

		if (clientOverlay != null && clientOverlay.activeSelf != state.clientOverlayVisible)
			clientOverlay.SetActive(state.clientOverlayVisible);

		if (clientTopPanel != null && clientTopPanel.activeSelf != state.clientTopPanelVisible)
			clientTopPanel.SetActive(state.clientTopPanelVisible);
		}
	}

}