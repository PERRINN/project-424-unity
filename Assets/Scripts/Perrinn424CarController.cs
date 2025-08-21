//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2025 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

// Vehicle controller for the Perrinn 424 electric racing car.


using UnityEngine;
using EdyCommonTools;
using VehiclePhysics;


// Assignments of the custom channel for the 424 in the data bus

public struct Perrinn424Data					// ID			DESCRIPTION							UNITS		RESOLUTION		EXAMPLE
	{
	public const int ThrottlePosition			= 0;		// Throttle pedal position				ratio		1000			1000 = 1.0 = 100%
	public const int BrakePosition				= 1;		// Brake Pedal Position					ratio		1000			1000 = 1.0 = 100%
	public const int SteeringWheelAngle			= 2;		// Angle in the steering column			deg			1000			12420 = 12.42 degrees
	public const int EngagedGear				= 3;		// Engaged gear 						-			-				-1 Reverse, 0 Neutral, 1 Drive
	public const int DrsPosition				= 4;		// DRS position. 0 = closed, 1 = open	%			1000			1000 = 1.0 = 100% open

	public const int FrontDiffFriction			= 5;		// Front differential friction			Nm			1000			50000 = 50 Nm
	public const int RearDiffFriction			= 6;		// Rear differential friction			Nm			1000			50000 = 50 Nm

	public const int FrontRideHeight			= 10;		// Front ride height					m			1000			230 = 0.23 m = 230 mm
	public const int FrontRollAngle				= 11;		// Front roll angle (signed)			deg			1000			2334 = 2.345 degrees
	public const int RearRideHeight				= 12;		// Rear ride height						m			1000			230 = 0.23 m = 230 mm
	public const int RearRollAngle				= 13;		// Rear roll angle (signed)				deg			1000			2334 = 2.345 degrees
	public const int GroundAngle				= 14;		// Road angle (positive upwards)		deg			1000			2334 = 2.345 degrees
	public const int GroundSlope				= 15;		// Road grade (positive upwards)		%			1000			1000 = 1.0 = 100%

	public const int UndersteerAngle			= 16;		// Understeer angle (positive = oversteer)	deg		1000			-3.567 = 3.567 degrees of oversteer

	// MGU data. Combine base ID with values.

	public const int FrontMguBase				= 20;		// Base ID for front MGU data
	public const int RearMguBase				= 40;		// Base ID for rear MGU data

	public const int Rpm						= 0;		// Motor rpm							rpm			1000			1200000 = 1200 rpm
	public const int Load						= 1;		// Motor load. Negative = renerative	ratio		1000			900 = 0.9 = 90%
	public const int PowerBalanceFeedForward	= 2;		// Power balance from torque map		ratio		1000			333 = 0.333 = 33.3%
	public const int PowerBalance				= 3;		// Final power balance (incl. offset)	ratio		1000			333 = 0.333 = 33.3%
	public const int Efficiency					= 4;		// Efficiency							ratio		1000			945 = 0.945
	public const int ElectricalPower			= 5;		// Electrical power consumed			kW			1000			250000 = 250 kW
	public const int MguPower					= 6;		// Mgu power provided					kW			1000			250000 = 250 kW
	public const int MguTorque					= 7;		// Mgu torque provided					Nm			1000			50000 = 50 Nm
	public const int MguStatorTorque			= 8;		// Pre-inertia torque					Nm			1000			55000 = 55 Nm
	public const int MguRotorTorque				= 9;		// Final torque in the mgu rotor		Nm			1000			50600 = 50.6 Nm
	public const int ShaftsTorque				= 10;		// Sum of torques at drive shafts		Nm			1000			150600 = 150.6 Nm
	public const int WheelsTorque				= 11;		// Sum of torques at wheels				Nm			1000			150600 = 150.6 Nm

	// Processed input data. Used by autopilot / automation.

	public const int EnableProcessedInput		= 60;		// If non-zero, use the processed input data below. Otherwise, use standard Input channel.

	public const int InputThrottlePosition		= 51;		// Throttle pedal position				ratio		10000			5000 = 0.5 = 50%
	public const int InputBrakePosition			= 52;		// Brake pedal position					ratio		10000			5000 = 0.5 = 50%
	public const int InputSteerAngle			= 53;		// Steer angle for the steering column	deg			10000			155000 = 15.5 degrees
	public const int InputGear					= 54;		// Gear (forward / neutral / reverse)				0 = Neutral, 1 = Forward, -1 = Reverse
	public const int InputDrsPosition			= 55;		// DRS position. 0 = closed, 1 = open	%			1000			1000 = 1.0 = 100% open
	public const int InputLiftAndCoast			= 56;		// Lift and coast State								0 = Disabled, 1 = Enabled
	}


