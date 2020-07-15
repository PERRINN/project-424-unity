//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

// Vehicle controller for the Perrinn 424 electric racing car.


using UnityEngine;
using EdyCommonTools;
using VehiclePhysics;


public class Perrinn424CarController : VehicleBase
	{
	public Inertia.Settings inertia = new Inertia.Settings();

	public VPAxle frontAxle;
	public VPAxle rearAxle;

	// Powertrain and dynamics

	public float maxDriveTorque = 600.0f;
	public float maxDriveRpm = 2000.0f;
	[Range(0,1)]
	public float frontToRearBalance = 0.5f;		// 0 = front, 1 = rear
	[UnityEngine.Serialization.FormerlySerializedAs("differential")]
	public Differential.Settings frontDifferential = new Differential.Settings();
	public Differential.Settings rearDifferential = new Differential.Settings();

	public Steering.Settings steering = new Steering.Settings();
	public Brakes.Settings brakes = new Brakes.Settings();

	public TireDataContainerBase frontTires;
	public TireDataContainerBase rearTires;

	public ElectricMotor.Settings electricMotor = new ElectricMotor.Settings();

	// Driving aids

	public SteeringAids.Settings steeringAids = new SteeringAids.Settings();
	public SpeedControl.Settings speedControl = new SpeedControl.Settings();

	// Safety aids

	public Brakes.AbsSettings antiLock = new Brakes.AbsSettings();
	public TractionControl.Settings tractionControl = new TractionControl.Settings();
	public StabilityControl.Settings stabilityControl = new StabilityControl.Settings();
	public AntiSpin.Settings antiSpin = new AntiSpin.Settings();

	// Mostly internal settings not exposed in the inspector

	[System.NonSerialized]
	public float gearChangeMaxSpeed = 1.0f;

	// Private members

	Inertia m_inertia;
	Steering m_steering;
	Brakes m_brakes;

	StabilityControl m_stabilityControl;
	AntiSpin m_frontAntiSpin;
	AntiSpin m_rearAntiSpin;

	int m_gearMode;
	int m_prevGearMode;


	// Internal Powertrain helper class

	class Powertrain
		{
		public ElectricMotor electricMotor;
		public Differential differential;


		public Powertrain (Wheel leftWheelBlock, Wheel rightWheelBlock)
			{
			electricMotor = new ElectricMotor();
			differential = new Differential();

			Block.Connect(leftWheelBlock, differential, 0);
			Block.Connect(rightWheelBlock, differential, 1);
			Block.Connect(differential, electricMotor);
			}


		// Overrides to the differential and driveline settings
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

	Powertrain m_frontPowertrain;
	Powertrain m_rearPowertrain;


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
		// Prepare the internal helpers: inertia, steering, brakes

		m_inertia = new Inertia();
		m_inertia.settings = inertia;
		m_inertia.Apply(cachedRigidbody);

		m_steering = new Steering();
		m_steering.settings = steering;

		m_brakes = new Brakes();
		m_brakes.settings = brakes;
		m_brakes.absSettings = antiLock;

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
		m_frontPowertrain.electricMotor.settings = electricMotor;
		m_frontPowertrain.differential.settings = frontDifferential;

        m_rearPowertrain = new Powertrain(wheels[2], wheels[3]);
		m_rearPowertrain.electricMotor.settings = electricMotor;
		m_rearPowertrain.differential.settings = rearDifferential;

		// Initialize driving aids

		m_stabilityControl = new StabilityControl();
		m_stabilityControl.settings = stabilityControl;

		m_frontAntiSpin = new AntiSpin();
		m_rearAntiSpin = new AntiSpin();
		m_frontAntiSpin.settings = antiSpin;
		m_rearAntiSpin.settings = antiSpin;

		// Stability control requires knowing the wheelbase

		m_stabilityControl.wheelbase = Mathf.Abs(GetAxleLocalPosition(frontAxle) - GetAxleLocalPosition(rearAxle));

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

		// Configure brakes

		m_brakes.AddWheel(leftWheelState, leftWheelBlock, axle.brakeCircuit, Brakes.LateralPosition.Left);
		m_brakes.AddWheel(rightWheelState, rightWheelBlock, axle.brakeCircuit, Brakes.LateralPosition.Right);
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

		float brakeInput = Mathf.Clamp01(inputData[InputData.Brake] / 10000.0f);
		float handbrakeInput = Mathf.Clamp01(inputData[InputData.Handbrake] / 10000.0f);
		float throttleInput = Mathf.Clamp01(inputData[InputData.Throttle] / 10000.0f);
		float steerInput = Mathf.Clamp(inputData[InputData.Steer] / 10000.0f, -1.0f, 1.0f);

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
			throttleInput = 0.0f;
		else
			throttleInput = SpeedControl.GetThrottle(speedControl, inputData, data.Get(Channel.Vehicle));

		// Electric motors

		int gear = m_gearMode - (int)Gearbox.AutomaticGear.N;

		if (gear != 0)
			{
			// Independent traction control per axle

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

			// Apply motor input

			float motorInput = throttleInput * Mathf.Sign(gear);
			m_rearPowertrain.electricMotor.motorInput = motorInput * frontToRearBalance;
			m_frontPowertrain.electricMotor.motorInput = motorInput * (1.0f - frontToRearBalance);
			}
		else
			{
			m_frontPowertrain.electricMotor.motorInput = 0.0f;
			m_rearPowertrain.electricMotor.motorInput = 0.0f;
			}

		// Steering

		if (settingsData[SettingsData.SteeringAidsOverride] != 2)
			SteeringAids.Apply(this, steering, steeringAids, ref steerInput);
		m_steering.steerInput = steerInput;
		m_steering.DoUpdate();

        // Stability Control (ECS) and Anti-Slip (ASR)
		// Applied before brakes as they trigger brake signals at the Brakes helper

		ApplyStabilityControl();
		ApplyAntiSpin(m_frontAntiSpin, 0, 1);
		ApplyAntiSpin(m_rearAntiSpin, 2, 3);

		// Brakes

		m_brakes.brakeInput = brakeInput;
		m_brakes.handbrakeInput = handbrakeInput;
		m_brakes.DoUpdate();

		// Track changes in the inertia settings

		m_inertia.DoUpdate(cachedRigidbody);

		// Implement supported values from the Settings channel

		m_brakes.absOverride = (Brakes.AbsOverride)settingsData[SettingsData.AbsOverride];
		m_stabilityControl.escOverride = (StabilityControl.Override)settingsData[SettingsData.EscOverride];
		m_frontAntiSpin.asrOverride = (AntiSpin.Override)settingsData[SettingsData.AsrOverride];
		m_rearAntiSpin.asrOverride = (AntiSpin.Override)settingsData[SettingsData.AsrOverride];

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
		vehicleData[VehicleData.GearboxGear] = m_gearMode - (int)Gearbox.AutomaticGear.N;
		vehicleData[VehicleData.ClutchLock] = m_gearMode == (int)Gearbox.AutomaticGear.N? 0 : 1000;

		// Engine rpm: maximum of both motors

		float engineRpm = MathUtility.MaxAbs(m_frontPowertrain.electricMotor.angularVelocity, m_rearPowertrain.electricMotor.angularVelocity) * Block.WToRpm;
		vehicleData[VehicleData.EngineRpm] = (int)(engineRpm * 1000.0f);

		// Engine torque: sum of both motors
		// The resulting maximum should be the configured maximum

		float engineTorque = m_frontPowertrain.electricMotor.torque + m_rearPowertrain.electricMotor.torque;
		vehicleData[VehicleData.EngineTorque] = (int)(engineTorque * 1000.0f);

		// Engine Power: sum of the powers of both motors

		float frontEnginePower = m_frontPowertrain.electricMotor.torque * m_frontPowertrain.electricMotor.angularVelocity;
		float rearEnginePower = m_rearPowertrain.electricMotor.torque * m_rearPowertrain.electricMotor.angularVelocity;
		vehicleData[VehicleData.EnginePower] = (int)(frontEnginePower + rearEnginePower);

        // Engine Load: sum of the loads of both motors
		// Negative means regenerative braking. Use average.

		float engineLoad = m_frontPowertrain.electricMotor.sensorLoad + m_rearPowertrain.electricMotor.sensorLoad;
		if (engineLoad < 0.0f) engineLoad *= 0.5f;
		vehicleData[VehicleData.EngineLoad] = (int)(engineLoad * 1000.0f);

		// Driving aids

		vehicleData[VehicleData.AbsEngaged] = m_brakes.sensorAbsEngaged? 1 : 0;
		vehicleData[VehicleData.EscEngaged] = m_stabilityControl.sensorEngaged? 1 : 0;
		vehicleData[VehicleData.AsrEngaged] = m_frontAntiSpin.sensorEngaged || m_rearAntiSpin.sensorEngaged? 1 : 0;

		// TO-DO: Implement with electric motors.
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
		}


 	// Helpers for safety aids


	void ApplyStabilityControl ()
		{
		m_stabilityControl.stateVehicleSpeed = speed;
		m_stabilityControl.stateVehicleSpeedAngle = speedAngle;
		m_stabilityControl.stateVehicleRotationRate = cachedRigidbody.angularVelocity.y;
		m_stabilityControl.stateVehicleSteeringAngle = steering.maxSteerAngle * m_steering.steerInput;
		m_stabilityControl.DoUpdate();

		// No need to apply further settings if disabled.
		// DoUpdate is invoked above to ensure sensors are cleared.

		if (!stabilityControl.enabled) return;

		m_brakes.AddBrakeRatio(m_stabilityControl.sensorBrakeFL, Brakes.BrakeCircuit.Front, Brakes.LateralPosition.Left);
		m_brakes.AddBrakeRatio(m_stabilityControl.sensorBrakeFR, Brakes.BrakeCircuit.Front, Brakes.LateralPosition.Right);
		m_brakes.AddBrakeRatio(m_stabilityControl.sensorBrakeRL, Brakes.BrakeCircuit.Rear, Brakes.LateralPosition.Left);
		m_brakes.AddBrakeRatio(m_stabilityControl.sensorBrakeRR, Brakes.BrakeCircuit.Rear, Brakes.LateralPosition.Right);

		// Cut drive power while ECS is stabilizing the vehicle

		float frontBrakes = Mathf.Max(m_stabilityControl.sensorBrakeFL, m_stabilityControl.sensorBrakeFR);
		float rearBrakes = Mathf.Max(m_stabilityControl.sensorBrakeRL, m_stabilityControl.sensorBrakeRR);
		float ecsBrakes = Mathf.Clamp01(Mathf.Max(frontBrakes, rearBrakes));

		if (ecsBrakes > 0.0f)
			{
			float ecsFactor = (1.0f - ecsBrakes);

			m_frontPowertrain.electricMotor.motorInput *= ecsFactor;
			m_rearPowertrain.electricMotor.motorInput *= ecsFactor;
			}
		}


	void ApplyAntiSpin (AntiSpin antiSpinInstance, int wheelIndexL, int wheelIndexR)
		{
		antiSpinInstance.stateVehicleSpeed = speed;
		antiSpinInstance.stateAngularVelocityL = wheelState[wheelIndexL].angularVelocity;
		antiSpinInstance.stateAngularVelocityR = wheelState[wheelIndexR].angularVelocity;
		antiSpinInstance.DoUpdate();

		// No need to apply further settings if disabled.
		// DoUpdate is invoked above to ensure sensors are cleared.

		if (!antiSpinInstance.settings.enabled) return;

		wheels[wheelIndexL].AddBrakeTorque(antiSpinInstance.sensorBrakeTorqueL);
		wheels[wheelIndexR].AddBrakeTorque(antiSpinInstance.sensorBrakeTorqueR);
		}


	// This is a convenient addition to the [DrawGizmo] decorator in ElectricVehicleControllerInspector.
	// The OnDrawGizmos method makes the component appear at the Scene view's Gizmos dropdown menu,
	// Also causes the gizmo to be hidden if the component inspector is collapsed even in
	// GizmoType.NonSelected mode.

	void OnDrawGizmos ()
		{
		}
	}
