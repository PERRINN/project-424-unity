
// Integration with Dynisma motion platform simulator
//
// Cordinate system is ISO 8855:
// https://www.mathworks.com/help/driving/ug/coordinate-systems.html


using UnityEngine;
using VehiclePhysics;
using EdyCommonTools;
using System;
using System.Text;


namespace Perrinn424
{
#if false
public class DynismaMotionPlatform : VehicleBehaviour
	{
	public string host = "127.0.0.1";
	public int port = 56236 ;

	[Space(5)]
	public bool motionEnabled = true;

	[Header("Test mode")]
	public bool testModeEnabled = false;
	public TestData testData = new TestData();

	[Serializable]
	public class TestData
		{
		[Range(-20,20)]
		public float accelerationX;
		[Range(-20,20)]
		public float accelerationY;
		[Range(-20,20)]
		public float accelerationZ;
		[Range(-2,2)]
		public float angularAccelerationX;
		[Range(-2,2)]
		public float angularAccelerationY;
		public float angularAccelerationZ;
		[Range(-30,30)]
		public float steeringTorque;
		[Range(0,100)]
		public float carSpeed;
		}


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


	UdpConnection m_udp = new UdpConnection();
	MotionData m_motionData = new MotionData();


	public override void OnEnableVehicle ()
		{
		// Connect with host

		try {
			m_udp.StartConnection(port+20);
			m_udp.SetDestination(host, port);
			}
		catch (Exception ex)
			{
			DebugLogWarning("Connection error: " + ex.Message + ". Component disabled.");
			enabled = false;
			}
		}


	public override void OnDisableVehicle ()
		{
		m_udp.StopConnection();
		}


	public override void FixedUpdateVehicle ()
		{
		if (testModeEnabled)
			SendTestData();
		else
			SendMotionData();
		}


	void SendMotionData ()
		{
		if (!motionEnabled) return;

		// Fill the short data struct (ISO convention)
		// https://www.mathworks.com/help/driving/ug/coordinate-systems.html

		Vector3 accel = vehicle.localAcceleration;
		m_motionData.accelerationX = accel.z;
		m_motionData.accelerationY = -accel.x;
		m_motionData.accelerationZ = accel.y;

		// ISO: Each axis is positive in the clockwise direction, when looking in the positive direction of that axis.
		// Unity: Each axis is positive in the counter-clockwise direction, when looking in the positive direction of that axis.

		Vector3 angularAccel = vehicle.localAngularAcceleration;
		m_motionData.angularAccelerationX = -angularAccel.z;
		m_motionData.angularAccelerationY = angularAccel.x;
		m_motionData.angularAccelerationZ = -angularAccel.y;

		// Speed and time

		m_motionData.carSpeed = vehicle.speed;
		m_motionData.simulationTime = Time.time;

		// Send data via UDP

		m_udp.SendMessageBinary(ObjectUtility.GetBytesFromStruct<MotionData>(m_motionData));
		}


	void SendTestData ()
		{
		if (!motionEnabled) return;

		// Fill the motion data struct with the test values

		m_motionData.accelerationX = testData.accelerationX;
		m_motionData.accelerationY = testData.accelerationY;
		m_motionData.accelerationZ = testData.accelerationZ;
		m_motionData.angularAccelerationX = testData.angularAccelerationX;
		m_motionData.angularAccelerationY = testData.angularAccelerationY;
		m_motionData.angularAccelerationZ = testData.angularAccelerationZ;
		m_motionData.steeringTorque = testData.steeringTorque;
		m_motionData.carSpeed = testData.carSpeed;
		m_motionData.simulationTime = Time.time;

		// Send data via UDP

		m_udp.SendMessageBinary(ObjectUtility.GetBytesFromStruct<MotionData>(m_motionData));
		}

	}
#endif
}