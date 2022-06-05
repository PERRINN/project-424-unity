
// Integration with Dynisma motion platform simulator


using UnityEngine;
using EdyCommonTools;


namespace Perrinn424
{

public class DynismaTester : MonoBehaviour
	{
	public string host = "127.0.0.1";
	public int port = 56234;
	public int listeningPort = 56236;

	public GUITextBox.Settings widget = new GUITextBox.Settings();


	// Trick to configure a default font in the widget. Configure the font at the script settings.
	[HideInInspector] public Font defaultConsoleFont;


	GUITextBox m_widget = new GUITextBox();
	UdpSender m_sender;
	UdpConnection m_listener = new UdpConnection();
	UdpListenThread m_thread = new UdpListenThread();
	byte[] m_buffer = new byte[1024];
	int m_size = 0;


	void OnValidate ()
		{
		if (widget.font == null)
			widget.font = defaultConsoleFont;
		}


	void OnEnable ()
		{
		// Initialize connections

        m_sender = new UdpSender(host, port);
		m_listener.StartConnection(listeningPort);
		m_thread.Start(m_listener, OnReceiveData);
		m_size = 0;

		// Initialize widget

		m_widget.settings = widget;
		m_widget.title = "Dynisma Tester";
		m_widget.richText = true;
		}


	void OnDisable ()
		{
		m_sender.Close();
		m_thread.Stop();
		m_listener.StopConnection();
		}


	// This is called from the listener thread

	void OnReceiveData ()
		{
		lock (m_buffer)
			{
			m_size = m_listener.GetMessageBinary(m_buffer);
			}
		}


	}

}