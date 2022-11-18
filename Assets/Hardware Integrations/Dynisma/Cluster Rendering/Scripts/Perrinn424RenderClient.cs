
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
using Mirror;


namespace Perrinn424
{

public class Perrinn424RenderClient : NetworkBehaviour
	{
	public Perrinn424CarController vehicle;
	public VPVisualEffects visualEffects;
	public SteeringScreen dashboardDisplay;
	[UnityEngine.Serialization.FormerlySerializedAs("head")]
	public Transform view;

	public Behaviour cameraController;


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
		}


	bool m_firstUpdate;
	Transform m_vehicleTransform;
	Transform m_viewTransform;
	VisualState m_state;


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

		// Disable camera controller

		if (cameraController != null)
			cameraController.enabled = false;
		}


	// Server LateUpdate executed _after_ the default time to get the latest position from the camera
	// and the latest visual values.

	void LateUpdate ()
		{
		if (m_firstUpdate && NetworkManager.DebugInfoLevel >= 2)
			Debug.Log("RenderClient First LateUpdate");
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

			m_state.steeringWheel.SetFrom(visualEffects.steeringWheel);

			// Retrieve dashboard states

			m_state.speedMps.SetFrom(dashboardDisplay.speedMps);
			m_state.speedKph.SetFrom(dashboardDisplay.speedKph);
			m_state.gear.SetFrom(dashboardDisplay.gear);
			m_state.power.SetFrom(dashboardDisplay.totalElecPower);
			m_state.minCaption.SetFrom(dashboardDisplay.minIndicator);
			m_state.deltaTime.SetFrom(dashboardDisplay.timeDifference);
			m_state.batterySOC.SetFrom(dashboardDisplay.batterySOC);
			m_state.batteryCapacity.SetFrom(dashboardDisplay.batteryCapacity);
			m_state.drs.SetFrom(dashboardDisplay.drsImage);
			m_state.autopilot.SetFrom(dashboardDisplay.autopilotImage);

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

		state.steeringWheel.ApplyTo(visualEffects.steeringWheel);

		// Apply dashboard states

		state.speedMps.ApplyTo(dashboardDisplay.speedMps);
		state.speedKph.ApplyTo(dashboardDisplay.speedKph);
		state.gear.ApplyTo(dashboardDisplay.gear);
		state.power.ApplyTo(dashboardDisplay.totalElecPower);
		state.minCaption.ApplyTo(dashboardDisplay.minIndicator);
		state.deltaTime.ApplyTo(dashboardDisplay.timeDifference);
		state.batterySOC.ApplyTo(dashboardDisplay.batterySOC);
		state.batteryCapacity.ApplyTo(dashboardDisplay.batteryCapacity);
		state.drs.ApplyTo(dashboardDisplay.drsImage);
		state.autopilot.ApplyTo(dashboardDisplay.autopilotImage);
		}
	}

}