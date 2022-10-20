
// Client renderer for cluster rendering setups


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
	public Transform cave;
	public Transform head;

	public Behaviour cameraController;



	// Convientent structs and methods to gather the visual states
	// and apply them to their networked counterpart.


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
		public string text;
		public bool enabled;

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


	public struct VisualState
		{
		// Vehicle and cave poses

		public VisualPose vehicle;
		public VisualPose cave;
		public VisualPose head;

		// Wheel poses

		public WheelPose wheelFL;
		public WheelPose wheelFR;
		public WheelPose wheelRL;
		public WheelPose wheelRR;

		// Steering wheel pose

		public VisualPose steeringWheel;

		// Dashboard display states

		public TextState speed;
		}


	bool m_firstUpdate;
	Transform m_vehicleTransform;
	Transform m_caveTransform;
	Transform m_headTransform;
	VisualState m_state;


	// Order of execution & flags
	//
	// Server:	OnEnable, OnStartServer, Update								isServer: true, isClient: false
	// Client:	OnEnable, OnStartClient, Update								isServer: false, isClient: true
	// Host:	OnEnable, OnStartServer, Update, OnStartClient, Update		isServer: true, isClient: true
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
		m_caveTransform = cave != null? cave.transform : null;
		m_headTransform = head != null? head.transform : null;

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
			// Retrieve vehicle and cave poses

			m_state.vehicle.SetFrom(m_vehicleTransform);
			m_state.cave.SetFrom(m_caveTransform);
			m_state.head.SetFrom(m_headTransform);

			// Retrieve wheel poses

			m_state.wheelFL.SetFrom(vehicle.frontAxle.leftWheel);
			m_state.wheelFR.SetFrom(vehicle.frontAxle.rightWheel);
			m_state.wheelRL.SetFrom(vehicle.rearAxle.leftWheel);
			m_state.wheelRR.SetFrom(vehicle.rearAxle.rightWheel);

			// Retrieve steering wheel pose

			m_state.steeringWheel.SetFrom(visualEffects.steeringWheel);

			// Retrieve dashboard state

			m_state.speed.SetFrom(dashboardDisplay.speedMps);

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
		// Apply vehicle and cave poses

		state.vehicle.ApplyTo(m_vehicleTransform);
		state.cave.ApplyTo(m_caveTransform);
		state.head.ApplyTo(m_headTransform);

		// Apply wheel poses

		state.wheelFL.ApplyTo(vehicle.frontAxle.leftWheel);
		state.wheelFR.ApplyTo(vehicle.frontAxle.rightWheel);
		state.wheelRL.ApplyTo(vehicle.rearAxle.leftWheel);
		state.wheelRR.ApplyTo(vehicle.rearAxle.rightWheel);

		// Apply steering wheel pose

		state.steeringWheel.ApplyTo(visualEffects.steeringWheel);

		// Apply dashboard state

		state.speed.ApplyTo(dashboardDisplay.speedMps);
		}
	}

}