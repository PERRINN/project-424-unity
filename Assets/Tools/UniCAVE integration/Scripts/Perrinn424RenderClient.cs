
// Client renderer for cluster rendering setups


using UnityEngine;
using Mirror;


namespace Perrinn424
{

public class Perrinn424RenderClient : NetworkBehaviour
	{
	public Perrinn424CarController vehicle;


	public struct VehicleState
		{
		public Vector3 position;
		public Quaternion rotation;
		}


	bool m_firstUpdate = true;
	Transform m_vehicleTransform;


	// Order of execution & flags
	//
	// Server:	OnEnable, OnStartServer, Update								isServer: true, isClient: false
	// Client:	OnEnable, OnStartClient, Update								isServer: false, isClient: true
	// Host:	OnEnable, OnStartServer, Update, OnStartClient, Update		isServer: true, isClient: true
	//
	// When the scene is loaded there may be an additional OnEnable just before Mirror disables the component.


	void OnEnable ()
		{
		Debug.Log("RenderClient OnEnable");

		if (vehicle == null)
			{
			Debug.LogWarning("Vehicle not configured. Component disabled.");
			enabled = false;
			return;
			}

		m_vehicleTransform = vehicle.cachedTransform;
		}


	public override void OnStartServer ()
		{
		Debug.Log($"RenderClient SERVER - IsServer: {isServer} IsClient: {isClient} IsServerOnly: {isServerOnly} IsClientOnly: {isClientOnly}");
		m_firstUpdate = true;
		}


	public override void OnStartClient ()
		{
		Debug.Log($"RenderClient CLIENT - IsServer: {isServer} IsClient: {isClient} IsServerOnly: {isServerOnly} IsClientOnly: {isClientOnly}");
		m_firstUpdate = true;

		// Host mode. Ignore client initialization.

		if (isServer) return;

		// FixedUpdate at normal rate. Configure physics.

		Time.fixedDeltaTime = 0.02f;
		Physics.autoSyncTransforms = false;

		// Disable vehicle dynamics

		vehicle.cachedRigidbody.isKinematic = true;
		vehicle.enabled = false;
		}


	void Update ()
		{
		if (m_firstUpdate) Debug.Log("RenderClient First Update");
		m_firstUpdate = false;

		if (isServer)
			{
			RpcSetVehicleState(new VehicleState() { position = m_vehicleTransform.position, rotation = m_vehicleTransform.rotation });
			}
		}


	//------------------------------------------------------------------------------------------------------


	[ClientRpc]
	void RpcSetVehicleState (VehicleState state)
		{
		m_vehicleTransform.SetPositionAndRotation(state.position, state.rotation);
		}

	}

}