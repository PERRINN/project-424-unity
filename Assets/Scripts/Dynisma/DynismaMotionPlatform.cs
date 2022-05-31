

using UnityEngine;
using VehiclePhysics;
using EdyCommonTools;
using System;
using System.Text;


namespace Perrinn424
{

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
		[Range(-2,2)]
		public float longitudinalG = 0.0f;
		[Range(-2,2)]
		public float lateralG = 0.0f;
		[Range(-2,2)]
		public float verticalG = 0.0f;
		[Range(-0.5f,0.5f)]
		public float yawVelocity = 0.0f;
		[Range(-0.5f,0.5f)]
		public float rollVelocity = 0.0f;
		[Range(-0.5f,0.5f)]
		public float pitchVelocity = 0.0f;
		}


	struct MotionData
		{
		public float longitudinalG;		// longitudinal g-force level
		public float lateralG;			// lateral g-force level
		public float verticalG;			// vertical g-force level
		public float yawVelocity;		// angular velocity around vertical axis (racing)
		public float rollVelocity;		// angular velocity around side-to-side axis (racing)
		public float pitchVelocity;		// angular velocity around front-to-back axis (racing)
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

		// Fill the short data struct

		Vector3 accel = vehicle.localAcceleration;
		m_motionData.longitudinalG = -accel.z / Gravity.reference;
		m_motionData.lateralG = accel.x / Gravity.reference;
		m_motionData.verticalG = accel.y / Gravity.reference;

		Vector3 angularVelocity = vehicle.cachedRigidbody.angularVelocity;
		m_motionData.yawVelocity = angularVelocity.y;
		m_motionData.rollVelocity = angularVelocity.z;
		m_motionData.pitchVelocity = angularVelocity.x;

		// Send data via UDP

		m_udp.SendMessageBinary(ObjectUtility.GetBytesFromStruct<MotionData>(m_motionData));
		}


	void SendTestData ()
		{
		if (!motionEnabled) return;

		// Fill the short data struct with the test values

		m_motionData.longitudinalG = testData.longitudinalG;
		m_motionData.lateralG = testData.lateralG;
		m_motionData.verticalG = testData.verticalG;
		m_motionData.yawVelocity = testData.yawVelocity;
		m_motionData.rollVelocity = testData.rollVelocity;
		m_motionData.pitchVelocity = testData.pitchVelocity;

		// Send data via UDP

		m_udp.SendMessageBinary(ObjectUtility.GetBytesFromStruct<MotionData>(m_motionData));
		}

	}

}