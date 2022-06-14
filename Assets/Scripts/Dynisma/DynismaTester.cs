
// Integration with Dynisma motion platform simulator


using UnityEngine;
using EdyCommonTools;
using System.Text;


namespace Perrinn424
{

public class DynismaTester : MonoBehaviour
	{
	[Header("Input Data")]
	public string host = "127.0.0.1";
	public int port = 56234;
	public int maxFrequency = 100;
	public int maxSteerAngle = 300;

	[Header("Motion Data")]
	public int listeningPort = 56236;

	[Header("Debug")]
	public bool showWidget = true;
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


	struct InputData
		{
		public double throttle;
		public double brake;
		public double steerAngle;
		public bool upShift;
		public bool downShift;
		public byte button;				// 1 bit per button
		public byte rotary;				// two 8-position rotaries on the wheel, split the uint8 into two 4-bit values
		}


	// Trick to configure a default font in the widget. Configure the font at the script settings.
	[HideInInspector] public Font defaultConsoleFont;

	GUITextBox m_textBox = new GUITextBox();
	StringBuilder m_text = new StringBuilder();

	// Listen to motion data

	UdpConnection m_listener = new UdpConnection();
	UdpListenThread m_thread = new UdpListenThread();
	byte[] m_buffer = new byte[1024];
	int m_size = 0;
	MotionData m_motionData = new MotionData();
	int m_received = 0;
	int m_packetCount = 0;
	float m_packetCountTime;
	float m_packetFrequency;

	// Send input data

	UdpSender m_sender;
	InputData m_inputData = new InputData();
	int m_skipCount = 0;
	float m_throttle = 0.0f;
	float m_brake = 0.0f;
	float m_steerInput = 0.0f;
	bool m_upShift = false;
	bool m_downShift = false;
	bool[] m_button = new bool[8];
	int m_rotary0 = 0;
	int m_rotary1 = 0;


	void OnValidate ()
		{
		if (widget.font == null)
			widget.font = defaultConsoleFont;
		}


	void OnEnable ()
		{
		// Initialize connections

        m_sender = new UdpSender(host, port);
		m_skipCount = 0;

		m_listener.StartConnection(listeningPort);
		m_size = 0;
		m_received = 0;
		m_packetCount = 0;
		m_packetCountTime = Time.time;
		m_packetFrequency = 0.0f;
		m_thread.Start(m_listener, () =>
			{
			lock (m_buffer)
				{
				m_size = m_listener.GetMessageBinary(m_buffer);
				m_received++;
				}
			});

		// Initialize widget

		m_textBox.settings = widget;
		m_textBox.title = "Dynisma Tester";
		}


	void OnDisable ()
		{
		m_sender.Close();
		m_thread.Stop();
		m_listener.StopConnection();
		}


	void Update ()
		{
		// Receive motion data

		lock (m_buffer)
			{
			if (m_size > 0)
				{
				m_motionData = ObjectUtility.GetStructFromBytes<MotionData>(m_buffer);
				m_size = 0;
				}
			}

		// Measure packet frequency

		if (Time.time - m_packetCountTime >= 1.0f)
			{
			m_packetFrequency = m_received - m_packetCount;
			m_packetCount = m_received;
			m_packetCountTime = Time.time;
			}

		// Update text from motion data

		if (showWidget)
			UpdateWidgetText();

		// Send input data limited by the maximum frequency specified

		float fixedUpdateFrequency = 1.0f / Time.fixedDeltaTime;
		int sendInterval = Mathf.CeilToInt(fixedUpdateFrequency / maxFrequency);

		m_skipCount++;
		if (m_skipCount >= sendInterval)
			{
			SendInputData();
			}
		}


	void SendInputData ()
		{
		m_inputData.throttle = m_throttle;
		m_inputData.brake = m_brake;
		m_inputData.steerAngle = m_steerInput * maxSteerAngle;
		m_inputData.upShift = m_upShift;
		m_inputData.downShift = m_downShift;

		m_inputData.button = 0;
		if (m_button[0]) m_inputData.button |= 0x01;
		if (m_button[1]) m_inputData.button |= 0x02;
		if (m_button[2]) m_inputData.button |= 0x04;
		if (m_button[3]) m_inputData.button |= 0x08;
		if (m_button[4]) m_inputData.button |= 0x10;
		if (m_button[5]) m_inputData.button |= 0x20;
		if (m_button[6]) m_inputData.button |= 0x40;
		if (m_button[7]) m_inputData.button |= 0x80;

		int rotary0 = Mathf.Clamp(m_rotary0, 0, 15);
		int rotary1 = Mathf.Clamp(m_rotary1, 0, 15);
		m_inputData.rotary = (byte)((rotary1 << 4) | rotary0);

		m_sender.SendSync(ObjectUtility.GetBytesFromStruct<InputData>(m_inputData));
		}

