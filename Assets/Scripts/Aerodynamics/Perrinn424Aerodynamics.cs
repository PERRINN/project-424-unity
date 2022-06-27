using UnityEngine;
using VehiclePhysics;
using System;
using Perrinn424.AerodynamicsSystem;


public class Perrinn424Aerodynamics : VehicleBehaviour
{
	public AltitudeConverter altitudeConverter;

	[System.Serializable]
	public class NoDRSarray
	{
		public float segmentStart = 0.0f;
		public float segmentEnd = 500.0f;
	}



	[Space(5)]
	public float deltaISA                  = 0.0f;
	public float dRSActivationDelay        = 0.0f;
	public float dRSActivationTime         = 0.0f;

	[Space(5)]
	[SerializeField] private NoDRSarray[] noDRSZone;

	[Space(5)]
	public float frontFlapStaticAngle         = 5.0f;
	public float frontFlapDRSAngle            = -15.0f;
	public float frontFlapSCz0				  = 0.34f;
	public float frontFlapSCz_perDeg          = 0.03f;

	//public float frontFlapFlexMaxDownforce    = 10000.0f;
	public float frontFlapDeflectionPreload   = 470.0f;
	public float frontFlapDeflectionStiffness = -0.006f;
	public float frontFlapDeflectionMax       = -5.0f;

	public float takeOffFrontRideHeight       = 70.0f;
	public float takeOffGain 						      = 0.26f;

	[Serializable]
	public class AeroSettings
	{
		public Transform applicationPoint;

		// Aerodynamic model coefficients
		public float constant                    = 1.0f;
		public float frontRideHeightCoefficient  = 1.0f;
		public float frontRideHeight2Coefficient = 1.0f;
		public float rearRideHeightCoefficient   = 1.0f;
		public float rearRideHeight2Coefficient  = 1.0f;
		public float absoluteYawCoefficient      = 1.0f;
		public float absoluteSteerCoefficient    = 1.0f;
		public float absoluteRollCoefficient     = 1.0f;
		public float dRS_Coefficient             = 1.0f;
		public float frontFlapCoefficient        = 1.0f;
	}

	[Space(5)]
	public AeroSettings front = new AeroSettings();
	public AeroSettings rear  = new AeroSettings();
	public AeroSettings drag  = new AeroSettings();

	[Space(5)]
	public bool emitTelemetry = false;

	[Header("Visual elements")]
	public Transform drsFlap;
	public float drsClosedAngle = 0.0f;
	public float drsOpenAngle = -90.0f;

	[Space(5)]
	public Transform frontFlap;
	public float frontFlapRestAngle = 0.0f;

	// Exposed state

	[HideInInspector] public float flapAngle   = 0.0f;
	[HideInInspector] public float flapDeflection = 0.0f;
	[HideInInspector] public float flapForce = 0.0f;
	[HideInInspector] public bool  DRSclosing  = false;
	[HideInInspector] public bool  DRSopenButton  = false;
	[HideInInspector] public float DRS      = 0;
	[HideInInspector] public float SCzFront = 0;
	[HideInInspector] public float SCzRear  = 0;
	[HideInInspector] public float SCx      = 0;
	[HideInInspector] public float downforceFront  = 0.0f;
	[HideInInspector] public float downforceRear   = 0.0f;
	[HideInInspector] public float aeroBal         = 0.0f;
	[HideInInspector] public float dragForce       = 0.0f;
	[HideInInspector] public float yawAngle        = 0.0f;
	[HideInInspector] public float steerAngle      = 0.0f;
	[HideInInspector] public float rollAngle       = 0.0f;
	[HideInInspector] public float fronRollAngle   = 0.0f;
	[HideInInspector] public float rearRollAngle   = 0.0f;
	[HideInInspector] public float frontRideHeight = 0.0f;
	[HideInInspector] public float rearRideHeight  = 0.0f;
	[HideInInspector] public float rho = 0.0f;
	[HideInInspector] public float noDRSzone = 0.0f;

	// Private members

	Atmosphere atmosphere = new Atmosphere();
	float DRStime = 0;

	// Function Name: noDRSZone
	// Check if car is inside a no DRS activation zone
	//
	//	 [IN]	lapDistance [m]
	//
	//	 [OUT]	true (DRS disabled) or false (DRS enabled)
	bool isNoDRSZone(float lapDistance)
	{
		for (int i = 0; i < noDRSZone.Length; i++)
		{
			if (lapDistance > noDRSZone[i].segmentStart && lapDistance < noDRSZone[i].segmentEnd)
				return true;
        }
		return false;
    }

