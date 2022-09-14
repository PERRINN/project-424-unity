
// Client renderer for cluster rendering setups


using UnityEngine;
using Mirror;


namespace Perrinn424
{

public class Perrinn424RenderClient : NetworkBehaviour
	{
	public Perrinn424CarController vehicle;


	public struct CarState
		{
		public Vector3 position;
		public Quaternion rotation;
		}


	bool m_firstUpdate = true;


	// Order of execution & flags
	//
	// Server:	OnEnable, OnStartServer, Update								isServer: true, isClient: false
	// Client:	OnEnable, OnStartClient, Update								isServer: false, isClient: true
	// Host:	OnEnable, OnStartServer, Update, OnStartClient, Update		isServer: true, isClient: true
	//
	// When the scene is loaded there may be an additional OnEnable just before Mirror disables the component.


	void OnEnable ()
		{
		Debug.Log("OnEnable");

		if (vehicle == null)
			{
			Debug.LogWarning("Vehicle not configured. Component disabled.");
			enabled = false;
			return;
			}
		}


	public override void OnStartServer ()
		{
		Debug.Log($"SERVER - IsServer: {isServer} IsClient: {isClient} IsServerOnly: {isServerOnly} IsClientOnly: {isClientOnly}");
		m_firstUpdate = true;
		}


	public override void OnStartClient ()
		{
		Debug.Log($"CLIENT - IsServer: {isServer} IsClient: {isClient} IsServerOnly: {isServerOnly} IsClientOnly: {isClientOnly}");
		m_firstUpdate = true;

		// Host mode. Ignore client initialization

		if (isServer) return;

		// FixedUpdate at normal rate

		Time.fixedDeltaTime = 0.02f;

		// Disable vehicle physics

		vehicle.cachedRigidbody.isKinematic = true;
		vehicle.enabled = false;
		}


	void Update ()
		{
		if (m_firstUpdate) Debug.Log("First Update");
		m_firstUpdate = false;
		}

	}

}