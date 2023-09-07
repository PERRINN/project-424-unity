
// Integration with Dynisma motion platform simulator


using UnityEngine;
using System;
using EdyCommonTools;


namespace Perrinn424
{

public class DynismaEyePointClient : MonoBehaviour
	{
	public Settings settings = new Settings();

	[Serializable]
	public class Settings
		{
		public int listeningPort = 56232;
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

		// 48 bytes
		}

	UdpConnection m_listener = new UdpConnection();
	UdpListenThread m_thread = new UdpListenThread();
	byte[] m_buffer = new byte[1024];
	int m_size = 0;
	EyePointData m_eyePointData = new EyePointData();


	void OnEnable ()
		{
		try {
			m_listener.StartConnection(settings.listeningPort);
			Debug.Log($"DynismaEyePointClient: listening at port {settings.listeningPort}");
			}
		catch (Exception ex)
			{
			Debug.LogWarning("DynismaEyePointClient connection error: " + ex.Message + ". Won't be reading eye-point data.");
			enabled = false;
			return;
			}

		m_thread.threadSleepIntervalMs = 1;
		m_thread.Start(m_listener, () =>
			{
			m_size = m_listener.GetMessageBinary(m_buffer);

			lock (m_buffer)
				{
				if (m_size > 0)
					m_eyePointData = ObjectUtility.GetStructFromBytes<EyePointData>(m_buffer);
				}
			});
		}


	void OnDisable ()
		{
		m_thread.Stop();
		m_listener.StopConnection();
		}


	void Update ()
		{
		lock (m_buffer)
			{
			// Eyepoint data passed directly to all VIOSOCamera component instances.
			// Values come in ISO 8855 (https://www.mathworks.com/help/driving/ug/coordinate-systems.html).
			// Convert to Unity coordinates.

			VIOSOCamera.eyePointPos = new Vector3()
				{
				x = -(float)m_eyePointData.eyePointPosY,
				y = (float)m_eyePointData.eyePointPosZ,
				z = (float)m_eyePointData.eyePointPosX,
				};

			// For some reason VIOSO expects X and Z rotations with opposite sign, despite using the same rotation axes as Unity.

			VIOSOCamera.eyePointRot = new Vector3()
				{
				x = -(float)m_eyePointData.eyePointRotY,
				y = -(float)m_eyePointData.eyePointRotZ,
				z = (float)m_eyePointData.eyePointRotX,
				};
			}
		}
	}

}