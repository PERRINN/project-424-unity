
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using EdyCommonTools;
using Mirror;
using Mirror.Discovery;


namespace Perrinn424
{

[RequireComponent(typeof(NetworkManager))]
public class NetworkMonitor : MonoBehaviour
	{
	[Header("GameObjects")]
	[Tooltip("These GameObjects will be enabled on servers and hosts. Always disabled on clients.")]
	public GameObject[] serverOnly = new GameObject[0];

	[Tooltip("These GameObjects will be enabled on clients. Always disabled on server and host.")]
	public GameObject[] clientOnly = new GameObject[0];

	[Tooltip("These GameObjects will be enabled on clients while they're not connected to a server (disconnected or connecting). They will be disabled when the client is connected. Always disabled on server and host.")]
	public GameObject[] clientDisconnected = new GameObject[0];

	[Tooltip("These GameObjects will be enabled on clients when they're connecting to a server. They will be disabled when the client is connected. Always disabled on server and host.")]
	public GameObject[] clientConnecting = new GameObject[0];

	[Tooltip("These GameObjects will be enabled on clients once they're connected to a server. They will be disabled while the client is connecting. Always disabled on server and host.")]
	public GameObject[] clientConnected = new GameObject[0];

	[Header("On-screen widget")]
	public bool debugInfo = false;
	public GUITextBox.Settings debugWidget = new GUITextBox.Settings();
	[Tooltip("Shows debug messages in the console whenever roles or states change")]
	public bool debugConsole = false;

	public enum Role { Undefined, Server, Client, Host }
	public enum State { Undefined, Disconnected, Connecting, Connected }

	// Public accessors

	public Role currentRole => m_currentRole;
	public State clientState => m_clientState;
	public bool clientSearching => m_clientSearching;
	public int connectedClients => m_connectedClients;

	public string serverAddress => m_manager.networkAddress;
	public string localAddress => m_localIP;

	// Private fields

	NetworkManager m_manager;
	NetworkDiscovery m_discovery;
	Role m_currentRole = Role.Undefined;
	State m_clientState = State.Undefined;
	string m_localIP = "127.0.0.1";
	bool m_clientSearching = false;
	int m_connectedClients = 0;

	GUITextBox m_textBox = new GUITextBox();
	StringBuilder m_text = new StringBuilder(1024);

	// Trick to assign a default font to the GUI box. Configure the default font in the script settings in the Editor.
	[HideInInspector] public Font defaultFont;


	void OnValidate ()
		{
		// Initialize default widget font
		if (debugWidget.font == null)
			debugWidget.font = defaultFont;
		}


	void OnEnable ()
		{
		m_manager = GetComponent<NetworkManager>();
		m_discovery = GetComponent<NetworkDiscovery>();
		m_textBox.settings = debugWidget;
		m_textBox.header = "Connection status                     \n";

		m_currentRole = Role.Undefined;
		m_clientState = State.Undefined;
		m_clientSearching = false;
		m_localIP = GetLocalIP();

		// Disable all gameobjects

		SetListActive(serverOnly, false);
		SetListActive(clientOnly, false);
		SetListActive(clientConnecting, false);
		SetListActive(clientConnected, false);
		}


	void OnDisable ()
		{
		m_currentRole = Role.Undefined;
		m_clientState = State.Undefined;
		m_clientSearching = false;
		m_connectedClients = 0;
		}


	void Update ()
		{
		// Current network states

		bool isServerActive = NetworkServer.active;
		bool isServerAdvertising = m_discovery != null? m_discovery.serverAdvertising : false;
		int connectedClients = NetworkServer.connections.Count;

		bool isClientSearching = m_discovery != null? m_discovery.clientSearching : false;;
		bool isClientActive = NetworkClient.active;
		bool isClientConnecing = NetworkClient.isConnecting;
		bool isClientConnected = NetworkClient.isConnected;
		bool isClientReady = NetworkClient.ready;

		// Define new role and state

		Role newRole = Role.Undefined;
		State newState = State.Undefined;
		m_clientSearching = isClientSearching;
		m_connectedClients = connectedClients;

		if (isServerActive && isClientActive)
			newRole = Role.Host;
		else
		if (isServerActive)
			newRole = Role.Server;
		else
		if (isClientActive || isClientSearching)
			newRole = Role.Client;

		if (isClientActive || isClientSearching)
			{
			if (isClientReady)
				newState = State.Connected;
			else
			if (isClientSearching || isClientConnecing || isClientConnected)
				newState = State.Connecting;
			else
				newState = State.Disconnected;
			}

		// Enable / disable the corresponding GameObjects
		//
		// GameObjects are enabled or disabled only once each time the state changes.
		// This allows the GameObjects to take further actions. For example, a isClientReady GameObject may
		// show the connection information (client id, transport, networkAddress, etc.) for some seconds
		// and then disable itself to hide the information.

		if (newRole != m_currentRole)
			{
			if (newRole == Role.Server || newRole == Role.Undefined)
				{
				SetListActive(clientDisconnected, false);
				SetListActive(clientConnecting, false);
				SetListActive(clientConnected, false);
				}

			SetListActive(serverOnly, newRole == Role.Server || newRole == Role.Host);
			SetListActive(clientOnly, newRole == Role.Client);

			if (debugConsole)
				Debug.Log($"NetworkMonitor: Role changed: {m_currentRole} -> {newRole}");

			m_currentRole = newRole;
			}

		if (newState != m_clientState)
			{
			SetListActive(clientDisconnected, newState != State.Connected && newState != State.Undefined);
			SetListActive(clientConnecting, newState == State.Connecting);
			SetListActive(clientConnected, newState == State.Connected);

			if (debugConsole)
				Debug.Log($"NetworkMonitor: Client state changed: {m_clientState} -> {newState}");

			m_clientState = newState;
			}

		// On-screen widget

		if (debugInfo)
			{
			m_text.Clear();
			m_text.Append($"ROLE:                {m_currentRole}\n");
			m_text.Append($"CLIENT:              {m_clientState}\n\n");
			m_text.Append($"Server active:       {isServerActive}\n");
			m_text.Append($"Server advertising:  {isServerAdvertising}\n");
			m_text.Append($"Connected clients:   {connectedClients}\n\n");
			m_text.Append($"Client searching:    {isClientSearching}\n");
			m_text.Append($"Client active:       {isClientActive}\n");
			m_text.Append($"Client connecting:   {isClientConnecing}\n");
			m_text.Append($"Client connected:    {isClientConnected}\n");
			m_text.Append($"Client ready:        {isClientReady}\n");
			m_text.Append($"Server address:      {m_manager.networkAddress}\n\n");

			string strLocalPlayer = NetworkClient.localPlayer != null? "yes" : "no";
			m_text.Append($"Local player:        {strLocalPlayer}\n");
			m_text.Append($"Network address:     {m_localIP}\n");
			m_text.Append($"Active transport:    {Transport.activeTransport}");

			m_textBox.text = m_text.ToString();
			}
		}


	void OnGUI ()
		{
		if (debugInfo)
			m_textBox.OnGUI();
		}


	void SetListActive (GameObject[] list, bool active)
		{
		foreach (GameObject go in list)
			{
			if (go != null)
				go.SetActive(active);
			}
		}


	string GetLocalIP ()
		{
		// Source: https://stackoverflow.com/a/27376368/2519774

		string localIP;

		try {
			using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
				{
				socket.Connect("8.8.8.8", 53);
				IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
				localIP = endPoint.Address.ToString();
				}
			}
		catch (Exception)
			{
			localIP = "127.0.0.1";
			}

		return localIP;
		}
	}

}