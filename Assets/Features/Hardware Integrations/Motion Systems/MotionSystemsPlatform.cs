
// Integration with Dynisma motion platform simulator
//
// Cordinate system is ISO 8855:
// https://www.mathworks.com/help/driving/ug/coordinate-systems.html


using UnityEngine;
using VehiclePhysics;
using EdyCommonTools;
using System;
using System.Runtime.InteropServices;


namespace Perrinn424
{

public class MotionSystemsPlatform : VehicleBehaviour
	{
	public bool motionEnabled = true;
	[Header("Settings")]
	public Settings settings = new Settings();


	[Serializable]
	public class Settings
		{
		public string host = "127.0.0.1";
		public int port = 11223;
		[Range(10, 500)]
		public int maxTransferFrequency = 150;		// packets/s
		}


	struct MotionData
		{
		public int version;						// first version = 1
		public int size;						// size of this struct in bytes

		public int state;						// 0 = running, 1 = pause
		public int gear;						// -1 = R, 0 = N, 1 = D

		public double simulationTime;			// s
		public double brakePosition;			// 0.0 -> released, 1.0 -> fully pressed

		public double linearAccelerationX;		// m/s2
		public double linearAccelerationY;		// m/s2
		public double linearAccelerationZ;		// m/s2

		public double linearVelocityX;			// m/s
		public double linearVelocityY;			// m/s
		public double linearVelocityZ;			// m/s

		public double angularVelocityX;			// rad/s
		public double angularVelocityY;			// rad/s
		public double angularVelocityZ;			// rad/s

		public double roll;						// rad
		public double pitch;					// rad
		public double yaw;						// rad
		public double speed;					// m/s

		// Normalized slip. 0.0 = no slip or wheel lifted, 1.0 = peak tire grip, >1.0 = wheel sliping beyond the peak tire grip (reduced grip)
		public double wheelSlipFL;
		public double wheelSlipFR;
		public double wheelSlipRL;
		public double wheelSlipRR;

		// 0.0 = no compression (wheel lifted), 1.0 = maximum compression
		public double suspensionFL;
		public double suspensionFR;
		public double suspensionRL;
		public double suspensionRR;

		// 0 = hard terrain (asphalt, kerbs, etc), 1 = rought terrain (offroad, grass, sand, etc)
		public int terrainTypeFL;
		public int terrainTypeFR;
		public int terrainTypeRL;
		public int terrainTypeRR;

		// rad. >0.0 = understeer, <0.0 = oversteer. The absolute value is the angle of understeer/oversteer.
		public double understeer;
		}


	UdpConnection m_udp = new UdpConnection();
	MotionData m_motionData = new MotionData();
	int m_structSize;
	int m_skipCount;
	int m_sendInterval;


	public override int GetUpdateOrder ()
		{
		// Execute after Perrinn424Input to ensure the force feedback values
		// have been calculated for the current simulation step.

		return 100;
		}


	public override void OnEnableVehicle ()
		{
		m_skipCount = 0;
		m_structSize = Marshal.SizeOf(m_motionData);

		// Connect with host

		try {
			m_udp.StartConnection(settings.port+20);
			m_udp.SetDestination(settings.host, settings.port);

			Debug.Log($"MotionSystemsPlatform: sending motion data to {settings.host}:{settings.port} (max {settings.maxTransferFrequency} Hz)");
			}
		catch (Exception ex)
			{
			Debug.LogWarning($"MotionSystemsPlatform connection error: {ex.Message}. Component disabled.");
			enabled = false;
			}
		}


	public override void OnDisableVehicle ()
		{
		m_udp.StopConnection();
		}


	public override void FixedUpdateVehicle ()
		{
		// The interval is stored so it can be inspected in Debug inspector

		float fixedUpdateFrequency = 1.0f / Time.fixedDeltaTime;
		m_sendInterval = Mathf.CeilToInt(fixedUpdateFrequency / settings.maxTransferFrequency);

		m_skipCount++;
		if (m_skipCount >= m_sendInterval)
			{
			SendMotionData();
			m_skipCount = 0;
			}
		}


	void SendMotionData ()
		{
		if (!motionEnabled) return;

		m_motionData.version = 1;
		m_motionData.size = m_structSize;

		m_motionData.simulationTime = Time.time;
		m_motionData.state = Time.timeScale == 0.0f || vehicle.paused? 1 : 0;

		// Fill the short data struct (ISO convention)
		// https://www.mathworks.com/help/driving/ug/coordinate-systems.html

		Vector3 accel = vehicle.localAcceleration;
		// m_motionData.accelerationX = accel.z;
		// m_motionData.accelerationY = -accel.x;
		// m_motionData.accelerationZ = accel.y;

		// ISO: Each axis is positive in the clockwise direction, when looking in the positive direction of that axis.
		// Unity: Each axis is positive in the counterclockwise direction, when looking in the positive direction of that axis.

		Vector3 angularAccel = vehicle.localAngularAcceleration;
		// m_motionData.angularAccelerationX = -angularAccel.z;
		// m_motionData.angularAccelerationY = angularAccel.x;
		// m_motionData.angularAccelerationZ = -angularAccel.y;

		// Speed and time

		// m_motionData.carSpeed = vehicle.speed;



		m_motionData.gear = 0;						// -1 = R, 0 = N, 1 = D

		m_motionData.brakePosition = 0;			// 0.0 -> released, 1.0 -> fully pressed

		m_motionData.linearAccelerationX = 0;		// m/s2
		m_motionData.linearAccelerationY = 0;		// m/s2
		m_motionData.linearAccelerationZ = 0;		// m/s2

		m_motionData.linearVelocityX = 0;			// m/s
		m_motionData.linearVelocityY = 0;			// m/s
		m_motionData.linearVelocityZ = 0;			// m/s

		m_motionData.angularVelocityX = 0;			// rad/s
		m_motionData.angularVelocityY = 0;			// rad/s
		m_motionData.angularVelocityZ = 0;			// rad/s

		m_motionData.roll = 0;						// rad
		m_motionData.pitch = 0;					// rad
		m_motionData.yaw = 0;						// rad
		m_motionData.speed = 0;					// m/s

		// Normalized slip. 0.0 = no slip or wheel lifted, 1.0 = peak tire grip, >1.0 = wheel sliping beyond the peak tire grip (reduced grip)
		m_motionData.wheelSlipFL = 0;
		m_motionData.wheelSlipFR = 0;
		m_motionData.wheelSlipRL = 0;
		m_motionData.wheelSlipRR = 0;

		// 0.0 = no compression (wheel lifted), 1.0 = maximum compression
		m_motionData.suspensionFL = 0;
		m_motionData.suspensionFR = 0;
		m_motionData.suspensionRL = 0;
		m_motionData.suspensionRR = 0;

		// 0 = hard terrain (asphalt, kerbs, etc), 1 = rought terrain (offroad, grass, sand, etc)
		m_motionData.terrainTypeFL = 0;
		m_motionData.terrainTypeFR = 0;
		m_motionData.terrainTypeRL = 0;
		m_motionData.terrainTypeRR = 0;

		// rad. >0.0 = understeer, <0.0 = oversteer. The absolute value is the angle of understeer/oversteer.
		m_motionData.understeer = 0;


		// Send data via UDP

		m_udp.SendMessageBinary(ObjectUtility.GetBytesFromStruct<MotionData>(m_motionData));
		}
	}

}