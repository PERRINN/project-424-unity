
// Integration with Dynisma motion platform simulator
//
// Cordinate system is ISO 8855:
// https://www.mathworks.com/help/driving/ug/coordinate-systems.html


using UnityEngine;
using VehiclePhysics;
using VehiclePhysics.InputManagement;
using EdyCommonTools;
using System;
using System.Runtime.InteropServices;


namespace Perrinn424
{

public class DynismaMotionPlatformDMGS : VehicleBehaviour
	{
	public bool motionEnabled = true;
	public Settings settings = new Settings();

	[Serializable]
	public class Settings
		{
		public string host = "127.0.0.1";
		public int port = 56236;
		[Range(10, 500)]
		public int maxTransferFrequency = 150;		// packets/s
		}

	[Header("Test mode")]
	public bool testModeEnabled = false;
	public TestData testData = new TestData();

	[Serializable]
	public class TestData
		{
		[Range(-20,20)]
		public float gLongDemand;
		[Range(-20,20)]
		public float gLatDemand;
		[Range(-20,20)]
		public float gVertDemand;
		[Range(-2,2)]
		public float dnRollDemand;
		[Range(-2,2)]
		public float dnPitchDemand;
		[Range(-2,2)]
		public float dnYawDemand;
		[Range(0,100)]
		public float vCar;
		[Range(-2, 2)]
		public float aSlipCar;
		[Range(-0.2f, 0.2f)]
		public float nSlipCar;
		[Range(-0.02f, 0.02f)]
		public float dnSlipCar;
		}

	struct MotionData
		{
		public byte MotionConsent;
		public byte SimulationRunningHeartbeat;		// Switch between 0 and 1 each frame
		public sbyte NSimRequest;
		public sbyte SteeringTorqueEnableRequest;
		public byte BResetAvailable;

		public float rWashoutLong;
		public float rWashoutLat;
		public float rWashoutVert;
		public float rWashoutRoll;
		public float rWashoutPitch;
		public float rWashoutYaw;
		public float fWashoutLong;
		public float fWashoutLat;
		public float fWashoutVert;
		public float fWashoutRoll;
		public float fWashoutPitch;
		public float fWashoutYaw;
		public float zWashoutLong;
		public float zWashoutLat;
		public float zWashoutVert;
		public float zWashoutRoll;
		public float zWashoutPitch;
		public float zWashoutYaw;
		public float sLongPrepos1;
		public float sLongPrepos2;
		public float vCarLongPrepos1;
		public float vCarLongPrepos2;
		public float rSideslipCue;
		public float rEngineCue;
		public float rUpshiftCue;
		public float rDownshiftCue;

		public float gLongDemand;				// m/s²		Chassis longitudinal acceleration
		public float gLatDemand;				// m/s²		Chassis lateral acceleration
		public float gVertDemand;				// m/s²		Chassis vertical acceleration
		public float dnRollDemand;				// rad/s²	Chassis roll acceleration
		public float dnPitchDemand;				// rad/s²	Chassis pitch acceleration
		public float dnYawDemand;				// rad/s²	Chassis yaw acceleration

		public float vLongDemand;
		public float vLatDemand;
		public float vVertDemand;
		public float nRollDemand;
		public float nPitchDemand;
		public float nYawDemand;
		public float xLongDemand;
		public float xLatDemand;
		public float xVertDemand;
		public float aRollDemand;
		public float aPitchDemand;
		public float aYawDemand;

		public float vCar;						// m/s		Chassis longitudinal velocity

		public float MSteer;
		public float nEngine;
		public float NGear;

		public float aSlipCar;					// rad		Vehicle slip angle
		public float nSlipCar;					// rad/s	Vehicle slip angle - first derivative
		public float dnSlipCar;					// rad/s²	Vehicle slip angle - second derivative
		public float Timestamp;					// s		Unix timestamp
		}


	UdpConnection m_udp = new UdpConnection();
	MotionData m_motionData = new MotionData();
	int m_skipCount;
	int m_sendInterval;

	bool m_heartbeat;
	double m_slip1;
	double m_slip2;
	double m_slip3;


