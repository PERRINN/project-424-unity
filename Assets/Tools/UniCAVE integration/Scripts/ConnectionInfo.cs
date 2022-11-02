
using UnityEngine;
using UnityEngine.UI;


namespace Perrinn424
{

public class ConnectionInfo : MonoBehaviour
{
	public Text infoText;

	NetworkMonitor m_monitor;


	void OnEnable ()
		{
		m_monitor = FindObjectOfType<NetworkMonitor>();
		if (m_monitor == null)
			{
			Debug.LogError("ConnectionInfo: No NetworkMonitor found in the scene. Component disabled.");
			enabled = false;
			return;
			}

		// Remove any placeholder text

		if (infoText != null)
			infoText.text = "";
		}


	void Update ()
		{
		if (infoText != null)
			{
			string machineName = System.Environment.MachineName;
			string state;

			if (m_monitor.currentRole == NetworkMonitor.Role.Undefined)
				{
				// Undefined state, most likely server or network not started

				state = $"Disconnected.\nMachine name: {machineName}\nLocal Address: {m_monitor.localAddress}\nClient state: {m_monitor.clientState}";
				}
			else
			if (m_monitor.currentRole == NetworkMonitor.Role.Client)
				{
				// Client connection to server

				state = $"Client: {machineName}\nAddress: {m_monitor.localAddress}\nState: ";

				if (m_monitor.clientState == NetworkMonitor.State.Connecting)
					{
					if (m_monitor.clientSearching)
						state += "Waiting for server...";
					else
						state += $"Connecting to server {m_monitor.serverAddress}...";
					}
				else
				if (m_monitor.clientState == NetworkMonitor.State.Connected)
					state += $"Connected to {m_monitor.serverAddress}";
				else
					state += "Disconnected";
				}
			else
				{
				// Server or host

				state = $"{m_monitor.currentRole}: {machineName}\nLocal Address: {m_monitor.localAddress}\nClient connections: {m_monitor.connectedClients}";
				}

			infoText.text = state;
			}
		}
	}
}
