
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
		public int listeningPort = 56236;
		}


	UdpConnection m_listener = new UdpConnection();
	UdpListenThread m_thread = new UdpListenThread();
	byte[] m_buffer = new byte[1024];
	int m_size = 0;


	public override void Open ()
		{
		m_listener.StartConnection(settings.listeningPort);
		m_thread.Start(m_listener, OnReceiveData);
		m_thread.threadSleepIntervalMs = 1;

		ClearState();
		TakeControlSnapshot();
		}


	public override void Close ()
		{
		m_thread.Stop();
		m_listener.StopConnection();

		ClearState();
		StorePreviousState();
		TakeControlSnapshot();
		}


	public override void Update ()
		{
		lock (m_buffer)
			{
			if (m_size > 0)
				{
				// New packet available. Convert buffer to state.

				StorePreviousState();
				// TODO Convert buffer to state

				m_size = 0;
				}
			}
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