	public override int GetUpdateOrder ()
		{
		// Execute after Perrinn424Input to ensure the force feedback values
		// have been calculated for the current simulation step.

		return 100;
		}


	public override void OnEnableVehicle ()
		{
		m_skipCount = 0;

		m_heartbeat = false;
		m_slip1 = vehicle.speedAngle;
		m_slip2 = 0.0;
		m_slip3 = 0.0;

		// Connect with host

		try {
			m_udp.StartConnection(settings.port+20);
			m_udp.SetDestination(settings.host, settings.port);

			Debug.Log($"DynismaMotionPlatform: sending motion data to {settings.host}:{settings.port} (max {settings.maxTransferFrequency} Hz), frame size {Marshal.SizeOf(m_motionData)} bytes");
			}
		catch (Exception ex)
			{
			Debug.LogWarning($"DynismaMotionPlatform connection error: {ex.Message}. Component disabled.");
			enabled = false;
			return;
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
			if (testModeEnabled)
				SendTestData();
			else
				SendMotionData();

			m_skipCount = 0;
			}
		}


	void SendMotionData ()
		{
		if (!motionEnabled) return;

		// Heartbeat

		m_motionData.SimulationRunningHeartbeat = m_heartbeat? (byte)1 : (byte)0;
		m_heartbeat = !m_heartbeat;

		// Fill the short data struct (ISO convention)
		// https://www.mathworks.com/help/driving/ug/coordinate-systems.html

		Vector3 accel = vehicle.localAcceleration;
		m_motionData.gLongDemand = accel.z;
		m_motionData.gLatDemand = -accel.x;
		m_motionData.gVertDemand = accel.y;

		// ISO: Each axis is positive in the clockwise direction, when looking in the positive direction of that axis.
		// Unity: Each axis is positive in the counterclockwise direction, when looking in the positive direction of that axis.

		Vector3 angularAccel = vehicle.localAngularAcceleration;
		m_motionData.nRollDemand = -angularAccel.z;
		m_motionData.nPitchDemand = angularAccel.x;
		m_motionData.nYawDemand = -angularAccel.y;

		// Slip - Second order backward differentiation (more accurate)

		double dt = Time.deltaTime;
		float slip = vehicle.speedAngle;

		m_motionData.aSlipCar = slip;
		m_motionData.nSlipCar = (float)((3 * slip - 4 * m_slip1 + m_slip2) / (2 * dt));
		m_motionData.dnSlipCar = (float)((2 * slip - 5 * m_slip1 + 4 * m_slip2 - m_slip3) / (dt * dt));

		m_slip3 = m_slip2;
		m_slip2 = m_slip1;
		m_slip1 = slip;

		// Speed and time

		m_motionData.vCar = vehicle.localVelocity.z;
		m_motionData.Timestamp = Time.time;

		// Send data via UDP

		m_udp.SendMessageBinary(ObjectUtility.GetBytesFromStruct<MotionData>(m_motionData));
		}


	void SendTestData ()
		{
		if (!motionEnabled) return;

		m_motionData.SimulationRunningHeartbeat = m_heartbeat? (byte)1 : (byte)0;
		m_heartbeat = !m_heartbeat;

		m_motionData.gLongDemand = testData.gLongDemand;
		m_motionData.gLatDemand = testData.gLatDemand;
		m_motionData.gVertDemand = testData.gVertDemand;
		m_motionData.dnRollDemand = testData.dnRollDemand;
		m_motionData.dnPitchDemand = testData.dnPitchDemand;
		m_motionData.dnYawDemand = testData.dnYawDemand;
		m_motionData.vCar = testData.vCar;
		m_motionData.aSlipCar = testData.aSlipCar;
		m_motionData.nSlipCar = testData.nSlipCar;
		m_motionData.dnSlipCar = testData.dnSlipCar;

		m_motionData.Timestamp = Time.time;

		// Send data via UDP

		m_udp.SendMessageBinary(ObjectUtility.GetBytesFromStruct<MotionData>(m_motionData));
		}

	}

}