	// Function Name: CalcAeroCoeff
	// This function calculates a given aerodynamic coefficient based on:
	//
	//	 [IN]	aeroSetting [AeroSettings]
	//	 [IN]	fRH_mm [mm]
	//	 [IN]	rRH_mm [mm]
	//	 [IN]	yawAngle_deg [deg]
	//	 [IN]	steerAngle_deg [deg]
	//	 [IN]	rollAngle_deg [deg]
	//	 [IN]	DRS [-]
	//	 [IN]	flapAngle_deg [deg]
	//
	//	 [OUT]	SCn [m2]

	float CalcAeroCoeff(AeroSettings aeroSetting, float fRH_mm, float rRH_mm, float yawAngle_deg, float steerAngle_deg, float rollAngle_deg, float DRSpos, float flapAngle_deg)
	{
		// Assigning return variable
		float SCn;

		// Checking limits before calculating forces
		fRH_mm = Mathf.Clamp(fRH_mm, 0, 200);
		rRH_mm = Mathf.Clamp(rRH_mm, 0, 200);
		DRSpos = Mathf.Clamp(DRSpos, 0, 1);
		flapAngle_deg = Mathf.Clamp(flapAngle_deg, -15, 15);
		yawAngle_deg = Mathf.Clamp(Math.Abs(yawAngle_deg), 0, 10);
		steerAngle_deg = Mathf.Clamp(Math.Abs(steerAngle_deg), 0, 20);
		rollAngle_deg = Mathf.Clamp(Math.Abs(rollAngle_deg), 0, 3);

		// fRH modifier for take off map
		fRH_mm = Mathf.Min(fRH_mm, ( fRH_mm - takeOffFrontRideHeight ) * takeOffGain + takeOffFrontRideHeight );

		// Calculate total force
		SCn = aeroSetting.constant +
			  aeroSetting.frontRideHeightCoefficient * fRH_mm +
			  aeroSetting.frontRideHeight2Coefficient * fRH_mm * fRH_mm +
			  aeroSetting.rearRideHeightCoefficient * rRH_mm +
			  aeroSetting.rearRideHeight2Coefficient * rRH_mm * rRH_mm +
			  aeroSetting.absoluteYawCoefficient * yawAngle_deg +
			  aeroSetting.absoluteSteerCoefficient * steerAngle_deg +
			  aeroSetting.absoluteRollCoefficient * rollAngle_deg +
			  aeroSetting.dRS_Coefficient * DRSpos +
			  aeroSetting.frontFlapCoefficient * flapAngle_deg;
		return SCn;
	}


	//  Function Name: CalcDRSPosition
	//  This function calculates the DRS position
	//
	//	 [IN]	throttlePos: 0 to 1 [-]
	//	 [IN]	brakePressure: [bar]
	//	 [IN]	DRSpos:      0 to 1 [-]
	//
	//	 [OUT]	DRSpos:      0 to 1 [-]

	float CalcDRSPosition(float throttlePos, float brakePressure, float DRSpos, bool DRSbutton, bool DRSdisabled)
	{
		if (throttlePos == 1 && brakePressure < 1 && !DRSclosing && !DRSdisabled)
		{
			if (DRSbutton == true)
				DRSopenButton = true;

			if (DRSopenButton)
            {
				DRSpos += Time.deltaTime * (1 / dRSActivationTime);
			}
            else
            {
				DRStime -= Time.deltaTime;
				if (DRStime <= 0.0f)
					DRSpos += Time.deltaTime * (1 / dRSActivationTime);
			}
		}
		else
		{
			DRSopenButton = false;
			DRSclosing = true;
			if (DRSpos == 0)
			{
				DRSclosing = false;
			}
			DRSpos -= Time.deltaTime * (1 / dRSActivationTime);
			DRStime = dRSActivationDelay;
		}
		DRSpos = Mathf.Clamp(DRSpos, 0, 1.0f);

		if (DRSpos == 1)
			DRSopenButton = false;

		return DRSpos;
	}


	public override void OnEnableVehicle() {
		flapAngle = frontFlapStaticAngle;
	}