// Custom car controller for Perrinn 424

public class Perrinn424CarController : VehicleBase
	{
	public Inertia.Settings inertia = new Inertia.Settings();

	public VPAxle frontAxle;
	public VPAxle rearAxle;

	public TireDataContainerBase frontTires;
	public TireDataContainerBase rearTires;

	public GroundTracker.Settings groundTracking = new GroundTracker.Settings();

	// Powertrain and dynamics

	public DualMguTorqueMapContainerBase torqueMap;
	[Range(0,1)]
	public float reverseGearLimiter = 0.05f;

	public Differential.Settings frontDifferential = new Differential.Settings();
	public Differential.Settings rearDifferential = new Differential.Settings();

	public Steering.Settings steering = new Steering.Settings();

	// Driving aids

	public SteeringAids.Settings steeringAids = new SteeringAids.Settings();
	public SpeedControl.Settings speedControl = new SpeedControl.Settings();

	// Safety aids

	public TractionControl.Settings tractionControl = new TractionControl.Settings();

	// Other

	public bool allowLiftAndCoastOnAutopilot = false;

	// Speed / power limiter applied externally

	[System.NonSerialized]
	public float mguLimiter = 1.0f;

	// Power balance offset applied externally

	[System.NonSerialized]
	public float powerBalanceOffset = 0.0f;

	// Regen power gain applied externally

	[System.NonSerialized]
	public float regenPowerGain = 1.0f;

	// Mostly internal settings not exposed in the inspector

	[System.NonSerialized]
	public float gearChangeMaxSpeed = 1.0f;

	// Motor input is ignored when brake position is beyond this value

	[System.NonSerialized]
	public float brakePressureThreshold = 0.03f;

	// Internal values exposed

	public float throttlePosition => m_throttlePosition;
	public float brakePosition => m_brakePosition;
	public float steerAngle => m_steerAngle;
	public int gear => m_gear;

	public float understeerAngle => m_understeerAngle;


	// Private members

	Powertrain m_frontPowertrain;
	Powertrain m_rearPowertrain;

	Inertia m_inertia;
	Steering m_steering;

	float m_throttlePosition;
	float m_brakePosition;
	float m_steerAngle;
	int m_gearMode;
	int m_prevGearMode;
	int m_gear;
	float m_understeerAngle;
	bool m_drivingStarted;

	GroundTracker m_groundTracker = new GroundTracker();


	// Internal Powertrain helper class

	class Powertrain
		{
		public MguCore mgu;
		public Differential differential;

		Wheel m_leftWheel;
		Wheel m_rightWheel;


		public Powertrain (Wheel leftWheelBlock, Wheel rightWheelBlock)
			{
			mgu = new MguCore();
			differential = new Differential();

			Block.Connect(leftWheelBlock, differential, 0);
			Block.Connect(rightWheelBlock, differential, 1);
			Block.Connect(differential, mgu);

			m_leftWheel = leftWheelBlock;
			m_rightWheel = rightWheelBlock;
			}


		public void SetInputs (int gearInput, float throttleInput, float brakeInput, float limiter, float powerBalanceOffset, float regenPowerGain)
			{
			// MGU

			mgu.gearInput = gearInput;
			mgu.throttleInput = throttleInput;
			mgu.brakeInput = brakeInput;
			mgu.limiterInput = Mathf.Clamp01(limiter);
			mgu.powerBalanceOffset = powerBalanceOffset;
			mgu.regenPowerGain = regenPowerGain;

			// Wheel brakes

			float brakeTorque = mgu.GetHydraulicTorquePerWheel();
			m_leftWheel.AddBrakeTorque(brakeTorque);
			m_rightWheel.AddBrakeTorque(brakeTorque);
			}


		public void FillDataBus (int[] channel, int baseId)
			{
			channel[baseId + Perrinn424Data.Rpm] = (int)(mgu.sensorRpm * 1000);
			channel[baseId + Perrinn424Data.Load] = (int)(mgu.sensorLoad * 1000);
			channel[baseId + Perrinn424Data.PowerBalanceFeedForward] = (int)(mgu.sensorPowerBalanceFeedForward * 1000);
			channel[baseId + Perrinn424Data.PowerBalance] = (int)(mgu.sensorPowerBalance * 1000);
			channel[baseId + Perrinn424Data.Efficiency] = (int)(mgu.sensorEfficiency * 1000);
			channel[baseId + Perrinn424Data.ElectricalPower] = (int)(mgu.sensorElectricalPower);
			channel[baseId + Perrinn424Data.MguPower] = (int)(mgu.sensorMguPower);
			channel[baseId + Perrinn424Data.MguTorque] = (int)(mgu.sensorMguTorque * 1000);
			channel[baseId + Perrinn424Data.MguStatorTorque] = (int)(mgu.sensorStatorTorque * 1000);
			channel[baseId + Perrinn424Data.MguRotorTorque] = (int)(mgu.sensorRotorTorque * 1000);
			channel[baseId + Perrinn424Data.ShaftsTorque] = (int)((m_leftWheel.sensorDriveTorque + m_rightWheel.sensorDriveTorque) * 1000);

			float leftWheelTorque = m_leftWheel.sensorDriveTorque + m_leftWheel.sensorBrakeTorque * Mathf.Sign(-m_leftWheel.contactPatch.localVelocity.y);
			float rightWheelTorque = m_rightWheel.sensorDriveTorque + m_rightWheel.sensorBrakeTorque * Mathf.Sign(-m_rightWheel.contactPatch.localVelocity.y);
			channel[baseId + Perrinn424Data.WheelsTorque] = (int)((leftWheelTorque + rightWheelTorque) * 1000);
			}


		public string GetDebutStr ()
			{
			float power = mgu.sensorRpm * Block.RpmToW * mgu.sensorMguTorque;
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
			DebugLogError("Some WheelColliderBehaviour references are missing in the axles.\nAll axles must have a reference to the corresponding left-right WheelColliderBehaviour objects.");
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

		// This removes one redundant tire friction attenuation. No significant difference though.

		hasRadialTires = true;

		// We now have the inherited properties wheels[wheelCount] and wheelsState[wheelCount].
		// Configure the wheels in the axles accordingly:
		//
		//	- Mandatory data
		//	- Steering
		//	- Brakes

		ConfigureAxle(frontAxle, 0, 1, frontTires.GetTireFriction());
		ConfigureAxle(rearAxle, 2, 3, rearTires.GetTireFriction());

		// Configure an independent powertrain per axle.
		// Use a default torque map unset.

		if (torqueMap == null)
			DebugLogWarning("Torque maps not set - using default.");

		DualMguTorqueMapBase dualTorqueMap = torqueMap != null? torqueMap.GetDualMguTorqueMap() : new DualMguTorqueMap();

        m_frontPowertrain = new Powertrain(wheels[0], wheels[1]);
		m_frontPowertrain.mgu.provider = dualTorqueMap.TorqueProviderFront();
		m_frontPowertrain.differential.settings = frontDifferential;

        m_rearPowertrain = new Powertrain(wheels[2], wheels[3]);
		m_rearPowertrain.mgu.provider = dualTorqueMap.TorqueProviderRear();
		m_rearPowertrain.differential.settings = rearDifferential;

		// Configure ground tracking

		m_groundTracker.settings = groundTracking;

		// Initialize internal data

		m_gearMode = (int)Gearbox.AutomaticGear.N;
		m_prevGearMode = (int)Gearbox.AutomaticGear.N;
		data.Set(Channel.Input, InputData.AutomaticGear, m_gearMode);
		m_drivingStarted = false;
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

	void ConfigureWheelData (WheelState ws, Wheel wheel, WheelColliderBehaviour wheelCol, TireFrictionBase tireFriction, bool steerable = false)
		{
		ws.wheelCol = wheelCol;
		ws.steerable = steerable;
		wheel.tireFriction = tireFriction;
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


	// Compute understeer-oversteer

	void ComputeUndersteerAngle ()
		{
		// We define understeer as the difference between absolute front slip angle (average both wheels) and abolute rear slip angle (average both wheels).
		// If positive then undesteer, if negative then oversteer.

		float absFrontSlipAngle = GetAbsSlipAngle(wheelState[0], wheelState[1]);
		float absRearSlipAngle = GetAbsSlipAngle(wheelState[2], wheelState[3]);
		m_understeerAngle = absFrontSlipAngle - absRearSlipAngle;
		}


	float GetAbsSlipAngle (VehicleBase.WheelState leftWheel, VehicleBase.WheelState rightWheel)
		{
		if (leftWheel.grounded && rightWheel.grounded)
			return MathUtility.FastAbs(0.5f * (leftWheel.slipAngle + rightWheel.slipAngle));
		else
		if (leftWheel.grounded)
			return MathUtility.FastAbs(leftWheel.slipAngle);
		else
		if (rightWheel.grounded)
			return MathUtility.FastAbs(rightWheel.slipAngle);
		else
			return float.NaN;
		}


	// Update section
	// ---------------------------------------------------------------------------------------------


 	// Read the standard input values and translate them to the internal blocks.
	// Also set the state values expected by each block.
	// Called before each integration step.

	protected override void DoUpdateBlocks ()
		{
		// Shortcuts to the data bus channels

		int[] inputData = data.Get(Channel.Input);
		int[] settingsData = data.Get(Channel.Settings);
		int[] customData = data.Get(Channel.Custom);

		// Retrieve processed inputs instead of standard inputs if specified

		bool processedInputs = customData[Perrinn424Data.EnableProcessedInput] != 0;
		if (processedInputs)
			{
			// Gear. Implies GearMode.

			m_gear = Mathf.Clamp(customData[Perrinn424Data.InputGear], -1, 1);
			m_gearMode = m_gear + (int)Gearbox.AutomaticGear.N;

			// Mgu throttle and brake pressure

			m_throttlePosition = Mathf.Clamp01(customData[Perrinn424Data.InputThrottlePosition] / 10000.0f);
			m_brakePosition = customData[Perrinn424Data.InputBrakePosition] / 10000.0f;

			// Steering angle to steer position

			float steeringHalfRange = steering.steeringWheelRange;
			m_steerAngle = Mathf.Clamp(customData[Perrinn424Data.InputSteerAngle] / 10000.0f, -steeringHalfRange, steeringHalfRange);

			// Lift and Coast. Will be read directly from the bus.
			// It may also be triggered externally when the inputs are processed.

			if (!allowLiftAndCoastOnAutopilot || customData[Perrinn424Data.InputLiftAndCoast] > 0)
				inputData[InputData.Retarder] = customData[Perrinn424Data.InputLiftAndCoast];

			// Assume driving not started so disabling autopilot engages brakes

			m_drivingStarted = false;
			}
		else
			{
			// Retrieve inputs from the standard Input channel

			float throttlePosition = Mathf.Clamp01(inputData[InputData.Throttle] / 10000.0f);
			float brakePosition = Mathf.Clamp01(inputData[InputData.Brake] / 10000.0f);
			float steerPosition = Mathf.Clamp(inputData[InputData.Steer] / 10000.0f, -1.0f, 1.0f);
			int automaticGearInput = inputData[InputData.AutomaticGear];

			// Car automatically applies brakes on startup until any pedal is pressed beyond 10%.
			// 50% brakes is more than enough to stop the car in place.

			if (!m_drivingStarted)
				{
				if (brakePosition > 0.1f || throttlePosition > 0.1f)
					m_drivingStarted = true;
				else
					brakePosition = 0.5f;
				}

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

			m_gear = m_gearMode - (int)Gearbox.AutomaticGear.N;

			// Process inputs

			// Throttle: apply speed limit / cruise control
			throttlePosition = SpeedControl.GetThrottle(speedControl, inputData, data.Get(Channel.Vehicle));

			m_throttlePosition = throttlePosition;
			m_brakePosition = brakePosition;

			// Process steering

			if (settingsData[SettingsData.SteeringAidsOverride] != 2)
				SteeringAids.Apply(this, steering, steeringAids, ref steerPosition);
			m_steerAngle = steerPosition * steering.steeringWheelRange * 0.5f;
			}

		bool brakePressed = m_brakePosition > brakePressureThreshold;

		// No throttle in any case if the vehicle is turned off

		int ignitionInput = inputData[InputData.Key];
		if (ignitionInput < 0)
			m_throttlePosition = 0.0f;

		// Limiter and lift-and-coast feature

		if (brakePressed || m_gear != 1 || ignitionInput < 0)
			inputData[InputData.Retarder] = 0;

		float effectiveLimiter = mguLimiter;
		if (m_gear < 0)
			effectiveLimiter = Mathf.Min(mguLimiter, reverseGearLimiter);
		else
		if (inputData[InputData.Retarder] != 0)
			effectiveLimiter = 0.0f;

		// Apply received inputs to car elements

		if (brakePressed) m_throttlePosition = 0.0f;
		m_frontPowertrain.SetInputs(m_gear, m_throttlePosition, m_brakePosition, effectiveLimiter, powerBalanceOffset, regenPowerGain);
		m_rearPowertrain.SetInputs(m_gear, m_throttlePosition, m_brakePosition, effectiveLimiter, powerBalanceOffset, regenPowerGain);

		m_steering.steerInput = m_steerAngle / steering.steeringWheelRange * 2.0f;
		m_steering.DoUpdate();

		// Traction control (TO-DO)

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

		// Update inertia settings and ground tracking

		m_inertia.DoUpdate(cachedRigidbody);
		m_groundTracker.DoUpdate(cachedTransform);

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

		// Engine Power: sum of the electrical powers of both motors

		float enginePower = m_frontPowertrain.mgu.sensorElectricalPower + m_rearPowertrain.mgu.sensorElectricalPower;
		vehicleData[VehicleData.EnginePower] = (int)(enginePower);

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
		customData[Perrinn424Data.ThrottlePosition] = (int)(m_throttlePosition * 1000.0f);
		customData[Perrinn424Data.BrakePosition] = (int)(m_brakePosition * 1000.0f);
		customData[Perrinn424Data.SteeringWheelAngle] = (int)(m_steerAngle * 1000.0f);
		customData[Perrinn424Data.EngagedGear] = m_gear;
		customData[Perrinn424Data.FrontDiffFriction] = (int)(m_frontPowertrain.differential.frictionTorque * 1000.0f);
		customData[Perrinn424Data.RearDiffFriction] = (int)(m_rearPowertrain.differential.frictionTorque * 1000.0f);

		m_frontPowertrain.FillDataBus(customData, Perrinn424Data.FrontMguBase);
		m_rearPowertrain.FillDataBus(customData, Perrinn424Data.RearMguBase);

		customData[Perrinn424Data.FrontRideHeight] = (int)(m_groundTracker.frontRideHeight * 1000.0f);
		customData[Perrinn424Data.RearRideHeight] = (int)(m_groundTracker.rearRideHeight * 1000.0f);
		customData[Perrinn424Data.FrontRollAngle] = (int)(m_groundTracker.frontRollAngle * 1000.0f);
		customData[Perrinn424Data.RearRollAngle] = (int)(m_groundTracker.rearRollAngle * 1000.0f);
		customData[Perrinn424Data.GroundAngle] = (int)(m_groundTracker.groundAngle * 1000.0f);
		customData[Perrinn424Data.GroundSlope] = (int)(m_groundTracker.groundSlope * 1000.0f);

		// Understeer

		ComputeUndersteerAngle();
		customData[Perrinn424Data.UndersteerAngle] = (int)(m_understeerAngle * 1000.0f);
		}


	// This is a convenient addition to the [DrawGizmo] decorator in ElectricVehicleControllerInspector.
	// The OnDrawGizmos method makes the component appear at the Scene view's Gizmos dropdown menu,
	// Also causes the gizmo to be hidden if the component inspector is collapsed even in
	// GizmoType.NonSelected mode.

	void OnDrawGizmos ()
		{
		}
	}
