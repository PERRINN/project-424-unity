
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using EdyCommonTools;
using Mirror;


namespace Perrinn424
{

[RequireComponent(typeof(NetworkManager))]
public class ConnectionScreen : MonoBehaviour
	{
	[Header("On-screen widget")]
	public bool debugInfo = false;
	public GUITextBox.Settings debugWidget = new GUITextBox.Settings();


	NetworkManager m_manager;

	GUITextBox m_textBox = new GUITextBox();
	StringBuilder m_text = new StringBuilder(1024);

	// Trick to assign a default font to the GUI box. Configure it at the script settings in the Editor.
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
		}


	void Update ()
		{
		if (debugInfo)
			{
			m_text.Clear();
			m_text.Append($"Server active:    {NetworkServer.active}\n\n");
			m_text.Append($"Client active:    {NetworkClient.active}\n");
			m_text.Append($"Client connected: {NetworkClient.isConnected}\n");
			m_text.Append($"Client ready:     {NetworkClient.ready}\n\n");

			string localPlayer = NetworkClient.localPlayer != null? "yes" : "no";
			m_text.Append($"Local player:     {localPlayer}\n");
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
	}

}