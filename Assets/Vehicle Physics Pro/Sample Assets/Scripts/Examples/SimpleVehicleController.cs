//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;


namespace VehiclePhysics.Examples
{

public class SimpleVehicleController : VehicleBase
	{
	[Header("Simple Vehicle Controller")]
	public VPWheelCollider wheelFL;
	public VPWheelCollider wheelFR;
	public VPWheelCollider wheelRL;
	public VPWheelCollider wheelRR;
	public TireFrictionLegacy tireFriction = new TireFrictionLegacy();

	public float maxDriveTorque = 500.0f;
	public float maxBrakeTorque = 1000.0f;
	public float maxSteerAngle = 45.0f;
	public float maxDriveRpm = 50.0f;

	[Range(-1,1)]
	public float driveInput = 0.0f;
	[Range(0,1)]
	public float brakeInput = 0.0f;
	[Range(-1,1)]
	public float steerInput = 0.0f;


	// Internal vehicle blocks (powertrain)

	DirectDrive m_directDrive;
	Differential m_differential;


	// Initialize the controller and blocks

	protected override void OnInitialize ()
		{
		// Declare the number of wheels

		SetNumberOfWheels(4);

		if (wheelFL == null || wheelFR == null || wheelRL == null || wheelRR == null)
			{
			Debug.LogError("Missing VPWheelCollider");
			return;
			}

		// Configure mandatory data per wheel

		ConfigureWheelData(wheelState[0], wheels[0], wheelFL, true);
		ConfigureWheelData(wheelState[1], wheels[1], wheelFR, true);
		ConfigureWheelData(wheelState[2], wheels[2], wheelRL);
		ConfigureWheelData(wheelState[3], wheels[3], wheelRR);

		// Initialize DirectDrive and connect it to the rear wheels via differential

		m_directDrive = new DirectDrive();
		m_differential = new Differential();
		m_differential.settings.gearRatio = 1.0f;

		Block.Connect(wheels[2], 0, m_differential, 0);
		Block.Connect(wheels[3], 0, m_differential, 1);
		Block.Connect(m_differential, 0, m_directDrive, 0);
		}


	// WheelState and Wheel objects must be initialized with a minimum data per wheel

	void ConfigureWheelData (WheelState ws, Wheel wheel, VPWheelCollider wheelCol, bool steerable = false)
		{
		ws.wheelCol = wheelCol;
		ws.steerable = steerable;
		wheel.tireFriction = tireFriction;
		wheel.radius = wheelCol.radius;
		wheel.mass = wheelCol.mass;
		}


	// Set up the state values at the blocks, tires, etc.

	protected override void DoUpdateBlocks ()
		{
		// Feed the DirectDrive with the values and input from the controller's properties

		m_directDrive.motorInput = driveInput;
		m_directDrive.maxMotorTorque = maxDriveTorque;
		m_directDrive.maxRpm = maxDriveRpm;

		// Apply steering

		float angle = steerInput * maxSteerAngle;
		wheelState[0].steerAngle = angle;
		wheelState[1].steerAngle = angle;

		// Set the brakes at the Wheel blocks

		float brakeTorque = brakeInput * maxBrakeTorque;
		wheels[0].AddBrakeTorque(brakeTorque);
		wheels[1].AddBrakeTorque(brakeTorque);
		wheels[2].AddBrakeTorque(brakeTorque);
		wheels[3].AddBrakeTorque(brakeTorque);
		}
	}
}