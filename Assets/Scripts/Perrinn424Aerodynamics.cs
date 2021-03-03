using UnityEngine;
using VehiclePhysics;
using System;

class Atmosphere
{
	// Constants for the atmosphere model
	private const double tHeight = 11000;            // Tropopause Height [m]
	private const double e       = 4.25587981;       // Gas Constant Ratio
	private const double rho0    = 1.225;            // Sea Level air density at ISA [m/s]
	private const double vs0     = 340.294005808213; // Sea Level speed of sound [m/s]
	private const double mu0     = 1.7894E-005;      // Sea Level air viscosity

	double T_std             = 0;
	double Rho_Rho0          = 0;
	double Nhp               = 0;
	double Tisa              = 0;
	double T_kelvin          = 0;
	double H                 = 0;
	double Rho_Std           = 0;
	public double P_P0       = 0;
	public double T_T0       = 0;
	public double DhpDh      = 0;
	public double Vsound     = 0;
	public double Rho        = 0;
	public double Sqrt_Sigma = 0;
	public double mu         = 0;

	// Function name: CalcRho
	// This function calculates the air density at a specific geometric altitude and delta ISA temperature.
	// Many calculations are commented because we don't need them from the atmosphere model
	//
	//   [IN]	alt		[m]
	//   [IN]   dISA	[degC]
	//		
	//	 [OUT]	rho		[kg/m3]
	public void calcAtmosphereParams(float alt, float dISA)
	{
		Tisa = dISA;
		H = alt / tHeight;

		if (H < 1)
		{
			T_std = 288.15 - 71.5 * H;
			P_P0 = Math.Pow(1.0 - 0.248134652090925 * H, e + 1);
			Rho_Rho0 = Math.Pow(1.0 - 0.248134652090925 * H, e);
		}
		else  // I'm assuming we'll never go this route ;)
		{
			T_std = 216.65;
			Nhp = Math.Exp(-1.73457 * H);
			P_P0 = Nhp * 1.26567;
			Rho_Rho0 = Nhp * 1.68338;
		}
		T_kelvin = Tisa + T_std;
		T_T0 = T_kelvin / 288.15;
		DhpDh = T_std / (T_std + Tisa);
		Vsound = Math.Sqrt(T_T0) * vs0;
		Rho_Std = Rho_Rho0 * rho0;
		Rho = Rho_Std / (1 + Tisa / T_std);
		Sqrt_Sigma = Math.Sqrt(Rho / rho0);
		mu = mu0 * (Math.Pow(T_kelvin, 1.5) / (T_kelvin + 120) / 11.984);

	}
}

public class Perrinn424Aerodynamics : VehicleBehaviour
{
	Atmosphere atmos = new Atmosphere();

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

	public float heightAboveSeaLevel       = 1.0f;
	public float deltaISA                  = 1.0f;
	public float dRSActivationDelay        = 1.0f;
	public float dRSActivationTime         = 1.0f;
	public float frontFlapStaticAngle      = 1.0f;
	public float frontFlapFlexDeltaAngle   = 1.0f;
	public float frontFlapFlexMaxDownforce = 1.0f;

	public AeroSettings front = new AeroSettings();
	public AeroSettings rear  = new AeroSettings();
	public AeroSettings drag  = new AeroSettings();

