
// Integration with Dynisma motion platform simulator


using UnityEngine;
using VehiclePhysics.InputManagement;
using EdyCommonTools;
using System;


namespace Perrinn424
{

public class DynismaInputDevice : InputDevice
	{
	public Settings settings = new Settings();

	[Serializable]
	public class Settings
		{
		public int listeningPort = 56234;
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

	UdpConnection m_listener = new UdpConnection();
	UdpListenThread m_thread = new UdpListenThread();
	byte[] m_buffer = new byte[1024];
	int m_size = 0;
	InputData m_inputData = new InputData();
	InputData m_prevInputData = new InputData();
	bool m_newInputData;
	bool m_firstState;


	public override void Open ()
		{
		m_newInputData = false;
		m_firstState = true;

		try {
			m_listener.StartConnection(settings.listeningPort);
			Debug.Log($"DynismaInputDevice: listening at port {settings.listeningPort}");
			}
		catch (Exception ex)
			{
			Debug.LogWarning("DynismaInputDevice connection error: " + ex.Message + ". Won't be reading inputs.");
			return;
			}

		m_thread.threadSleepIntervalMs = 1;
		m_thread.Start(m_listener, () =>
			{
			lock (m_buffer)
				{
				m_size = m_listener.GetMessageBinary(m_buffer);
				if (m_size > 0)
					{
					m_inputData = ObjectUtility.GetStructFromBytes<InputData>(m_buffer);

					if (m_newInputData)
						{
						// Previous packet was not processed in Update. Accumulate buttons to prevent input loss.

						m_inputData.upShift |= m_prevInputData.upShift;
						m_inputData.downShift |= m_prevInputData.downShift;
						m_inputData.button |= m_prevInputData.button;
						}

					m_prevInputData = m_inputData;
					m_newInputData = true;
					}
				}
			});

		ClearState();
		m_state.analog[1] = -32767;
		m_state.analog[2] = -32767;
		}


	public override void Close ()
		{
		m_thread.Stop();
		m_listener.StopConnection();

		ClearState();
		m_state.analog[1] = -32767;
		m_state.analog[2] = -32767;

		StorePreviousState();
		TakeControlSnapshot();
		}


