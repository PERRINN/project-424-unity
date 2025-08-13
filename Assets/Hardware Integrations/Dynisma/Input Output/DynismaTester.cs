
// Integration with Dynisma motion platform simulator


using UnityEngine;
using EdyCommonTools;
using System.Text;
using System.Runtime.InteropServices;
using VersionCompatibility;


namespace Perrinn424
{

public class DynismaTester : MonoBehaviour
	{
	[UnityEngine.Serialization.FormerlySerializedAs("maxFrequency")]
	public int maxSendFrequency = 100;

	[Header("Input Data")]
	[UnityEngine.Serialization.FormerlySerializedAs("host")]
	public string inputHost = "127.0.0.1";
	[UnityEngine.Serialization.FormerlySerializedAs("port")]
	public int inputPort = 56234;
	public int steerAngleRange = 300;

	[Header("Eye Point Data")]
	public string eyePointHost = "127.0.0.1";
	public int eyePointPort = 56232;

	[Header("Simulated Platform")]
	public float maxDisplacement = 1.0f;
	public float displacementRate = 1.0f;
	public float maxRotation = 30.0f;
	public float rotationRate = 30.0f;
	public bool autoCenter = true;
	public bool autoLoop = false;

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


	struct EyePointData
		{
		// Eye point position from motion platform
		// ISO 8855 (https://www.mathworks.com/help/driving/ug/coordinate-systems.html)

		public double eyePointPosX;		// m
		public double eyePointPosY;		// m
		public double eyePointPosZ;		// m
		public double eyePointRotX;		// rad
		public double eyePointRotY;		// rad
		public double eyePointRotZ;		// rad
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

	UdpSender m_inputSender = null;
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

	// Send eye point data

	UdpSender m_eyePointSender = null;
	EyePointData m_eyePointData = new EyePointData();
	int m_autoLoopDir = 1;


	void OnValidate ()
		{
		if (widget.font == null)
			widget.font = defaultConsoleFont;
		}


	void OnEnable ()
		{
		// Show runtime byte sizes of each struct

		Debug.Log($"InputData: {Marshal.SizeOf(typeof(InputData))}  MotionData: {Marshal.SizeOf(typeof(MotionData))}  EyePointData: {Marshal.SizeOf(typeof(EyePointData))}");

		// Initialize widget

		m_textBox.settings = widget;
		m_textBox.title = "Dynisma Tester";

		// Initialize connections

		Connect();
		}


	void OnDisable ()
		{
		Disconnect();
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

		// Measure received packet frequency

		if (Time.time - m_packetCountTime >= 1.0f)
			{
			m_packetFrequency = m_received - m_packetCount;
			m_packetCount = m_received;
			m_packetCountTime = Time.time;
			}

		// Update text from motion data

		if (showWidget)
			UpdateWidgetText();

		// Update simulated motion platform position

		UpdatePlatformPosition();
		}


	void FixedUpdate ()
		{
		// Send input data limited by the maximum frequency specified

		float fixedUpdateFrequency = 1.0f / Time.fixedDeltaTime;
		int sendInterval = Mathf.CeilToInt(fixedUpdateFrequency / maxSendFrequency);

		m_skipCount++;
		if (m_skipCount >= sendInterval)
			{
			if (m_inputSender != null)
				SendInputData();
			if (m_eyePointSender != null)
				SendEyePointData();

			m_skipCount = 0;
			}
		}


	void Connect ()
		{
        m_inputSender = new UdpSender(inputHost, inputPort);
        m_eyePointSender = new UdpSender(eyePointHost, eyePointPort);
		m_skipCount = 0;

		try
			{
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
			}
		catch (System.Exception e)
			{
			Debug.LogWarning($"DynismaTester Connection error: Most likely the port {listeningPort} is already in use by other application.\n{e.Message}");
			}
		}


	void Disconnect ()
		{
		m_inputSender.Close();
		m_eyePointSender.Close();
		m_thread.Stop();
		m_listener.StopConnection();

		m_inputSender = null;
		m_eyePointSender = null;
		}


	void SendInputData ()
		{
		m_inputData.throttle = m_throttle;
		m_inputData.brake = m_brake;
		m_inputData.steerAngle = m_steerInput * steerAngleRange / 2;
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

		m_inputSender.SendSync(ObjectUtility.GetBytesFromStruct<InputData>(m_inputData));
		}


	void SendEyePointData ()
		{
		// Convert simulated motion platform pose to ISO 8855
		// (https://www.mathworks.com/help/driving/ug/coordinate-systems.html)

		Vector3 position = transform.localPosition;
		Vector3 rotation = transform.localRotation.eulerAngles;

		m_eyePointData.eyePointPosX = position.z;
		m_eyePointData.eyePointPosY = -position.x;
		m_eyePointData.eyePointPosZ = position.y;
		m_eyePointData.eyePointRotX = -MathUtility.ClampAngle(rotation.z) * Mathf.Deg2Rad;
		m_eyePointData.eyePointRotY = MathUtility.ClampAngle(rotation.x) * Mathf.Deg2Rad;
		m_eyePointData.eyePointRotZ = -MathUtility.ClampAngle(rotation.y) * Mathf.Deg2Rad;

		m_eyePointSender.SendSync(ObjectUtility.GetBytesFromStruct<EyePointData>(m_eyePointData));
		}