	[HideInInspector] public float flapAngle = 1.0f;
	[HideInInspector] public bool  DRSclosing  = false;
	[HideInInspector] public float DRS = 0;
	[HideInInspector] public float SCzFront = 0;
	[HideInInspector] public float SCzRear  = 0;
	[HideInInspector] public float SCx      = 0;
	[HideInInspector] public float downforceFront  = 1.0f;
	[HideInInspector] public float downforceRear   = 1.0f;
	[HideInInspector] public float dragForce       = 1.0f;
	[HideInInspector] public float yawAngle        = 1.0f;
	[HideInInspector] public float steerAngle      = 1.0f;
	[HideInInspector] public float rollAngle       = 1.0f;
	[HideInInspector] public float fronRollAngle   = 1.0f;
	[HideInInspector] public float rearRollAngle   = 1.0f;
	[HideInInspector] public float frontRideHeight = 1.0f;
	[HideInInspector] public float rearRideHeight  = 1.0f;
	float DRStime = 0;
	bool DRSStatus = false;


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
		fRH_mm = Math.Min(Math.Max(fRH_mm, 0), 100);
		rRH_mm = Math.Min(Math.Max(rRH_mm, 0), 100);
		DRSpos = Math.Min(Math.Max(DRSpos, 0), 1);
		flapAngle_deg = Math.Min(Math.Max(flapAngle_deg, -5), 5);
		yawAngle_deg = Math.Min(Math.Max(Math.Abs(yawAngle_deg), 0), 10);
		steerAngle_deg = Math.Min(Math.Max(Math.Abs(steerAngle_deg), 0), 20);
		rollAngle_deg = Math.Min(Math.Max(Math.Abs(rollAngle_deg), 0), 3);

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
	//	 [IN]	brakePos:    0 to 1 [-]
	//	 [IN]	DRSpos:      0 to 1 [-]
	//
	//	 [OUT]	DRSpos:      0 to 1 [-]
		float CalcDRSPosition(float throttlePos, float brakePos, float DRSpos)
	{
		if (throttlePos == 1 && brakePos == 0 && !DRSclosing)
		{
			DRSStatus = true;
			DRStime -= Time.deltaTime;
			if (DRStime <= 0.0f)
				DRSpos += Time.deltaTime * (1 / dRSActivationTime);
		}
		else
		{
			DRSclosing = true;
			if (DRSpos == 0)
			{
				DRSclosing = false;
			}
			DRSStatus = false;
			DRSpos -= Time.deltaTime * (1 / dRSActivationTime);
			DRStime = dRSActivationDelay;
		}
		DRSpos = Math.Min(Math.Max(DRSpos, 0), 1);
		return DRSpos;
	}

	
	public override void FixedUpdateVehicle()
	{
		Rigidbody rb = vehicle.cachedRigidbody;

		// Getting driver's input
		int[] input            = vehicle.data.Get(Channel.Input);
		float throttlePosition = input[InputData.Throttle] / 10000.0f;
		float brakePosition    = input[InputData.Brake] / 10000.0f;

		// Calculating dynamic pressure
		float vSquared = rb.velocity.sqrMagnitude;
		atmos.calcAtmosphereParams(heightAboveSeaLevel, deltaISA);
		float dynamicPressure = (float)(atmos.Rho * vSquared / 2.0);

		// Setting vehicle parameters for the aero model
		yawAngle        = vehicle.speedAngle;
		steerAngle      = (vehicle.wheelState[0].steerAngle + vehicle.wheelState[1].steerAngle) / 2;
		fronRollAngle   = vehicle.data.Get(Channel.Custom, Perrinn424Data.FrontRollAngle) / 1000.0f;
		rearRollAngle   = vehicle.data.Get(Channel.Custom, Perrinn424Data.RearRollAngle) / 1000.0f;
		rollAngle       = (fronRollAngle + rearRollAngle) / 2;
		frontRideHeight = vehicle.data.Get(Channel.Custom, Perrinn424Data.FrontRideHeight);
		rearRideHeight  = vehicle.data.Get(Channel.Custom, Perrinn424Data.RearRideHeight);

		// Calculating DRS position and feeding to the car data bus
		DRS = CalcDRSPosition(throttlePosition, brakePosition, DRS);
		vehicle.data.Set(Channel.Custom, Perrinn424Data.DrsPosition, Mathf.RoundToInt(DRS * 1000));

		// Calculating front flap deflection due to aeroelasticity
		flapAngle = frontFlapStaticAngle + downforceFront * frontFlapFlexDeltaAngle / frontFlapFlexMaxDownforce;

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
	}

}




