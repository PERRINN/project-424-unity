//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;


namespace VehiclePhysics.Examples
{

public class SimpleTrackController : VehicleBase
	{
	[Header("Simple Track Controller")]
	public VPWheelCollider LeftTrack0;
	public VPWheelCollider LeftTrack1;
	public VPWheelCollider LeftTrack2;
	public VPWheelCollider LeftTrack3;

	[Space(5)]
	public VPWheelCollider RightTrack0;
	public VPWheelCollider RightTrack1;
	public VPWheelCollider RightTrack2;
	public VPWheelCollider RightTrack3;

	public TireFrictionLegacy trackFriction = new TireFrictionLegacy();

	public float trackRpm = 50.0f;
	public float trackTorque = 3000.0f;
	public float trackBrakeTorque = 5000.0f;

	[Range(-1,1)]
	public int leftTrackInput = 0;
	[Range(-1,1)]
	public int rightTrackInput = 0;


	// Each track is drived by an independent ideal engine.
	// These simulate the actual hydraulic engines that power real tracks in
	// excavators (no direct driveline from the engine).

	DirectDrive m_leftDrive;
	DirectDrive m_rightDrive;


	// Initialize the controller and blocks

	protected override void OnInitialize ()
		{
		// Declare the number of wheels

		SetNumberOfWheels(8);

		if (LeftTrack0 == null || LeftTrack1 == null || LeftTrack2 == null || LeftTrack3 == null
			|| RightTrack0 == null || RightTrack1 == null || RightTrack2 == null || RightTrack3 == null)
			{
			Debug.LogError("Missing VPWheelCollider");
			return;
			}

		// Configure mandatory data per wheel

		ConfigureWheelData(wheelState[0], wheels[0], LeftTrack0);
		ConfigureWheelData(wheelState[1], wheels[1], LeftTrack1);
		ConfigureWheelData(wheelState[2], wheels[2], LeftTrack2);
		ConfigureWheelData(wheelState[3], wheels[3], LeftTrack3);

		ConfigureWheelData(wheelState[4], wheels[4], RightTrack0);
		ConfigureWheelData(wheelState[5], wheels[5], RightTrack1);
		ConfigureWheelData(wheelState[6], wheels[6], RightTrack2);
		ConfigureWheelData(wheelState[7], wheels[7], RightTrack3);

		// Initialize DirectDrive blocks and conect each one with its track

		m_leftDrive = new DirectDrive();
		m_rightDrive = new DirectDrive();

		SetupTrackTransmission(m_leftDrive, wheels[0], wheels[1], wheels[2], wheels[3]);
		SetupTrackTransmission(m_rightDrive, wheels[4], wheels[5], wheels[6], wheels[7]);

		// Set up the relaxed criteria for putting the wheels to sleep. This allows half of the
		// wheels to be braked without the vehicle entering in full sleep mode.

		vehicleSleepCriteria = VehicleSleepCriteria.Relaxed;
		}

	// Set up the state values at the blocks, tires, etc.

	protected override void DoUpdateBlocks ()
		{
		SendInputToTrack(leftTrackInput, m_leftDrive,  wheels[0], wheels[1], wheels[2], wheels[3]);
		SendInputToTrack(rightTrackInput, m_rightDrive, wheels[4], wheels[5], wheels[6], wheels[7]);
		}


	// Return wheel index based on axle and position

	public override int GetWheelIndex (int axle, WheelPos position = WheelPos.Default)
		{
		if (axle < 0 || axle > 3) return -1;

		int wheel = (int)position;
		if (wheel < 0) wheel = 0;
		else if (wheel >= 2) wheel = 1;

		return wheel * 4 + axle;
		}


	// =========================================================================================


	void ConfigureWheelData(WheelState ws, Wheel wheel, VPWheelCollider wheelCol)
		{
		ws.wheelCol = wheelCol;
		ws.steerable = false;
		wheel.tireFriction = trackFriction;
		wheel.radius = wheelCol.radius;
		wheel.mass = wheelCol.mass;
		}

	// Create a differential configured as hard-link

	Differential NewLockedDifferential ()
		{
		Differential diff = new Differential();
		diff.settings.gearRatio = 1.0f;
		diff.settings.type = Differential.Type.Locked;
		return diff;
		}


	// Configure a single track consisting on 4 wheels hard-linked together.

	void SetupTrackTransmission (Block powerTrainOutput,
			Wheel w0, Wheel w1, Wheel w2, Wheel w3)
		{
		Differential diff0 = NewLockedDifferential();
		Differential diff1 = NewLockedDifferential();
		Differential diff2 = NewLockedDifferential();

		// Wheels 0 and 1 linked by differential 0

		Block.Connect(w0, 0, diff0, 0);
		Block.Connect(w1, 0, diff0, 1);

		// Wheels 2 and 3 linked by differential 1

		Block.Connect(w2, 0, diff1, 0);
		Block.Connect(w3, 0, diff1, 1);

		// Differentials 0 and 1 linked by differential 3

		Block.Connect(diff0, 0, diff2, 0);
		Block.Connect(diff1, 0, diff2, 1);

		// Differential 3 connected to power train output

		Block.Connect(diff2, 0, powerTrainOutput, 0);
		}


	// Convert the input state into torque and brake in the track

	void SendInputToTrack (int input, DirectDrive track, Wheel w0, Wheel w1, Wheel w2, Wheel w3)
		{
		track.maxMotorTorque = trackTorque;
		track.maxRpm = trackRpm;

		if (input == 0)
			{
			float brakeTorque = trackBrakeTorque * 0.25f;	// 4 wheels per track

			w0.AddBrakeTorque(brakeTorque);
			w1.AddBrakeTorque(brakeTorque);
			w2.AddBrakeTorque(brakeTorque);
			w3.AddBrakeTorque(brakeTorque);
			track.motorInput = 0.0f;
			}
		else
			{
			track.motorInput = Mathf.Sign(input);
			}
		}
	}
}