
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


	public struct VisualState
		{
		// Vehicle pose

		public Vector3 vehiclePosition;
		public Quaternion vehicleRotation;

		// UniCave poses

		public Vector3 cavePosition;
		public Quaternion caveRotation;

		public Vector3 headPosition;
		public Quaternion headRotation;

		// Wheel steering angles

		public float steerAngleLeft;
		public float steerAngleRight;

		// Wheel angular positions

		public float angularPositionFL;
		public float angularPositionFR;
		public float angularPositionRL;
		public float angularPositionRR;
		}


	bool m_firstUpdate;
	bool m_clientStateReceived;
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

		// Monitor states received

		m_clientStateReceived = false;
		}


	// Client Update to apply the visual stuff from the latest state
	//
	// QUESTION: Move to RpcUpdateVisualState? Useful being here? I think not, as we're not syncing angular velocity anymore...

	void Update ()
		{
		if (!isServer && m_clientStateReceived)
			{
			vehicle.frontAxle.leftWheel.steerAngle = m_state.steerAngleLeft;
			vehicle.frontAxle.rightWheel.steerAngle = m_state.steerAngleRight;

			vehicle.frontAxle.leftWheel.angularPosition = m_state.angularPositionFL;
			vehicle.frontAxle.rightWheel.angularPosition = m_state.angularPositionFR;
			vehicle.rearAxle.leftWheel.angularPosition = m_state.angularPositionRL;
			vehicle.rearAxle.rightWheel.angularPosition = m_state.angularPositionRR;

			// Using deltaTime = 0 so the specified angular positions are preserved
			// (not modified with any residual angular velocity).

			vehicle.frontAxle.leftWheel.UpdateVisualWheel(0.0f);
			vehicle.frontAxle.rightWheel.UpdateVisualWheel(0.0f);
			vehicle.rearAxle.leftWheel.UpdateVisualWheel(0.0f);
			vehicle.rearAxle.rightWheel.UpdateVisualWheel(0.0f);
			}
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
			// Vehicle pose

			m_state.vehiclePosition = m_vehicleTransform.position;
			m_state.vehicleRotation = m_vehicleTransform.rotation;

			// UniCave poses

			if (m_caveTransform != null)
				{
				m_state.cavePosition = m_caveTransform.position;
				m_state.caveRotation = m_caveTransform.rotation;
				}

			if (m_headTransform != null)
				{
				m_state.headPosition = m_headTransform.position;
				m_state.headRotation = m_headTransform.rotation;
				}

			// Wheel steering angles and angular positions

			m_state.steerAngleLeft = vehicle.frontAxle.leftWheel.steerAngle;
			m_state.steerAngleRight = vehicle.frontAxle.rightWheel.steerAngle;

			m_state.angularPositionFL = vehicle.frontAxle.leftWheel.angularPosition;
			m_state.angularPositionFR = vehicle.frontAxle.rightWheel.angularPosition;
			m_state.angularPositionRL = vehicle.rearAxle.leftWheel.angularPosition;
			m_state.angularPositionRR = vehicle.rearAxle.rightWheel.angularPosition;

			RpcUpdateVisualState(m_state);
			}
		}


	//------------------------------------------------------------------------------------------------------


	// Apply the render state received from the server.
	// Vehicle controller is disabled here.

	[ClientRpc]
	void RpcUpdateVisualState (VisualState state)
		{
		// Vehicle pose

		m_vehicleTransform.SetPositionAndRotation(state.vehiclePosition, state.vehicleRotation);

		// UniCave poses

		if (m_caveTransform != null)
			m_caveTransform.SetPositionAndRotation(state.cavePosition, state.caveRotation);

		if (m_headTransform != null)
			m_headTransform.SetPositionAndRotation(state.headPosition, state.headRotation);

		// We've received at least one state correctly

		m_clientStateReceived = true;
		}
	}

}