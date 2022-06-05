
// Integration with Dynisma motion platform simulator


using UnityEngine;
using EdyCommonTools;
using System.Text;


namespace Perrinn424
{

public class DynismaTester : MonoBehaviour
	{
	public string host = "127.0.0.1";
	public int port = 56234;
	public int listeningPort = 56236;

	public GUITextBox.Settings widget = new GUITextBox.Settings();


	struct MotionData
		{
		public double accelerationX;			// m/s2
		public double accelerationY;			// m/s2
		public double accelerationZ;			// m/s2
		public double angularAccelerationX;		// rad/s2
		public double angularAccelerationY;		// rad/s2
		public double angularAccelerationZ;		// rad/s2
		public double steeringTorque;			// Nm
		public double carSpeed;					// m/s
		public double simulationTime;			// s
		}


	// Trick to configure a default font in the widget. Configure the font at the script settings.
	[HideInInspector] public Font defaultConsoleFont;

	GUITextBox m_widget = new GUITextBox();
	StringBuilder m_text = new StringBuilder();

	UdpSender m_sender;
	UdpConnection m_listener = new UdpConnection();
	UdpListenThread m_thread = new UdpListenThread();
	byte[] m_buffer = new byte[1024];
	int m_size = 0;
	MotionData m_motionData = new MotionData();
	int m_received = 0;


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
		m_received = 0;

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


	void Update ()
		{
		lock (m_buffer)
			{
			if (m_size > 0)
				{
				m_motionData = ObjectUtility.GetStructFromBytes<MotionData>(m_buffer);
				m_size = 0;
				}
			}

		UpdateWidgetText();
		}


	void OnGUI ()
		{
		m_widget.OnGUI();
		}


	void UpdateWidgetText ()
		{
		m_text.Clear();
		m_text.Append("Motion Platform Data\n\n");
		m_text.Append($"Packets:               {m_received}\n");
		m_text.Append($"Acceleration:          X:{m_motionData.accelerationX,12:0.000000}  Y:{m_motionData.accelerationY,12:0.000000}  Z:{m_motionData.accelerationZ,12:0.000000}  m/s2\n");
		m_text.Append($"Angular Acceleration:  X:{m_motionData.angularAccelerationX,12:0.000000}  Y:{m_motionData.angularAccelerationY,12:0.000000}  Z:{m_motionData.angularAccelerationZ,12:0.000000}  rad/s2\n");
		m_text.Append($"Steering Torque:      {m_motionData.steeringTorque,11:0.000000}  Nm\n");
		m_text.Append($"Car Speed:            {m_motionData.carSpeed,11:0.000000}  m/s\n");
		m_text.Append($"Simulation Time:      {m_motionData.simulationTime,11:0.000000}  s\n");
		m_widget.text = m_text.ToString();
		}

	// This is called from the listener thread

	void OnReceiveData ()
		{
		lock (m_buffer)
			{
			m_size = m_listener.GetMessageBinary(m_buffer);
			m_received++;
			}
		}
	}

}