	void UpdatePlatformPosition ()
		{
		bool ctrl = UnityInput.ctrlKeyPressed;
		bool shift = UnityInput.shiftKeyPressed;
		bool translate = !ctrl || shift;
		bool rotate = ctrl || shift;

		Vector3 targetPos = Vector3.zero;
		if (translate)
			{
			if (UnityInput.GetKey(UnityKey.LeftArrow)) targetPos.x = -1;
			if (UnityInput.GetKey(UnityKey.RightArrow)) targetPos.x = +1;
			if (UnityInput.GetKey(UnityKey.PageDown)) targetPos.y = -1;
			if (UnityInput.GetKey(UnityKey.PageUp)) targetPos.y = +1;
			if (UnityInput.GetKey(UnityKey.DownArrow)) targetPos.z = -1;
			if (UnityInput.GetKey(UnityKey.UpArrow)) targetPos.z = +1;
			}

		if (autoLoop)
			{
			if (m_autoLoopDir == 1 && transform.localPosition.z >= 1)
				m_autoLoopDir = -1;
			else
			if (m_autoLoopDir == -1 && transform.localPosition.z <= -1)
				m_autoLoopDir = 1;

			targetPos = new Vector3(0, 0, m_autoLoopDir);
			}

		Vector3 targetAngles = Vector3.zero;
		if (rotate)
			{
			if (UnityInput.GetKey(UnityKey.LeftArrow)) targetAngles.y = -1;
			if (UnityInput.GetKey(UnityKey.RightArrow)) targetAngles.y = +1;
			if (UnityInput.GetKey(UnityKey.PageDown)) targetAngles.z = +1;
			if (UnityInput.GetKey(UnityKey.PageUp)) targetAngles.z = -1;
			if (UnityInput.GetKey(UnityKey.DownArrow)) targetAngles.x = -1;
			if (UnityInput.GetKey(UnityKey.UpArrow)) targetAngles.x = +1;
			}

		targetPos = targetPos.normalized * maxDisplacement;
		if (autoCenter || targetPos.magnitude > 0.0f)
			transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPos, displacementRate * Time.deltaTime);

		targetAngles = targetAngles * maxRotation;
		if (autoCenter || targetAngles.magnitude > 0.0f)
			{
			Quaternion targetRot = Quaternion.Euler(targetAngles);
			transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRot, rotationRate * Time.deltaTime);
			}
		}


	void OnGUI ()
		{
		if (!showWidget)
			return;

		m_textBox.OnGUI();
		GUISettings();
		GUIInputControls();
		}


	readonly string[] m_rotary0Labels = new string[] { "A", "B", "C", "D", "E", "F", "G", "H" };
	readonly string[] m_rotary1Labels = new string[] { "M", "N", "O", "P", "Q", "R", "S", "T" };


	void GUIInputControls ()
		{
		Rect boxRect = m_textBox.boxRect;

		int boxWidth = 216;
		boxRect.x += boxRect.width - boxWidth - 8;
		boxRect.width = boxWidth;
		boxRect.xMin = boxRect.xMax - boxWidth;
		boxRect.yMin = boxRect.yMax - 160 - m_textBox.style.lineHeight * 6;

		GUILayout.BeginArea(boxRect);
		m_steerInput = GUILayout.HorizontalScrollbar(m_steerInput, 0.2f, -1.0f, 1.2f);
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


	GUIStyle m_inputFieldStyle = null;


	void GUISettings ()
		{
		if (m_inputFieldStyle == null)
			m_inputFieldStyle = new GUIStyle(GUI.skin.textField);

		m_inputFieldStyle.font = m_textBox.settings.font;
		m_inputFieldStyle.fontSize = m_textBox.settings.fontSize;

		float fieldSize = m_textBox.style.CalcSize(new GUIContent("888888888888888888888888")).x;

		Rect rect = m_textBox.contentRect;
		rect.y += m_textBox.margin/2;
		GUILayout.BeginArea(rect);
		GUILayout.Label("Listen to motion data at (port):", m_textBox.style);
		int.TryParse(GUILayout.TextField(listeningPort.ToString(), m_inputFieldStyle, GUILayout.Width(fieldSize/2)), out listeningPort);

		GUILayout.Label("Send input to (host, port):", m_textBox.style);
		GUILayout.BeginHorizontal();
		inputHost = GUILayout.TextField(inputHost, m_inputFieldStyle, GUILayout.Width(fieldSize));
		int.TryParse(GUILayout.TextField(inputPort.ToString(), m_inputFieldStyle, GUILayout.Width(fieldSize/2)), out inputPort);
		GUILayout.EndHorizontal();

		GUILayout.Label("Send eye point data to (host, port):", m_textBox.style);
		GUILayout.BeginHorizontal();
		eyePointHost = GUILayout.TextField(eyePointHost, m_inputFieldStyle, GUILayout.Width(fieldSize));
		int.TryParse(GUILayout.TextField(eyePointPort.ToString(), m_inputFieldStyle, GUILayout.Width(fieldSize/2)), out eyePointPort);
		GUILayout.EndHorizontal();
		GUILayout.EndArea();

		// Reset connections button

		rect.xMin = rect.width - 100;
		rect.height = 60;
		rect.width -= 8;
		if (GUI.Button(rect, "Reset\nConnections"))
			{
			Disconnect();
			Connect();
			}
		}


	void UpdateWidgetText ()
		{
		m_text.Clear();
		m_text.Append("\n\n\n\n\n\n\n\n\n\n");

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

		m_text.Append("\nEye Point Data (Sent)\n\n");
		m_text.Append($"Eye Point Position:    {m_eyePointData.eyePointPosX,6:0.000}, {m_eyePointData.eyePointPosY,6:0.000}, {m_eyePointData.eyePointPosZ,6:0.000}  m\n");
		m_text.Append($"Eye Point Rotation:    {m_eyePointData.eyePointRotX,6:0.000}, {m_eyePointData.eyePointRotY,6:0.000}, {m_eyePointData.eyePointRotZ,6:0.000}  rad\n");

		m_text.Append("\nMove eye point with arrows, page up/down, and ctrl/shift.");

		m_textBox.text = m_text.ToString();
		}
	}

}