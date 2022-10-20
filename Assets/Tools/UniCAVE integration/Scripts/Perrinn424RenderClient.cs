
// Client renderer for cluster rendering setups


using UnityEngine;
using VehiclePhysics;
using Mirror;


namespace Perrinn424
{

public class Perrinn424RenderClient : NetworkBehaviour
	{
	public Perrinn424CarController vehicle;
	public Behaviour cameraController;
	public Transform cave;
	public Transform head;


	// Convientent structs and methods to gather the visual states
	// and apply them to their networked counterpart.


	public struct VisualPose
		{
		public Vector3 position;
		public Quaternion rotation;

		public void SetFrom (Transform transform)
			{
			position = transform.position;
			rotation = transform.rotation;
			}

		public void ApplyTo (Transform transform)
			{
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

			if (m_caveTransform != null)
				m_state.cave.SetFrom(m_caveTransform);

			if (m_headTransform != null)
				m_state.head.SetFrom(m_headTransform);

			// Retrieve wheel poses

			m_state.wheelFL.SetFrom(vehicle.frontAxle.leftWheel);
			m_state.wheelFR.SetFrom(vehicle.frontAxle.rightWheel);
			m_state.wheelRL.SetFrom(vehicle.rearAxle.leftWheel);
			m_state.wheelRR.SetFrom(vehicle.rearAxle.rightWheel);

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

		m_state.vehicle.ApplyTo(m_vehicleTransform);

		if (m_caveTransform != null)
			m_state.cave.ApplyTo(m_caveTransform);

		if (m_headTransform != null)
			m_state.head.ApplyTo(m_headTransform);

		// Apply wheel poses

		m_state.wheelFL.ApplyTo(vehicle.frontAxle.leftWheel);
		m_state.wheelFR.ApplyTo(vehicle.frontAxle.rightWheel);
		m_state.wheelRL.ApplyTo(vehicle.rearAxle.leftWheel);
		m_state.wheelRR.ApplyTo(vehicle.rearAxle.rightWheel);
		}
	}

}