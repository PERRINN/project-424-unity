
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using EdyCommonTools;
using Mirror;


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
	[Tooltip("These GameObjects will be enabled on clients when they're connecting to a server. They will be disabled when the client is connected. Always disabled on server and host.")]
	public GameObject[] clientConnecting = new GameObject[0];
	[Tooltip("These GameObjects will be enabled on clients once they're connected to a server. They will be disabled while the client is connecting. Always disabled on server and host.")]
	public GameObject[] clientConnected = new GameObject[0];
	[Header("On-screen widget")]
	public bool debugInfo = false;
	public GUITextBox.Settings debugWidget = new GUITextBox.Settings();


	public enum Role { Undefined, Server, Client, Host }
	public enum State { Undefined, Connecting, Connected }


	NetworkManager m_manager;
	Role m_currentRole = Role.Undefined;
	State m_currentState = State.Undefined;

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
		m_textBox.settings = debugWidget;
		m_textBox.header = "Connection status              \n";

		m_currentRole = Role.Undefined;
		m_currentState = State.Undefined;

		// Disable all gameobjects

		DisableList(serverOnly);
		DisableList(clientOnly);
		DisableList(clientConnecting);
		DisableList(clientConnected);
		}


	void OnDisable ()
		{
		m_currentRole = Role.Undefined;
		m_currentState = State.Undefined;
		}


	void Update ()
		{
		// TODO:
		// - Define role: undefined, server, client, host.
		// - Monitor role changes.
		// - Enable and/or disable serverOnly and clientOnly just once only when the state changes.

		// TODO:
		// - Define client connection state: undefined (in server), connecting, connected.
		// - Monitor connection state changes.
		// - Enable and/or disable clientConnecting and clientReady just once only when the state changes.

		// Current network states

		bool serverActive = NetworkServer.active;
		bool clientActive = NetworkClient.active;
		bool clientConnected = NetworkClient.isConnected;
		bool clientReady = NetworkClient.ready;

		// Define role

		if (serverActive && clientActive)
			m_currentRole = Role.Host;
		else
		if (serverActive)
			m_currentRole = Role.Server;
		else
		if (clientActive)
			m_currentRole = Role.Client;
		else
			m_currentRole = Role.Undefined;

		// Define state

		if (clientActive)
			{

			}
		else
			{
			m_currentState = State.Undefined;
			}

		// GameObjects are enabled or disabled only once each time the state changes.
		// This allows the GameObjects to take further actions. For example, a clientReady GameObject may
		// show the connection information (client id, transport, networkAddress, etc.) for some seconds
		// and then disable itself to hide the information.

		// TODO: Public event delegates: OnRoleChange, OnClientStateChange


		if (debugInfo)
			{
			m_text.Clear();
			m_text.Append($"Server active:    {serverActive}\n\n");
			m_text.Append($"Client active:    {clientActive}\n");
			m_text.Append($"Client connected: {clientConnected}\n");
			m_text.Append($"Client ready:     {clientReady}\n\n");

			string strLocalPlayer = NetworkClient.localPlayer != null? "yes" : "no";
			m_text.Append($"Local player:     {strLocalPlayer}\n");
			m_text.Append($"Network address:  {m_manager.networkAddress}\n");
			m_text.Append($"Active transport: {Transport.activeTransport}");

			m_textBox.text = m_text.ToString();
			}
		}


	void OnGUI ()
		{
		if (debugInfo)
			m_textBox.OnGUI();
		}


	void DisableList (GameObject[] list)
		{
		foreach (GameObject go in list)
			go.SetActive(false);
		}


	void EnableList (GameObject[] list)
		{
		foreach (GameObject go in list)
			go.SetActive(true);
		}
	}

}