	readonly string[] m_rotary0Labels = new string[] { "A", "B", "C", "D", "E", "F", "G", "H" };
	readonly string[] m_rotary1Labels = new string[] { "M", "N", "O", "P", "Q", "R", "S", "T" };

	void OnGUI ()
		{
		if (!showWidget)
			return;

		m_textBox.OnGUI();

		Rect boxRect = m_textBox.boxRect;
		float margin = m_textBox.margin;

		int boxWidth = 216;
		boxRect.x += boxRect.width - boxWidth - 8;
		boxRect.width = boxWidth;
		boxRect.xMin = boxRect.xMax - boxWidth;
		boxRect.yMin = boxRect.yMax - 160;

		GUILayout.BeginArea(boxRect);
		m_steerInput = GUILayout.HorizontalScrollbar(m_steerInput, 0.2f, -1.0f, 1.1f);
		m_throttle = GUILayout.HorizontalScrollbar(m_throttle, 0.1f, 0.0f, 1.1f);
		m_brake = GUILayout.HorizontalScrollbar(m_brake, 0.1f, 0.0f, 1.1f);

		GUILayout.BeginHorizontal();
		m_upShift = GUILayout.RepeatButton("  Up  ");
		m_downShift = GUILayout.RepeatButton("Down");
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		m_button[0] = GUILayout.RepeatButton("1");
		m_button[1] = GUILayout.RepeatButton("2");
		m_button[2] = GUILayout.RepeatButton("3");
		m_button[3] = GUILayout.RepeatButton("4");
		m_button[4] = GUILayout.RepeatButton("5");
		m_button[5] = GUILayout.RepeatButton("6");
		m_button[6] = GUILayout.RepeatButton("7");
		m_button[7] = GUILayout.RepeatButton("8");
		GUILayout.EndHorizontal();

		m_rotary0 = GUILayout.Toolbar(m_rotary0, m_rotary0Labels);
		m_rotary1 = GUILayout.Toolbar(m_rotary1, m_rotary1Labels);

		GUILayout.EndArea();
		}


	void UpdateWidgetText ()
		{
		m_text.Clear();
		m_text.Append("Motion Platform Data (Received)\n\n");
		m_text.Append($"Packets:               {m_received}  ({m_packetFrequency:0.} Hz)\n");
		m_text.Append($"Acceleration:          X:{m_motionData.accelerationX,10:0.000000}  Y:{m_motionData.accelerationY,10:0.000000}  Z:{m_motionData.accelerationZ,10:0.000000}  m/s2\n");
		m_text.Append($"Angular Acceleration:  X:{m_motionData.angularAccelerationX,10:0.000000}  Y:{m_motionData.angularAccelerationY,10:0.000000}  Z:{m_motionData.angularAccelerationZ,10:0.000000}  rad/s2\n");
		m_text.Append($"Steering Torque:      {m_motionData.steeringTorque,11:0.000000}  Nm\n");
		m_text.Append($"Car Speed:            {m_motionData.carSpeed,11:0.000000}  m/s\n");
		m_text.Append($"Simulation Time:      {m_motionData.simulationTime,11:0.000000}  s\n");
		m_text.Append("\nInput Data (Sent)\n\n");
		m_text.Append($"Steer Angle:          {m_inputData.steerAngle,8:0.000}\n");
		m_text.Append($"Throttle:             {m_inputData.throttle,8:0.000}\n");
		m_text.Append($"Brake:                {m_inputData.brake,8:0.000}\n");
		m_text.Append($"Up Shift:              {m_inputData.upShift}\n");
		m_text.Append($"Down Shift:            {m_inputData.downShift}\n");

		m_text.Append($"Buttons:               ");
		for (int i = 0, c = m_button.Length; i < c; i++)
			if (m_button[i]) m_text.Append($"{i+1} ");
		m_text.Append("\n");

		int rotary0 = (m_inputData.rotary & 0x0F);
		int rotary1 = (m_inputData.rotary >> 4);
		m_text.Append($"Rotary 1:              {rotary0}\n");
		m_text.Append($"Rotary 2:              {rotary1}\n");

		m_textBox.text = m_text.ToString();
		}
	}

}