	public override void FixedUpdateVehicle()
	{
		Rigidbody rb = vehicle.cachedRigidbody;

		// Getting traveled distance in current lap
		Telemetry.DataRow telemetryDataRow = vehicle.telemetry.latest;
		float distance = (float)telemetryDataRow.distance;

		// checking if car is in a DRS disabled zone
		bool drsDisabled = isNoDRSZone(distance);
		noDRSzone = System.Convert.ToSingle(drsDisabled);

		// Getting driver's input
		int[] customData = vehicle.data.Get(Channel.Custom);
		bool processedInputs = customData[Perrinn424Data.EnableProcessedInput] != 0;

		//if (processedInputs)
		//	{
		//	// Processed DRS position from the 424 data
		//	DRS = customData[Perrinn424Data.InputDrsPosition] / 1000.0f;
		//	}
		//else
		//	{
			// Detecting DRS button pressed and acknowledge
			int[] raceInput = vehicle.data.Get(Channel.RaceInput);
			bool drsPressed = raceInput[RaceInputData.Drs] != 0;
			raceInput[RaceInputData.Drs] = 0;

			// Raw inputs from the 424 data
			float throttleInput = customData[Perrinn424Data.ThrottleInput] / 1000.0f;
			float brakePressure = customData[Perrinn424Data.BrakePressure] / 1000.0f;

			// Calculating DRS position
			DRS = CalcDRSPosition(throttleInput, brakePressure, DRS, drsPressed, drsDisabled);
		//	}

		// Feeding DRS position to the car data bus
		customData[Perrinn424Data.DrsPosition] = Mathf.RoundToInt(DRS * 1000);

		float dynamicPressure = CalculateDynamicPressure();
		rho = (float)atmosphere.Density;

		// Setting vehicle parameters for the aero model
		yawAngle        = vehicle.speed > 1.0f ? vehicle.speedAngle : 0.0f;
		steerAngle      = (vehicle.wheelState[0].steerAngle + vehicle.wheelState[1].steerAngle) / 2;
		fronRollAngle   = customData[Perrinn424Data.FrontRollAngle] / 1000.0f;
		rearRollAngle   = customData[Perrinn424Data.RearRollAngle] / 1000.0f;
		rollAngle       = (fronRollAngle + rearRollAngle) / 2;
		frontRideHeight = customData[Perrinn424Data.FrontRideHeight];
		rearRideHeight  = customData[Perrinn424Data.RearRideHeight];

		// Calculating front flap deflection due to aeroelasticity
		flapForce = (frontFlapSCz0 + frontFlapSCz_perDeg * flapAngle) * dynamicPressure;
		flapDeflection = Mathf.Max(frontFlapDeflectionMax, Mathf.Max(0, flapForce - frontFlapDeflectionPreload) * frontFlapDeflectionStiffness);
		flapAngle = Mathf.Clamp(frontFlapStaticAngle + flapDeflection + DRS * frontFlapDRSAngle, -15, 15);

		// Calculating aero forces
		if (front.applicationPoint != null)
		{
			SCzFront = CalcAeroCoeff(front, frontRideHeight, rearRideHeight, yawAngle, steerAngle, rollAngle, DRS, flapAngle);
			Vector3 VEC_SCzFront = -SCzFront * dynamicPressure * front.applicationPoint.up;
			rb.AddForceAtPosition(VEC_SCzFront, front.applicationPoint.position);
		}

		if (rear.applicationPoint != null)
		{
			SCzRear = CalcAeroCoeff(rear, frontRideHeight, rearRideHeight, yawAngle, steerAngle, rollAngle, DRS, flapAngle);
			Vector3 VEC_SCzRear = -SCzRear * dynamicPressure * rear.applicationPoint.up;
			rb.AddForceAtPosition(VEC_SCzRear, rear.applicationPoint.position);
		}

		if (drag.applicationPoint != null)
		{
			SCx = CalcAeroCoeff(drag, frontRideHeight, rearRideHeight, yawAngle, steerAngle, rollAngle, DRS, flapAngle);
			Vector3 VEC_SCx = -SCx * dynamicPressure * drag.applicationPoint.forward;
			rb.AddForceAtPosition(VEC_SCx, drag.applicationPoint.position);
		}

		downforceFront = SCzFront * dynamicPressure;
		downforceRear  = SCzRear  * dynamicPressure;
		dragForce      = SCx      * dynamicPressure;
		aeroBal	= downforceFront / (downforceFront + downforceRear) * 100;
	}


	public override void UpdateVehicle()
	{
		if (drsFlap != null)
		{
			float drsAngle = Mathf.Lerp(drsClosedAngle, drsOpenAngle, DRS);
			drsFlap.localRotation = Quaternion.Euler(drsAngle, 0.0f, 0.0f);
		}

		if (frontFlap != null)
		{
			float flapNorm  = (flapAngle - 15.0f) / (-30.0f);
			float visualFlapAngle = Mathf.Lerp(15.0f, -15.0f, flapNorm);

			frontFlap.localRotation = Quaternion.Euler(visualFlapAngle + frontFlapRestAngle, 0.0f, 0.0f);
		}
	}


	private float CalculateDynamicPressure()
	{
		Rigidbody rb = vehicle.cachedRigidbody;
		float vSquared = rb.velocity.sqrMagnitude;
		float y = rb.worldCenterOfMass.y;
		float altitude = altitudeConverter.ToAltitude(y);
		atmosphere.UpdateAtmosphere(altitude, deltaISA);
		float dynamicPressure = (float)(atmosphere.Density * vSquared / 2.0);
		return dynamicPressure;
	}


	// Telemetry


	public override bool EmitTelemetry ()
	{
		return emitTelemetry;
	}


