using UnityEngine;
using VehiclePhysics;
using System;
using System.Collections;

// THINGS TO MODIFY:
// bodyRoll: Change to the correct vehicle body roll due to suspension kinematics
public class Perrinn424Aerodynamics : VehicleBehaviour
{
	[Serializable]

	public class AeroSettings
	{
		public Transform applicationPoint;

		// Coefficients for the aero model equation
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

	public float airDensity         = 1.0f;
	public float frontRideHeight    = 1.0f;
	public float rearRideHeight     = 1.0f;
	public float flapAngle          = 1.0f;
	public float dRSActivationDelay = 1.0f;
	public float dRSActivationTime  = 1.0f;

	// creating public aero instances
	public AeroSettings front = new AeroSettings();
	public AeroSettings rear  = new AeroSettings();
	public AeroSettings drag  = new AeroSettings();

	float DRStime    = 0;
    bool DRSStatus   = false;
	bool DRSclosing  = false;
	public float DRS = 0;

	public float SCzFront = 0;
	public float SCzRear  = 0;
	public float SCx      = 0;

	public float downforceFront = 0;
	public float downforceRear  = 0;
	public float dragForce      = 0;

	public float yawAngle   = 0;
	public float steerAngle = 0;
	public float rollAngle  = 0;

	//%  Function Name: CalcAeroCoeff 
	//%  This function calculates a given aerodynamic coefficient based on:
	//% 
	//%	 [IN]	aeroSetting [AeroSettings]
	//%	 [IN]	fRH_mm [mm]
	//%	 [IN]	rRH_mm [mm]
	//%	 [IN]	yawAngle_deg [deg]
	//%	 [IN]	steerAngle_deg [deg]
	//%	 [IN]	rollAngle_deg [deg]
	//%	 [IN]	DRS [-]
	//%	 [IN]	flapAngle_deg [deg]
	//%
	//%	 [OUT]	SCn [m2]
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

	//%  Function Name: ConvertAngle 
	//%  This function converts the an angle from 0/360 to -180/180 deg.
	//% 
	//%	 [IN]	angle_deg [deg]
	//%
	//%	 [OUT]	angle_deg [deg]
	//%********************************************************************
	float ConvertAngle(float angle_deg)
    {
		if (angle_deg > 180)
			angle_deg -= 360;

		return angle_deg;
	}


	//%********************************************************************
	//%  Function Name: CalcDRSPosition 
	//%  This function calculates the DRS position
	//% 
	//%	 [IN]	throttlePos: 0 to 1 [-]
	//%	 [IN]	brakePos: 0 to 1 [-]
	//%	 [IN]	DRSpos: 0 to 1 [-]
	//%
	//%	 [OUT]	DRSpos: 0 to 1 [-]
	//%********************************************************************
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
		// Assisgning local variables
		Rigidbody rb = vehicle.cachedRigidbody;
		float vSquared        = rb.velocity.sqrMagnitude;
		float dynamicPressure = (float)(airDensity * vSquared / 2.0);
		
		yawAngle   = Math.Min(Math.Max(Math.Abs(vehicle.speedAngle), 0), 10);
		steerAngle = Math.Min(Math.Max(Math.Abs(Math.Abs(vehicle.wheelState[0].steerAngle + vehicle.wheelState[1].steerAngle) / 2), 0), 20);

        // Normalizing roll angle to -180/+180
        // NOTE: THIS ANGLE IS NOT THE ACTUAL BODY ROLL DUE TO SUSPENSION KINEMATICS, BUT THE WORLD-BASED ROLL
        rollAngle = Math.Abs(ConvertAngle(rb.rotation.eulerAngles[2]));

        // Getting driver's input
        int[] input = vehicle.data.Get(Channel.Input);
		float throttlePosition = input[InputData.Throttle] / 10000.0f;
		float brakePosition = input[InputData.Brake] / 10000.0f;

		// Calculating DRS position and feeding to the car data bus
		DRS = CalcDRSPosition(throttlePosition, brakePosition, DRS);
		vehicle.data.Set(Channel.Custom, Perrinn424Data.DrsPosition, Mathf.RoundToInt(DRS * 1000));
		
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

		downforceFront = (float)(SCzFront * dynamicPressure / 9.80665);
		downforceRear  = (float)(SCzRear  * dynamicPressure / 9.80665);
		dragForce      = (float)(SCx      * dynamicPressure / 9.80665);

	}

}




