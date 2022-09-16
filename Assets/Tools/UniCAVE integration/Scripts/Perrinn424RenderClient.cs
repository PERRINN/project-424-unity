
// Client renderer for cluster rendering setups


using UnityEngine;
using Mirror;


namespace Perrinn424
{

public class Perrinn424RenderClient : NetworkBehaviour
	{
	public Perrinn424CarController vehicle;
	public Behaviour cameraController;
	public Transform cave;
	public Transform head;


	public struct RenderState
		{
		public Vector3 vehiclePosition;
		public Quaternion vehicleRotation;

		public Vector3 cavePosition;
		public Quaternion caveRotation;

		public Vector3 headPosition;
		public Quaternion headRotation;
		}


	bool m_firstUpdate = true;
	Transform m_vehicleTransform;
	Transform m_caveTransform;
	Transform m_headTransform;


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


	// LateUpdate executed _after_ the default time to get the latest position from the camera.

	void LateUpdate ()
		{
		if (m_firstUpdate && NetworkManager.DebugInfoLevel >= 2)
			Debug.Log("RenderClient First Update");
		m_firstUpdate = false;

		if (isServer)
			{
			RenderState state = new RenderState()
				{
				vehiclePosition = m_vehicleTransform.position,
				vehicleRotation = m_vehicleTransform.rotation,
				};

			if (m_caveTransform != null)
				{
				state.cavePosition = m_caveTransform.position;
				state.caveRotation = m_caveTransform.rotation;
				}

			if (m_headTransform != null)
				{
				state.headPosition = m_headTransform.position;
				state.headRotation = m_headTransform.rotation;
				}

			RpcSetRenderState(state);
			}
		}


	//------------------------------------------------------------------------------------------------------


	[ClientRpc]
	void RpcSetRenderState (RenderState state)
		{
		m_vehicleTransform.SetPositionAndRotation(state.vehiclePosition, state.vehicleRotation);

		if (m_caveTransform != null)
			m_caveTransform.SetPositionAndRotation(state.cavePosition, state.caveRotation);

		if (m_headTransform != null)
			m_headTransform.SetPositionAndRotation(state.headPosition, state.headRotation);
		}
	}

}