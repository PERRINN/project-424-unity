//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

// Vehicle controller for the Perrinn 424 electric racing car.


using UnityEngine;
using EdyCommonTools;
using VehiclePhysics;


// Assignments of the custom channel for the 424 in the data bus

public struct Perrinn424Data					// ID			DESCRIPTION							UNITS		RESOLUTION		EXAMPLE
	{
	public const int ThrottleInput				= 0;		// Throttle input sent to mgu			ratio		1000			1000 = 1.0 = 100%
	public const int BrakePressure				= 1;		// Brake circuit pressure				bar			1000			30500 = 30.5 bar
	public const int DrsPosition				= 2;		// DRS position. 0 = closed, 1 = open	%			1000			1000 = 1.0 = 100% open

	public const int FrontRideHeight			= 3;		// Front ride height					m			1000			230 = 0.23 m = 230 mm
	public const int FrontRollAngle				= 4;		// Front roll angle (signed)			deg			1000			2334 = 2.345 degrees
	public const int RearRideHeight				= 5;		// Rear ride height						m			1000			230 = 0.23 m = 230 mm
	public const int RearRollAngle				= 6;		// Rear roll angle (signed)				deg			1000			2334 = 2.345 degrees

	// MGU data. Combine base ID with values.

	public const int FrontMguBase				= 10;		// Base ID for front MGU data
	public const int RearMguBase				= 20;		// Base ID for rear MGU data

	public const int Rpm						= 0;		// Motor rpm							rpm			1000			1200000 = 1200 rpm
	public const int Load						= 1;		// Motor load. Negative = renerative	ratio		1000			900 = 0.9 = 90%
	public const int Efficiency					= 2;		// Efficiency							ratio		1000			945 = 0.945
	public const int ElectricalPower			= 3;		// Generated electrical power			kW			1000			250000 = 250 kW
	public const int ElectricalTorque			= 4;		// Generated electrical torque			Nm			1000			50000 = 50 Nm
	public const int MechanicalTorque			= 5;		// Generated mechanical torque			Nm			1000			50000 = 50 Nm
	public const int StatorTorque				= 6;		// Pre-inertia torque					Nm			1000			55000 = 55 Nm
	public const int RotorTorque				= 7;		// Final torque in the mgu rotor		Nm			1000			50600 = 50.6 Nm
	public const int ShaftsTorque				= 8;		// Sum of torques at drive shafts		Nm			1000			150600 = 150.6 Nm
	public const int WheelsTorque				= 9;		// Sum of torques at wheels				Nm			1000			150600 = 150.6 Nm
	}


// Custom car controller for Perrinn 424

