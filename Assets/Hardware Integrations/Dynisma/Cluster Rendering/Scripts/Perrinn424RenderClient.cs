
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


namespace Perrinn424
{

public class Perrinn424RenderClient : NetworkBehaviour
	{
	public Perrinn424CarController vehicle;
	[UnityEngine.Serialization.FormerlySerializedAs("head")]
	public Transform view;

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

		public void SetFrom (VPWheelCollider wheelCol)
			{
			if (wheelCol.suspensionTransform != null)
				suspension.SetFrom(wheelCol.suspensionTransform);

			if (wheelCol.caliperTransform != null)
				caliper.SetFrom(wheelCol.caliperTransform);

			if (wheelCol.wheelTransform != null)
				wheel.SetFrom(wheelCol.wheelTransform);
			}

		public void ApplyTo (VPWheelCollider wheelCol)
			{
			if (wheelCol.suspensionTransform != null)
				suspension.ApplyTo(wheelCol.suspensionTransform);

			if (wheelCol.caliperTransform != null)
				caliper.ApplyTo(wheelCol.caliperTransform);

			if (wheelCol.wheelTransform != null)
				wheel.ApplyTo(wheelCol.wheelTransform);
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
		public int sector1ms;
		public int sector2ms;
		public int sector3ms;
		public int sector4ms;
		public int sector5ms;

		public int[] sectors;

		public void SetFrom (LapTime lapTime)
			{
			timeMs = lapTime.timeMs;
			sector1ms = lapTime.sectorMs[0];
			sector2ms = lapTime.sectorMs[1];
			sector3ms = lapTime.sectorMs[2];
			sector4ms = lapTime.sectorMs[3];
			sector5ms = lapTime.sectorMs[4];

			sectors = new int[] { sector1ms, sector2ms, sector3ms, sector4ms, sector5ms };
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

		public VisualPose steeringWheel;
		public bool steeringWheelVisible;

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
		}


	bool m_firstUpdate;
	Transform m_vehicleTransform;
	Transform m_viewTransform;
	VPVisualEffects m_visualEffects;
	SteeringScreen m_dashboardDisplay;
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
		m_dashboardDisplay = vehicle.GetComponentInChildren<SteeringScreen>();
		m_vehicleTransform = vehicle.cachedTransform;
		m_viewTransform = view != null? view.transform : null;

		m_state = new VisualState();
		m_firstUpdate = false;
		}


	public override void OnStartServer ()
		{
		if (NetworkManager.DebugInfoLevel >= 2)
			Debug.Log($"RenderClient SERVER - IsServer: {isServer} IsClient: {isClient} IsServerOnly: {isServerOnly} IsClientOnly: {isClientOnly}");
		m_firstUpdate = true;
		}


	public override void OnStartClient ()
		{
		if (NetworkManager.DebugInfoLevel >= 2)
			Debug.Log($"RenderClient CLIENT - IsServer: {isServer} IsClient: {isClient} IsServerOnly: {isServerOnly} IsClientOnly: {isClientOnly}");
		m_firstUpdate = true;

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

			// Retrieve steering wheel pose

			m_state.steeringWheel.SetFrom(m_visualEffects.steeringWheel);
			if (m_visualEffects.steeringWheel != null)
				m_state.steeringWheelVisible = m_visualEffects.steeringWheel.gameObject.activeSelf;

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

			// Send state to clients

			RpcUpdateVisualState(m_state);
			}
		}


	//------------------------------------------------------------------------------------------------------


	// Apply the render state received from the server.
	// Vehicle controller is disabled here.

	[ClientRpc]
	void RpcUpdateVisualState (VisualState state)
		{
		// Apply vehicle and view poses

		state.vehicle.ApplyTo(m_vehicleTransform);
		state.view.ApplyTo(m_viewTransform);

		// Apply wheel poses

		state.wheelFL.ApplyTo(vehicle.frontAxle.leftWheel);
		state.wheelFR.ApplyTo(vehicle.frontAxle.rightWheel);
		state.wheelRL.ApplyTo(vehicle.rearAxle.leftWheel);
		state.wheelRR.ApplyTo(vehicle.rearAxle.rightWheel);

		// Apply steering wheel pose

		state.steeringWheel.ApplyTo(m_visualEffects.steeringWheel);
		if (m_visualEffects.steeringWheel != null)
			m_visualEffects.steeringWheel.gameObject.SetActive(state.steeringWheelVisible);

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

		VIOSOCamera.eyePointPos = state.eyePointPos;
		VIOSOCamera.eyePointRot = state.eyePointRot;

		// Apply ideal lap time

		// LapTime lapTime = new LapTime(sectors: 5);
		if (state.idealLapTime.timeMs > 0)
			{
			Debug.Log($"Got a lap: {state.idealLapTime.timeMs} - {state.idealLapTime.sector1ms} {state.idealLapTime.sector2ms} {state.idealLapTime.sector3ms} {state.idealLapTime.sector4ms} {state.idealLapTime.sector5ms}");
			Debug.Log($"{state.idealLapTime.sectors[0]} {state.idealLapTime.sectors[1]} {state.idealLapTime.sectors[2]} {state.idealLapTime.sectors[3]} {state.idealLapTime.sectors[4]}");
			}
		}
	}

}