	public override void Update ()
		{
		StorePreviousState();

		lock (m_buffer)
			{
			// New input data available? Convert to state.

			if (m_newInputData)
				{
				// Analog steer, throttle and brake

				float halfWheelRange = InputManager.instance.settings.physicalWheelRange * 0.5f;
				m_state.analog[0] = (int)(Mathf.Clamp((float)(m_inputData.steerAngle / halfWheelRange), -1.0f, 1.0f) * 32767);
				m_state.analog[1] = (int)(Mathf.Clamp01((float)(m_inputData.throttle)) * 32767 * 2 - 32767);
				m_state.analog[2] = (int)(Mathf.Clamp01((float)(m_inputData.brake)) * 32767 * 2 - 32767);

				// Buttons

				m_state.button[0] = (byte)((m_inputData.button >> 0) & 1);
				m_state.button[1] = (byte)((m_inputData.button >> 1) & 1);
				m_state.button[2] = (byte)((m_inputData.button >> 2) & 1);
				m_state.button[3] = (byte)((m_inputData.button >> 3) & 1);
				m_state.button[4] = (byte)((m_inputData.button >> 4) & 1);
				m_state.button[5] = (byte)((m_inputData.button >> 5) & 1);
				m_state.button[6] = (byte)((m_inputData.button >> 6) & 1);
				m_state.button[7] = (byte)((m_inputData.button >> 7) & 1);

				m_state.button[8] = (byte)(m_inputData.upShift? 1 : 0);
				m_state.button[9] = (byte)(m_inputData.downShift? 1 : 0);

				// Rotaries encoded as an individual button for each position

				int rotary0 = (m_inputData.rotary & 0x0F);
				int rotary1 = (m_inputData.rotary >> 4);

				m_state.button[10] = (byte)(rotary0 == 0? 1 : 0);
				m_state.button[11] = (byte)(rotary0 == 1? 1 : 0);
				m_state.button[12] = (byte)(rotary0 == 2? 1 : 0);
				m_state.button[13] = (byte)(rotary0 == 3? 1 : 0);
				m_state.button[14] = (byte)(rotary0 == 4? 1 : 0);
				m_state.button[15] = (byte)(rotary0 == 5? 1 : 0);
				m_state.button[16] = (byte)(rotary0 == 6? 1 : 0);
				m_state.button[17] = (byte)(rotary0 == 7? 1 : 0);
				m_state.button[18] = (byte)(rotary0 == 8? 1 : 0);
				m_state.button[19] = (byte)(rotary0 == 9? 1 : 0);
				m_state.button[20] = (byte)(rotary0 == 10? 1 : 0);
				m_state.button[21] = (byte)(rotary0 == 11? 1 : 0);
				m_state.button[22] = (byte)(rotary0 == 12? 1 : 0);
				m_state.button[23] = (byte)(rotary0 == 13? 1 : 0);
				m_state.button[24] = (byte)(rotary0 == 14? 1 : 0);
				m_state.button[25] = (byte)(rotary0 == 15? 1 : 0);

				m_state.button[30] = (byte)(rotary1 == 0? 1 : 0);
				m_state.button[31] = (byte)(rotary1 == 1? 1 : 0);
				m_state.button[32] = (byte)(rotary1 == 2? 1 : 0);
				m_state.button[33] = (byte)(rotary1 == 3? 1 : 0);
				m_state.button[34] = (byte)(rotary1 == 4? 1 : 0);
				m_state.button[35] = (byte)(rotary1 == 5? 1 : 0);
				m_state.button[36] = (byte)(rotary1 == 6? 1 : 0);
				m_state.button[37] = (byte)(rotary1 == 7? 1 : 0);
				m_state.button[38] = (byte)(rotary1 == 8? 1 : 0);
				m_state.button[39] = (byte)(rotary1 == 9? 1 : 0);
				m_state.button[40] = (byte)(rotary1 == 10? 1 : 0);
				m_state.button[41] = (byte)(rotary1 == 11? 1 : 0);
				m_state.button[42] = (byte)(rotary1 == 12? 1 : 0);
				m_state.button[43] = (byte)(rotary1 == 13? 1 : 0);
				m_state.button[44] = (byte)(rotary1 == 14? 1 : 0);
				m_state.button[45] = (byte)(rotary1 == 15? 1 : 0);

				// Acknowledge new input data converted

				m_newInputData = false;

				// Take a snapshot if this is the first state received

				if (m_firstState)
					{
					TakeControlSnapshot();
					m_firstState = false;
					}
				}
			}
		}


	// Comprehensive names for the controls


	public override void SetCustomControlName (ref ControlDefinition control)
		{
		if (control.type == ControlType.Analog)
			{
			switch (control.id0)
				{
				case 0: control.name = "STEER"; break;
				case 1: control.name = "THROTTLE"; break;
				case 2: control.name = "BRAKE"; break;
				}
			}
		else
		if (control.type == ControlType.Binary)
			{
			if (control.dualBinary)
				control.name = $"{GetBinaryControlName(control.id0)},{GetBinaryControlName(control.id1)}";
			else
				control.name = $"{GetBinaryControlName(control.id0)}";
			}
		}


	string GetBinaryControlName (int id)
		{
		if (id == 8)
			{
			return "UPSHIFT";
			}
		else
		if (id == 9)
			{
			return "DOWNSHIFT";
			}
		else
		if (id >= 10 && id <= 25)
			{
			return $"ROT1-{id}";
			}
		else
		if (id >= 20 && id <= 35)
			{
			return $"ROT2-{id}";
			}

		return $"BTN{id+1}";
		}
	}

}