
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
	public Settings settings = new Settings();


	[Serializable]
	public class Settings
		{
		public string host = "127.0.0.1";
		public int port = 42477;
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
		public double brakePressure;			// bar, from 0 to up to 100 bar

		// Coordinate system is ISO 8855
		// https://www.mathworks.com/help/driving/ug/coordinate-systems.html

		public double linearAccelerationX;		// m/s2
		public double linearAccelerationY;		// m/s2
		public double linearAccelerationZ;		// m/s2

		public double linearVelocityX;			// m/s
		public double linearVelocityY;			// m/s
		public double linearVelocityZ;			// m/s

		public double angularVelocityX;			// rad/s
		public double angularVelocityY;			// rad/s
		public double angularVelocityZ;			// rad/s

		// States

		public double steer;					// rad (front wheels average)
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
		public double understeerAngle;
		}


	UdpConnection m_udp = new UdpConnection();
	MotionData m_motionData = new MotionData();
	int m_structSize;
	int m_skipCount;
	int m_sendInterval;

	VehicleBase.WheelState m_wheelFL;
	VehicleBase.WheelState m_wheelFR;
	VehicleBase.WheelState m_wheelRL;
	VehicleBase.WheelState m_wheelRR;

	Perrinn424CarController m_controller;


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
		m_controller = vehicle as Perrinn424CarController;

		// Initialize wheel identifiers

		int wheelFLid = vehicle.GetWheelIndex(0, VehicleBase.WheelPos.Left);
		int wheelFRid = vehicle.GetWheelIndex(0, VehicleBase.WheelPos.Right);

		int lastAxle = vehicle.GetAxleCount()-1;
		if (lastAxle < 0) lastAxle = 0;

		int wheelRLid = vehicle.GetWheelIndex(lastAxle, VehicleBase.WheelPos.Left);
		int wheelRRid = vehicle.GetWheelIndex(lastAxle, VehicleBase.WheelPos.Right);

		if (wheelFLid < 0 || wheelFRid < 0 || wheelRLid < 0 || wheelRRid < 0)
			{
			Debug.Log("MotionSystemsPlatform: vehicle should have 4 wheels. Component disabled.");
			enabled = false;
			return;
			}

		m_wheelFL = vehicle.wheelState[wheelFLid];
		m_wheelFR = vehicle.wheelState[wheelFRid];
		m_wheelRL = vehicle.wheelState[wheelRLid];
		m_wheelRR = vehicle.wheelState[wheelRRid];

		// Connect with host

		try {
			m_udp.StartConnection(settings.port+20);
			m_udp.SetDestination(settings.host, settings.port);

			Debug.Log($"MotionSystemsPlatform: sending motion data to {settings.host}:{settings.port} (max {settings.maxTransferFrequency} Hz), frame size {m_structSize} bytes.");
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

		// Frame and simulation data

		m_motionData.version = 1;
		m_motionData.size = m_structSize;

		m_motionData.simulationTime = Time.time;
		m_motionData.state = Time.timeScale == 0.0f || vehicle.paused? 1 : 0;

		// Vehicle input

		m_motionData.gear = m_controller.gear;
		m_motionData.brakePressure = m_controller.brakePosition * 100;

		// Coordinate system is ISO 8855
		// https://www.mathworks.com/help/driving/ug/coordinate-systems.html
		// ISO: Each axis is positive in the clockwise direction, when looking in the positive direction of that axis.
		// Unity: Each axis is positive in the counterclockwise direction, when looking in the positive direction of that axis.

		// Accelerations

		Vector3 accel = vehicle.localAcceleration;
		m_motionData.linearAccelerationX = accel.z;
		m_motionData.linearAccelerationY = -accel.x;
		m_motionData.linearAccelerationZ = accel.y;

		// Velocities

		Vector3 linearVelocity = vehicle.localVelocity;
		m_motionData.linearVelocityX = linearVelocity.z;
		m_motionData.linearVelocityY = -linearVelocity.x;
		m_motionData.linearVelocityZ = linearVelocity.y;

		Vector3 angularVelocity = vehicle.localAngularVelocity;
		m_motionData.angularVelocityX = -angularVelocity.z;
		m_motionData.angularVelocityY = angularVelocity.x;
		m_motionData.angularVelocityZ = -angularVelocity.y;

		// Vehicle state

		m_motionData.steer = (m_wheelFL.steeringAngle + m_wheelFR.steeringAngle) * 0.5f * Mathf.Deg2Rad;
		m_motionData.speed = vehicle.speed;

		Vector3 angles = vehicle.cachedTransform.eulerAngles;
		m_motionData.roll = MathUtility.ClampAngle(angles.z) * Mathf.Deg2Rad;
		m_motionData.pitch = MathUtility.ClampAngle(angles.x) * Mathf.Deg2Rad;
		m_motionData.yaw = angles.y * Mathf.Deg2Rad;

		// Normalized wheel slip state

		m_motionData.wheelSlipFL = m_wheelFL.normalizedSlip;
		m_motionData.wheelSlipFR = m_wheelFR.normalizedSlip;
		m_motionData.wheelSlipRL = m_wheelRL.normalizedSlip;
		m_motionData.wheelSlipRR = m_wheelRR.normalizedSlip;

		// Normalized suspension compression

		m_motionData.suspensionFL = m_wheelFL.suspensionState.compressionRatio;
		m_motionData.suspensionFR = m_wheelFR.suspensionState.compressionRatio;
		m_motionData.suspensionRL = m_wheelRL.suspensionState.compressionRatio;
		m_motionData.suspensionRR = m_wheelRR.suspensionState.compressionRatio;

		// Terrain type
		// 0 = hard terrain (asphalt, kerbs, etc), 1 = rought terrain (offroad, grass, sand, etc)

		m_motionData.terrainTypeFL = m_wheelFL.groundMaterial != null? (int)m_wheelFL.groundMaterial.surfaceType : 0;
		m_motionData.terrainTypeFR = m_wheelFR.groundMaterial != null? (int)m_wheelFR.groundMaterial.surfaceType : 0;
		m_motionData.terrainTypeRL = m_wheelRL.groundMaterial != null? (int)m_wheelRL.groundMaterial.surfaceType : 0;
		m_motionData.terrainTypeRR = m_wheelRR.groundMaterial != null? (int)m_wheelRR.groundMaterial.surfaceType : 0;

		// rad. >0.0 = understeer, <0.0 = oversteer. The absolute value is the angle of understeer/oversteer.

		m_motionData.understeerAngle = float.IsNaN(m_controller.understeerAngle)? 0.0 : m_controller.understeerAngle * Mathf.Deg2Rad;

		// Send data via UDP

		m_udp.SendMessageBinary(ObjectUtility.GetBytesFromStruct<MotionData>(m_motionData));
		}
	}

}