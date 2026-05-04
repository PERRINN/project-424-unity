
// Integration with Dynisma motion platform simulator


using UnityEngine;
using System;
using EdyCommonTools;


namespace Perrinn424
{

public class DynismaEyePointClient : MonoBehaviour
	{
	public enum Protocol { Vioso, DomeProjection }
	public Protocol protocol = Protocol.Vioso;

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

	struct EyePointData32
		{
		// Eye point position from motion platform
		// ISO 8855 (https://www.mathworks.com/help/driving/ug/coordinate-systems.html)

		public float xLongWorld;		// m
		public float xLatWorld;			// m
		public float xVertWorld;		// m
		public float aRollWorld;		// rad
		public float aPitchWorld;		// rad
		public float aYawWorld; 		// rad

		// 24 bytes
		}

	UdpConnection m_listener = new UdpConnection();
	UdpListenThread m_thread = new UdpListenThread();
	byte[] m_buffer = new byte[1024];
	int m_size = 0;
	EyePointData m_eyePointData = new EyePointData();
	EyePointData32 m_eyePointData32 = new EyePointData32();


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
					{
					if (protocol == Protocol.Vioso)
						{
						m_eyePointData = ObjectUtility.GetStructFromBytes<EyePointData>(m_buffer);
						}
					else
					if (protocol == Protocol.DomeProjection)
						{
						m_eyePointData32 = ObjectUtility.GetStructFromBytes<EyePointData32>(m_buffer);
						}
					}
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
			if (protocol == Protocol.Vioso)
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
			else
			if (protocol == Protocol.DomeProjection)
				{
				// Eyepoint data passed directly to the DynismaDomeProjectionOrigin component.
				// Values come in ISO 8855, with semantic names. Convert to Unity coordinates.

				DynismaDomeProjectionOrigin.eyePointPos = new Vector3()
					{
					x = -m_eyePointData32.xLatWorld,
					y = m_eyePointData32.xVertWorld,
					z = m_eyePointData32.xLongWorld,
					};

				DynismaDomeProjectionOrigin.eyePointRot = new Vector3()
					{
					x = m_eyePointData32.aPitchWorld * Mathf.Rad2Deg,
					y = -m_eyePointData32.aYawWorld * Mathf.Rad2Deg,
					z = -m_eyePointData32.aRollWorld * Mathf.Rad2Deg,
					};
				}
			}
		}
	}

}