	public override void RegisterTelemetry ()
	{
		vehicle.telemetry.Register<Perrinn424AeroTelemetry>(this);
	}


	public override void UnregisterTelemetry ()
	{
		vehicle.telemetry.Unregister<Perrinn424AeroTelemetry>(this);
	}


	public class Perrinn424AeroTelemetry : Telemetry.ChannelGroup
	{
		public override int GetChannelCount ()
		{
			return 13;
		}


		public override Telemetry.PollFrequency GetPollFrequency ()
		{
			return Telemetry.PollFrequency.Normal;
		}


		public override void GetChannelInfo (Telemetry.ChannelInfo[] channelInfo, UnityEngine.Object instance)
		{
			// Custom semantics

			Telemetry.SemanticInfo aeroCoeffSemantic = new Telemetry.SemanticInfo();
			aeroCoeffSemantic.SetRangeAndFormat(1.0f, 3.0f, "0.00", "", quantization:0.1f);

			Telemetry.SemanticInfo aeroForceSemantic = new Telemetry.SemanticInfo();
			aeroForceSemantic.SetRangeAndFormat(0.0f, 15000.0f, "0", " N", quantization:1000);

			Telemetry.SemanticInfo aeroAngleSemantic = new Telemetry.SemanticInfo();
			aeroAngleSemantic.SetRangeAndFormat(-5.0f, 5.0f, "0.00", "°", quantization:1);

			Telemetry.SemanticInfo airDensitySemantic = new Telemetry.SemanticInfo();
			airDensitySemantic.SetRangeAndFormat(1.15f, 1.19f, "0.0000", " kg/m³", quantization:0.05f, alternateFormat:"0.00");

			// TODO: Use built-in SteerAngle semantic when available.
			// Current SteerAngle semantic is related to the steering wheel angle and has been
			// renamed to SteeringWheelAngle in the latest VPP source.

			Perrinn424Aerodynamics aero = instance as Perrinn424Aerodynamics;
			Steering.Settings steering = aero.vehicle.GetInternalObject(typeof(Steering.Settings)) as Steering.Settings;

			Telemetry.SemanticInfo steerAngleSemantic = new Telemetry.SemanticInfo();
			steerAngleSemantic.SetRangeAndFormat(-steering.maxSteerAngle, steering.maxSteerAngle, "0.0", "°", quantization:5);

			// Fill-in channel information

			channelInfo[0].SetNameAndSemantic("AeroDrsPosition", Telemetry.Semantic.Ratio);
			channelInfo[1].SetNameAndSemantic("AeroSczFront", Telemetry.Semantic.Custom, aeroCoeffSemantic);
			channelInfo[2].SetNameAndSemantic("AeroSczRear", Telemetry.Semantic.Custom, aeroCoeffSemantic);
			channelInfo[3].SetNameAndSemantic("AeroDownforceFront", Telemetry.Semantic.Custom, aeroForceSemantic);
			channelInfo[4].SetNameAndSemantic("AeroDownforceRear", Telemetry.Semantic.Custom, aeroForceSemantic);
			channelInfo[5].SetNameAndSemantic("AeroDrag", Telemetry.Semantic.Custom, aeroForceSemantic);

			channelInfo[6].SetNameAndSemantic("AeroSteer", Telemetry.Semantic.Custom, steerAngleSemantic);
			channelInfo[7].SetNameAndSemantic("AeroYaw", Telemetry.Semantic.BankAngle);
			channelInfo[8].SetNameAndSemantic("AeroRoll", Telemetry.Semantic.BankAngle);

			channelInfo[9].SetNameAndSemantic("AeroBalance", Telemetry.Semantic.Ratio);
			channelInfo[10].SetNameAndSemantic("AeroFrontFlap", Telemetry.Semantic.Custom, aeroAngleSemantic);
			channelInfo[11].SetNameAndSemantic("AirDensity", Telemetry.Semantic.Custom, airDensitySemantic);

			channelInfo[12].SetNameAndSemantic("AeroNoDRSZone", Telemetry.Semantic.Ratio);
		}


		public override void PollValues (float[] values, int index, UnityEngine.Object instance)
		{
			Perrinn424Aerodynamics aero = instance as Perrinn424Aerodynamics;

			values[index+0] = aero.DRS;
			values[index+1] = aero.SCzFront;
			values[index+2] = aero.SCzRear;
			values[index+3] = aero.downforceFront;
			values[index+4] = aero.downforceRear;
			values[index+5] = aero.dragForce;

			values[index+6] = aero.steerAngle;
			values[index+7] = aero.yawAngle;
			values[index+8] = aero.rollAngle;

			values[index+9] = aero.aeroBal / 100.0f;
			values[index+10] = aero.flapAngle;
			values[index+11] = aero.rho;

			values[index+12] = aero.noDRSzone;
		}
	}



}