public class Perrinn424CarController : VehicleBase
	{
	public Inertia.Settings inertia = new Inertia.Settings();

	public VPAxle frontAxle;
	public VPAxle rearAxle;

	public TireDataContainerBase frontTires;
	public TireDataContainerBase rearTires;

	public Transform frontAxleReference;
	public Transform rearAxleReference;

	// Powertrain and dynamics

	public InputSettings input = new InputSettings();
	public MotorGeneratorUnit.Settings frontMgu = new MotorGeneratorUnit.Settings();
	public MotorGeneratorUnit.Settings rearMgu = new MotorGeneratorUnit.Settings();

	public Differential.Settings frontDifferential = new Differential.Settings();
	public Differential.Settings rearDifferential = new Differential.Settings();

	public Steering.Settings steering = new Steering.Settings();

	// Driving aids

	public SteeringAids.Settings steeringAids = new SteeringAids.Settings();
	public SpeedControl.Settings speedControl = new SpeedControl.Settings();

	// Safety aids

	public TractionControl.Settings tractionControl = new TractionControl.Settings();

	// Mostly internal settings not exposed in the inspector

	[System.NonSerialized]
	public float gearChangeMaxSpeed = 1.0f;

	// Motor input is ignored when brake pressure is beyond this value

	[System.NonSerialized]
	public float brakePressureThreshold = 3.0f;


	// Internal values exposed

	public float throttleInput { get => m_throttleInput; }
	public float brakePressure { get => m_brakePressure; }
	public int gear { get => m_gear; }


	// Private members

	Powertrain m_frontPowertrain;
	Powertrain m_rearPowertrain;

	Inertia m_inertia;
	Steering m_steering;

	float m_throttleInput;
	float m_brakePressure;
	int m_gearMode;
	int m_prevGearMode;
	int m_gear;

	AxleFrameSensor m_frontAxleSensor = new AxleFrameSensor();
	AxleFrameSensor m_rearAxleSensor = new AxleFrameSensor();


	// Internal Powertrain helper class

	class Powertrain
		{
		public MotorGeneratorUnit mgu;
		public Differential differential;

		Wheel m_leftWheel;
		Wheel m_rightWheel;


		public Powertrain (Wheel leftWheelBlock, Wheel rightWheelBlock)
			{
			mgu = new MotorGeneratorUnit();
			differential = new Differential();

			Block.Connect(leftWheelBlock, differential, 0);
			Block.Connect(rightWheelBlock, differential, 1);
			Block.Connect(differential, mgu);

			m_leftWheel = leftWheelBlock;
			m_rightWheel = rightWheelBlock;
			}


		public void SetInputs (int gearInput, float throttleInput, float brakePressure)
			{
			// MGU

			mgu.gearInput = gearInput;
			mgu.throttleInput = throttleInput;
			mgu.brakePressure = brakePressure;

			// Wheel brakes

			float brakeTorque = mgu.GetWheelBrakeTorque(brakePressure);
			m_leftWheel.AddBrakeTorque(brakeTorque);
			m_rightWheel.AddBrakeTorque(brakeTorque);
			}


		public void FillDataBus (int[] channel, int baseId)
			{
			channel[baseId + Perrinn424Data.Rpm] = (int)(mgu.sensorRpm * 1000);
			channel[baseId + Perrinn424Data.Load] = (int)(mgu.sensorLoad * 1000);
			channel[baseId + Perrinn424Data.Efficiency] = (int)(mgu.sensorEfficiency * 1000);
			channel[baseId + Perrinn424Data.ElectricalPower] = (int)(mgu.sensorElectricalPower * 1000);
			channel[baseId + Perrinn424Data.ElectricalTorque] = (int)(mgu.sensorElectricalTorque * 1000);
			channel[baseId + Perrinn424Data.MechanicalTorque] = (int)(mgu.sensorMechanicalTorque * 1000);
			channel[baseId + Perrinn424Data.StatorTorque] = (int)(mgu.sensorStatorTorque * 1000);
			channel[baseId + Perrinn424Data.RotorTorque] = (int)(mgu.sensorRotorTorque * 1000);
			channel[baseId + Perrinn424Data.ShaftsTorque] = (int)((m_leftWheel.driveTorque + m_rightWheel.driveTorque) * 1000);

			float leftWheelTorque = m_leftWheel.driveTorque + m_leftWheel.brakeTorque * Mathf.Sign(-m_leftWheel.contact.localVelocity.y);
			float rightWheelTorque = m_rightWheel.driveTorque + m_rightWheel.brakeTorque * Mathf.Sign(-m_rightWheel.contact.localVelocity.y);
			channel[baseId + Perrinn424Data.WheelsTorque] = (int)((leftWheelTorque + rightWheelTorque) * 1000);
			}


		public string GetDebutStr ()
			{
			float power = mgu.sensorRpm * Block.RpmToW * mgu.sensorElectricalTorque;
			return $"{power/1000:0.0} kW";
			}


		// Overrides to the differential and driveline settings.
		// The DifferentialOverride enum matches the override values at the Data Bus

		public enum DifferentialOverride { None, ForceLocked, ForceUnlocked };

		DifferentialOverride m_differentialOverride = DifferentialOverride.None;
		Differential.Type m_differentialType;


		public DifferentialOverride differentialOverride
			{
			get
				{ return m_differentialOverride; }

			set
				{
				if (value != m_differentialOverride)
					{
					if (m_differentialOverride == DifferentialOverride.None)
						m_differentialType = differential.settings.type;

					switch (value)
						{
						case DifferentialOverride.ForceLocked:
							differential.settings.type = Differential.Type.Locked;
							break;

						case DifferentialOverride.ForceUnlocked:
							differential.settings.type = Differential.Type.Open;
							break;

						case DifferentialOverride.None:
							differential.settings.type = m_differentialType;
							break;
						}

					m_differentialOverride = value;
					}
				}
			}
		}


	// Initialization section
	// ---------------------------------------------------------------------------------------------


	// Default values when the component is first instanced.
	// These will be overriden when serialized values are available.

	Perrinn424CarController()
		{
		frontAxle = new VPAxle();
		rearAxle = new VPAxle();
		frontAxle.steeringMode = Steering.SteeringMode.Steerable;
		frontAxle.brakeCircuit = Brakes.BrakeCircuit.Front;
		rearAxle.brakeCircuit = Brakes.BrakeCircuit.Rear;
		}


	protected override void OnInitialize ()
		{
		// Prepare the internal helpers: inertia, steering

		m_inertia = new Inertia();
		m_inertia.settings = inertia;
		m_inertia.Apply(cachedRigidbody);

		m_steering = new Steering();
		m_steering.settings = steering;

		// Declare the number of wheels

		SetNumberOfWheels(4);

		// Verify wheel references

		if (frontAxle.leftWheel == null || frontAxle.rightWheel == null
			|| rearAxle.leftWheel == null || rearAxle.rightWheel == null)
			{
			DebugLogError("Some VPWheelCollider references are missing in the axles.\nAll axles must have a reference to the corresponding left-right VPWheelCollider objects.");
			enabled = false;
			return;
			}

		// Initialize tire friction

		if (frontTires == null || rearTires == null)
			{
			DebugLogError("Missing tire settings. Ensure both front and rear tires are set");
			enabled = false;
			return;
			}

		// We now have the inherited properties wheels[wheelCount] and wheelsState[wheelCount].
		// Configure the wheels in the axles accordingly:
		//
		//	- Mandatory data
		//	- Steering
		//	- Brakes

		ConfigureAxle(frontAxle, 0, 1, frontTires.GetTireFriction());
		ConfigureAxle(rearAxle, 2, 3, rearTires.GetTireFriction());

		// Configure an independent powertrain per axle

        m_frontPowertrain = new Powertrain(wheels[0], wheels[1]);
		m_frontPowertrain.mgu.settings = frontMgu;
		m_frontPowertrain.differential.settings = frontDifferential;

        m_rearPowertrain = new Powertrain(wheels[2], wheels[3]);
		m_rearPowertrain.mgu.settings = rearMgu;
		m_rearPowertrain.differential.settings = rearDifferential;

		// Configure axle sensors

		m_frontAxleSensor.Configure(this, 0);
		m_rearAxleSensor.Configure(this, 1);

		// Initialize internal data

		m_gearMode = (int)Gearbox.AutomaticGear.N;
		m_prevGearMode = (int)Gearbox.AutomaticGear.N;
		data.Set(Channel.Input, InputData.AutomaticGear, m_gearMode);
		}


	void ConfigureAxle (VPAxle axle, int leftWheelIndex, int rightWheelIndex, TireFrictionBase tireFriction)
		{
		WheelState leftWheelState = wheelState[leftWheelIndex];
		WheelState rightWheelState = wheelState[rightWheelIndex];
		Wheel leftWheelBlock = wheels[leftWheelIndex];
		Wheel rightWheelBlock = wheels[rightWheelIndex];

		bool isSteeringAxle = axle.steeringMode != Steering.SteeringMode.Disabled;

		// Configure the mandatory data per wheel

		ConfigureWheelData(leftWheelState, leftWheelBlock, axle.leftWheel, tireFriction, isSteeringAxle);
		ConfigureWheelData(rightWheelState, rightWheelBlock, axle.rightWheel, tireFriction, isSteeringAxle);

		// Configure steering

		if (isSteeringAxle)
			{
			m_steering.AddWheel(leftWheelState, GetWheelLocalPosition(axle.leftWheel), axle.steeringMode, axle.steeringRatio);
			m_steering.AddWheel(rightWheelState, GetWheelLocalPosition(axle.rightWheel), axle.steeringMode, axle.steeringRatio);
			}
		}


	// WheelState and Wheel objects must be initialized with a minimum data per wheel

	void ConfigureWheelData (WheelState ws, Wheel wheel, VPWheelCollider wheelCol, TireFrictionBase tireFriction, bool steerable = false)
		{
		ws.wheelCol = wheelCol;
		ws.steerable = steerable;
		wheel.tireFriction = tireFriction;
		wheel.radius = wheelCol.radius;
		wheel.mass = wheelCol.mass;
		}


	// Compute the longitudinal position of an axle (local)

	float GetAxleLocalPosition (VPAxle axle)
		{
		return 0.5f * (GetWheelLocalPosition(axle.leftWheel).z + GetWheelLocalPosition(axle.rightWheel).z);
		}


	// Expose internal components

	public override object GetInternalObject (System.Type type)
		{
		// - Inertia exposed for visualization.
		// - Steering settings exposed for addon components to get the steering angle.

		if (type == typeof(Inertia))
			return m_inertia;
		else
		if (type == typeof(Steering.Settings))
			return steering;

		return null;
		}


	// Update section
	// ---------------------------------------------------------------------------------------------


 	// Read the standard input values and translate them to the internal blocks.
	// Also set the state values expected by each block.
	// Called before each integration step.

	protected override void DoUpdateBlocks ()
		{
		// Collect input and settings

		int[] inputData = data.Get(Channel.Input);
		int[] settingsData = data.Get(Channel.Settings);

		float brakePosition = Mathf.Clamp01(inputData[InputData.Brake] / 10000.0f);
		float handbrakePosition = Mathf.Clamp01(inputData[InputData.Handbrake] / 10000.0f);
		float throttlePosition = Mathf.Clamp01(inputData[InputData.Throttle] / 10000.0f);
		float steerPosition = Mathf.Clamp(inputData[InputData.Steer] / 10000.0f, -1.0f, 1.0f);

		int automaticGearInput = inputData[InputData.AutomaticGear];
		int ignitionInput = inputData[InputData.Key];

		// Process gear mode preventing direction changes when the vehicle is not stopped

		m_gearMode = Mathf.Clamp(automaticGearInput, (int)Gearbox.AutomaticGear.R, (int)Gearbox.AutomaticGear.D);

		if (m_gearMode != m_prevGearMode)
			{
			if (m_gearMode == (int)Gearbox.AutomaticGear.D && speed < -gearChangeMaxSpeed
				|| m_gearMode == (int)Gearbox.AutomaticGear.R && speed > gearChangeMaxSpeed)
				{
				m_gearMode = m_prevGearMode;
				}
			else
				{
				m_prevGearMode = m_gearMode;
				}
			}

		// No throttle if the vehicle is switched off.

		if (ignitionInput < 0)
			throttlePosition = 0.0f;
		else
			throttlePosition = SpeedControl.GetThrottle(speedControl, inputData, data.Get(Channel.Vehicle));

		// Process inputs
		// Input settings are configured in the car independently of the torque maps.
		// Being in a separate class allows all intermediate steps to be traced separately
		// (pedal > input > electrical torque > mechanical torque > wheel torque)

		m_gear = m_gearMode - (int)Gearbox.AutomaticGear.N;
		m_throttleInput = input.GetThrottleInput(throttlePosition);
		m_brakePressure = input.GetBrakePressure(brakePosition);
		if (m_brakePressure > brakePressureThreshold) m_throttleInput = 0.0f;

		m_frontPowertrain.SetInputs(m_gear, m_throttleInput, m_brakePressure);
		m_rearPowertrain.SetInputs(m_gear, m_throttleInput, m_brakePressure);

		// Traction control

		if (m_gear != 0)
			{
			// Independent traction control per axle
			/*
			int tcsOverride = settingsData[SettingsData.TcsOverride];
			if (tractionControl.enabled && tcsOverride != 2 || tcsOverride == 1)
				{
				float frontFinalRatio = frontDifferential.gearRatio * Mathf.Sign(gear);
				float rearFinalRatio = rearDifferential.gearRatio * Mathf.Sign(gear);
				float maxFrontRpm = TractionControl.GetMaxEngineRpm(this, tractionControl, 0, 1, frontFinalRatio, maxDriveRpm);
				float maxRearRpm = TractionControl.GetMaxEngineRpm(this, tractionControl, 2, 3, rearFinalRatio, maxDriveRpm);

				// TO-DO: electric motors
				// m_frontPowertrain.directDrive.maxRpm = maxFrontRpm;
				// m_rearPowertrain.directDrive.maxRpm = maxRearRpm;
				}
			*/
			// Apply motor input
			/*
			float motorInput = throttlePosition * Mathf.Sign(gear);
			m_rearPowertrain.electricMotor.motorInput = motorInput * frontToRearBalance;
			m_frontPowertrain.electricMotor.motorInput = motorInput * (1.0f - frontToRearBalance);
			*/
			}

		// Steering

		if (settingsData[SettingsData.SteeringAidsOverride] != 2)
			SteeringAids.Apply(this, steering, steeringAids, ref steerPosition);
		m_steering.steerInput = steerPosition;
		m_steering.DoUpdate();

		// Track changes in the inertia settings

		m_inertia.DoUpdate(cachedRigidbody);

		// Differential overrides

		m_frontPowertrain.differentialOverride = (Powertrain.DifferentialOverride)settingsData[SettingsData.DifferentialLock];
		m_rearPowertrain.differentialOverride = (Powertrain.DifferentialOverride)settingsData[SettingsData.DifferentialLock];
		}


	protected override void DoUpdateData ()
		{
		int[] inputData = data.Get(Channel.Input);
		int[] vehicleData = data.Get(Channel.Vehicle);

		vehicleData[VehicleData.Speed] = (int)(speed * 1000.0f);	// speed inherited from VehicleBase

		// Powertrain

		int ignitionKey = inputData[InputData.Key];

		vehicleData[VehicleData.EngineStalled] = 0;
		vehicleData[VehicleData.EngineStarting] = 0;
		vehicleData[VehicleData.EngineWorking] = ignitionKey < 0? 0 : 1;

		vehicleData[VehicleData.GearboxMode] = m_gearMode;
		vehicleData[VehicleData.GearboxGear] = m_gear;
		vehicleData[VehicleData.ClutchLock] = m_gearMode == (int)Gearbox.AutomaticGear.N? 0 : 1000;

		// Engine rpm: maximum of both motors

		float engineRpm = MathUtility.MaxAbs(m_frontPowertrain.mgu.sensorRpm, m_rearPowertrain.mgu.sensorRpm);
		vehicleData[VehicleData.EngineRpm] = (int)(engineRpm * 1000.0f);

		// Engine torque: sum of both motors
		// The resulting maximum should be the configured maximum

		float engineTorque = m_frontPowertrain.mgu.sensorRotorTorque + m_rearPowertrain.mgu.sensorRotorTorque;
		vehicleData[VehicleData.EngineTorque] = (int)(engineTorque * 1000.0f);

		// Engine Power: sum of the powers of both motors

		float enginePower = m_frontPowertrain.mgu.sensorElectricalPower + m_rearPowertrain.mgu.sensorElectricalPower;
		vehicleData[VehicleData.EnginePower] = (int)(enginePower * 1000.0f);

        // Engine Load: average of the loads of both motors
		// Negative means regenerative braking.

		float engineLoad = (m_frontPowertrain.mgu.sensorLoad + m_rearPowertrain.mgu.sensorLoad) * 0.5f;
		vehicleData[VehicleData.EngineLoad] = (int)(engineLoad * 1000.0f);

		// Driving aids

		// TODO: Implement TC with electric motors.
		// bool tcsEngaged =
			// m_frontPowertrain.directDrive.sensorLoad < m_frontPowertrain.directDrive.motorInput && m_frontPowertrain.directDrive.maxRpm < maxDriveRpm
			// || m_rearPowertrain.directDrive.sensorLoad < m_rearPowertrain.directDrive.motorInput && m_rearPowertrain.directDrive.maxRpm < maxDriveRpm;
		bool tcsEngaged = false;
		vehicleData[VehicleData.TcsEngaged] = tcsEngaged ? 1 : 0;

		// Aided steering must be stored separately from the input vector for preventing
		// the steering aids system to feed itself.

		vehicleData[VehicleData.AidedSteer] = (int)(m_steering.steerInput * 10000.0f);

		// Update the vehicle input channel with the feedback of the blocks.
		// They may force the state of actual input elements, as gearbox stick / lever.

		inputData[InputData.AutomaticGear] = m_gearMode;

		// Fill the custom data bus

		int[] customData = data.Get(Channel.Custom);
		customData[Perrinn424Data.ThrottleInput] = (int)(m_throttleInput * 1000.0f);
		customData[Perrinn424Data.BrakePressure] = (int)(m_brakePressure * 1000.0f);

		m_frontPowertrain.FillDataBus(customData, Perrinn424Data.FrontMguBase);
		m_rearPowertrain.FillDataBus(customData, Perrinn424Data.RearMguBase);

		// Axle values

		if (frontAxleReference != null)
			{
			m_frontAxleSensor.DoUpdate(frontAxleReference);
			customData[Perrinn424Data.FrontRideHeight] = (int)(m_frontAxleSensor.GetRideHeight() * 1000.0f);
			customData[Perrinn424Data.FrontRollAngle] = (int)(m_frontAxleSensor.GetRollAngle() * 1000.0f);
			}

		if (rearAxleReference != null)
			{
			m_rearAxleSensor.DoUpdate(rearAxleReference);
			customData[Perrinn424Data.RearRideHeight] = (int)(m_rearAxleSensor.GetRideHeight() * 1000.0f);
			customData[Perrinn424Data.RearRollAngle] = (int)(m_rearAxleSensor.GetRollAngle() * 1000.0f);
			}
		}


	// This is a convenient addition to the [DrawGizmo] decorator in ElectricVehicleControllerInspector.
	// The OnDrawGizmos method makes the component appear at the Scene view's Gizmos dropdown menu,
	// Also causes the gizmo to be hidden if the component inspector is collapsed even in
	// GizmoType.NonSelected mode.

	void OnDrawGizmos ()
		{
		}
	}
