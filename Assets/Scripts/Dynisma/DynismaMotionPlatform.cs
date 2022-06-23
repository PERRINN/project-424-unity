
// Integration with Dynisma motion platform simulator
//
// Cordinate system is ISO 8855:
// https://www.mathworks.com/help/driving/ug/coordinate-systems.html


using UnityEngine;
using VehiclePhysics;
using VehiclePhysics.InputManagement;
using EdyCommonTools;
using System;


namespace Perrinn424
{

public class DynismaMotionPlatform : VehicleBehaviour
	{
	public bool motionEnabled = true;
	[Header("Settings")]
	public Settings settings = new Settings();

	[Serializable]
	public class Settings
		{
		public string host = "127.0.0.1";
		public int port = 56236;
		[Range(10, 500)]
		public int maxTransferFrequency = 150;		// packets/s
		public float maxSteeringTorque = 20.0f;		// Nm
		}

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
		[Range(-2,2)]
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
	Perrinn424Input m_input;
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

		// Connect with host

		try {
			m_udp.StartConnection(settings.port+20);
			m_udp.SetDestination(settings.host, settings.port);

			Debug.Log($"DynismaMotionPlatform: sending motion data to {settings.host}:{settings.port} (max {settings.maxTransferFrequency} Hz)");
			}
		catch (Exception ex)
			{
			Debug.LogWarning($"DynismaMotionPlatform connection error: {ex.Message}. Component disabled.");
			enabled = false;
			}

		// Locate the Perrinn424Input component for the force feedback data

		m_input = vehicle.GetComponentInChildren<Perrinn424Input>();
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

		// Fill the short data struct (ISO convention)
		// https://www.mathworks.com/help/driving/ug/coordinate-systems.html

		Vector3 accel = vehicle.localAcceleration;
		m_motionData.accelerationX = accel.z;
		m_motionData.accelerationY = -accel.x;
		m_motionData.accelerationZ = accel.y;

		// ISO: Each axis is positive in the clockwise direction, when looking in the positive direction of that axis.
		// Unity: Each axis is positive in the counterclockwise direction, when looking in the positive direction of that axis.

		Vector3 angularAccel = vehicle.localAngularAcceleration;
		m_motionData.angularAccelerationX = -angularAccel.z;
		m_motionData.angularAccelerationY = angularAccel.x;
		m_motionData.angularAccelerationZ = -angularAccel.y;

		// Speed and time

		m_motionData.carSpeed = vehicle.speed;
		m_motionData.simulationTime = Time.time;

		// Force feedback

		m_motionData.steeringTorque = 0.0;

		if (m_input != null && m_input.isActiveAndEnabled)
			{
			InputDevice.ForceFeedback forceFeedback = m_input.ForceFeedback();

			if (forceFeedback != null && forceFeedback.force)
				m_motionData.steeringTorque = settings.maxSteeringTorque * forceFeedback.forceMagnitude;
			}

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

}