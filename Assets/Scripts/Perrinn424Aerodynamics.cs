using UnityEngine;
using VehiclePhysics;
using System;
using System.Collections;

// THINGS TO MODIFY:
// bodyRoll: Change to the correct vehicle body roll due to suspension kinematics
public class Perrinn424Aerodynamics : VehicleBehaviour
{
	public bool showTelemetry = false;
	public Vector2 position = new Vector2(8, 8);
	public Font font;
	[Range(6, 100)]
	public int fontSize = 10;
	public Color fontColor = Color.white;

	string m_text = "";
	GUIStyle m_textStyle = new GUIStyle();
	float m_boxWidth;
	float m_boxHeight;

	[Serializable]

	public class AeroSettings
	{
		public Transform applicationPoint;

		// Coefficients for the aero model equation
		public float constant = 1.0f;
		public float frontRideHeightCoefficient = 1.0f;
		public float frontRideHeight2Coefficient = 1.0f;
		public float rearRideHeightCoefficient = 1.0f;
		public float rearRideHeight2Coefficient = 1.0f;
		public float absoluteYawCoefficient = 1.0f;
		public float absoluteSteerCoefficient = 1.0f;
		public float absoluteRollCoefficient = 1.0f;
		public float dRS_Coefficient = 1.0f;
		public float frontFlapCoefficient = 1.0f;
	}

	// creating public dummies - These should be fed by the physics and system inputs
	public float airDensity = 1.0f;
	public float frontRideHeight = 1.0f;
	public float rearRideHeight = 1.0f;
	public float flapAngle = 1.0f;
	public float dRSActivationDelay = 1.0f;
	public float dRSActivationTime = 1.0f;

	// creating public aero instances
	public AeroSettings front = new AeroSettings();
	public AeroSettings rear = new AeroSettings();
	public AeroSettings drag = new AeroSettings();

	private float DRStime = 0;
	private float DRS = 0;
	private bool DRSlogic = false;
	
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
	private float CalcAeroCoeff(AeroSettings aeroSetting, float fRH_mm, float rRH_mm, float yawAngle_deg, float steerAngle_deg, float rollAngle_deg, float DRSpos, float flapAngle_deg)
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
	private float ConvertAngle(float angle_deg)
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
	private float CalcDRSPosition(float throttlePos, float brakePos, float DRSpos)
	{
		if (throttlePos == 1 && brakePos == 0)
        {
			DRSlogic = true;
			DRStime -= Time.deltaTime;
			if (DRStime <= 0.0f)
				DRSpos += Time.deltaTime * (1 / dRSActivationTime);
		}
        else
        {
			DRSlogic = false;
			DRSpos -= Time.deltaTime * (1 / dRSActivationTime);
			DRStime = dRSActivationDelay;
		}
		DRSpos = Math.Min(Math.Max(DRSpos, 0), 1);
		return DRSpos;
	}


	public override void FixedUpdateVehicle()
	{
		// Setting telmetry window properties
		m_textStyle.font = font;
		m_textStyle.fontSize = fontSize;
		m_textStyle.normal.textColor = fontColor;

		// Assisgning local variables
		Rigidbody rb = vehicle.cachedRigidbody;
		float vSquared        = rb.velocity.sqrMagnitude;
		float dynamicPressure = (float)(airDensity * vSquared / 2.0);
        float yawAngle        = Math.Abs(vehicle.speedAngle);
	    float steerAngle      = Math.Abs(vehicle.wheelState[0].steerAngle + vehicle.wheelState[1].steerAngle) / 2;
		float SCzFront        = 0;
		float SCzRear         = 0;
		float SCx             = 0;

        // Normalizing roll angle to -180/+180
        // NOTE: THIS ANGLE IS NOT THE ACTUAL BODY ROLL DUE TO SUSPENSION KINEMATICS, BUT THE WORLD-BASED ROLL
        float rollAngle = Math.Abs(ConvertAngle(rb.rotation.eulerAngles[2]));

        // Getting driver's input
        int[] input = vehicle.data.Get(Channel.Input);
		float throttlePosition = input[InputData.Throttle] / 10000.0f;
		float brakePosition = input[InputData.Brake] / 10000.0f;

		// Calculating DRS position
		DRS = CalcDRSPosition(throttlePosition, brakePosition, DRS);

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

		if (showTelemetry)
        {
			m_text = "";
			m_text += $"SCz Front       : {SCzFront, 6:0.000}\n";
			m_text += $"SCz Rear        : {SCzRear, 6:0.000}\n";
			m_text += $"SCx             : {SCx, 6:0.000}\n";
			m_text += $"Cz Front        : {-SCzFront * dynamicPressure / 9.80665, 6:0} kg\n";
			m_text += $"Cz Rear         : {-SCzRear * dynamicPressure / 9.80665, 6:0} kg\n";
			m_text += $"Cx              : {-SCx * dynamicPressure / 9.80665, 6:0} kg\n";
			m_text += $"DRS logic       : {DRSlogic, 6:0.000}\n";
			m_text += $"DRS position    : {DRS, 6:0.000}\n";
			m_text += $"Ride height F	: {frontRideHeight,6:0.0} mm\n";
			m_text += $"Ride height R	: {rearRideHeight,6:0.0} mm\n";
			m_text += $"Yaw angle abs	: {yawAngle,6:0.0} deg\n";
			m_text += $"Steer angle abs	: {steerAngle,6:0.0} deg\n";
			m_text += $"Roll angle abs	: {rollAngle,6:0.0} deg\n";
		}
			
		
	}

	void OnGUI()
	{
		if (showTelemetry)
        {
			// Compute box size

			Vector2 contentSize = m_textStyle.CalcSize(new GUIContent(m_text));
			float margin = m_textStyle.lineHeight * 1.2f;
			float headerHeight = GUI.skin.box.lineHeight;

			m_boxWidth = contentSize.x + margin;
			m_boxHeight = contentSize.y + headerHeight + margin / 2;

			// Compute box position

			float xPos = position.x < 0 ? Screen.width + position.x - m_boxWidth : position.x;
			float yPos = position.y < 0 ? Screen.height + position.y - m_boxHeight : position.y;

			// Draw telemetry box

			GUI.Box(new Rect(xPos, yPos, m_boxWidth, m_boxHeight), "424 Aerodynamics");
			GUI.Label(new Rect(xPos + margin / 2, yPos + margin / 2 + headerHeight, Screen.width, Screen.height), m_text, m_textStyle);
		}
	}
}




