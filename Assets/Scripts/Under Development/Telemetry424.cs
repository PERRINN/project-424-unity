//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

// VPDiagnosticsCharts: Additional data loggers for diagnosing specific features.
//
// Add-on component for enabling the new charts in the VPPerformanceDisplay component.


using UnityEngine;
using System;
using EdyCommonTools;
using VehiclePhysics;


namespace Project424
{
#if !VPP_ESSENTIAL

[RequireComponent(typeof(VPPerformanceDisplay))]
public class Telemetry424 : MonoBehaviour
	{
	public enum Charts { ForceFeedback, AxleSuspension, SuspensionAnalysis, PID };
	public Charts chart = Charts.ForceFeedback;

	// public int monitoredWheel = 0;
	// public float maxBrakeTorque = 3200.0f;


	PerformanceChart[] m_charts = new PerformanceChart[]
		{
		new ForceFeedbackChart(),
		// new AbsDiagnosticsChart(),
		new AxleSuspensionChart(),
		new SuspensionAnalysisChart(),
		// new KineticEnergyChart(),
		new PIDChart(),
		};

	VPPerformanceDisplay m_perfComponent;


	void OnEnable ()
		{
		m_perfComponent = GetComponent<VPPerformanceDisplay>();
		}


	void FixedUpdate ()
		{
		// Pass the exposed parameters to their corresponding charts
		/*
		AbsDiagnosticsChart absChart = m_charts[(int)Charts.AbsDiagnostics] as AbsDiagnosticsChart;
		absChart.monitoredWheel = monitoredWheel;
		absChart.maxBrakeTorque = maxBrakeTorque;
		*/

		// Apply the selected custom chart

		m_perfComponent.customChart = m_charts[(int)chart];
		}
	}


public class ForceFeedbackChart : PerformanceChart
	{
	DataLogger.Channel m_forceFactor;
	DataLogger.Channel m_damperFactor;

	VPDeviceInput m_deviceInput;


	public override string Title ()
		{
		return "Force Feedback";
		}


	public override void Initialize ()
		{
		dataLogger.topLimit = 12.0f;
		dataLogger.bottomLimit = -0.5f;

		m_deviceInput = vehicle.GetComponentInChildren<VPDeviceInput>();
		}


	public override void ResetView ()
		{
		dataLogger.rect = new Rect(0.0f, -0.5f, 30.0f, 12.5f);
		}


	public override void SetupChannels ()
		{
		m_damperFactor = dataLogger.NewChannel("Damper");
		m_damperFactor.color = GColor.Alpha(GColor.orange, 1.0f);
		m_damperFactor.SetOriginAndSpan(0.0f, 5.0f);
		m_damperFactor.valueFormat = "0.0 %";

		m_forceFactor = dataLogger.NewChannel("Force");
		m_forceFactor.color = GColor.Alpha(GColor.accentBlue, 1.0f);
		m_forceFactor.SetOriginAndSpan(5.0f, 5.0f);
		m_forceFactor.valueFormat = "0.0 %";
		}

	public override void RecordData ()
		{
		if (m_deviceInput == null) return;

		m_forceFactor.Write(m_deviceInput.currentForceFactor);
		m_damperFactor.Write(m_deviceInput.currentDamperFactor);
		}

	}






/*
// Abs Diagnostics


public class AbsDiagnosticsChart : PerformanceChart
	{
	// Channels

	DataLogger.Channel m_speed;
	DataLogger.Channel m_brakePedal;
	DataLogger.Channel m_brakeTorque;
	DataLogger.Channel m_wheelSpin;
	DataLogger.Channel m_longitudinalG;

	DataLogger.Channel m_slip;


	public int monitoredWheel = 0;
	public float maxBrakeTorque = 3200.0f;


	public override string Title ()
		{
		return "Abs Diagnostics";
		}


	public override void Initialize ()
		{
		dataLogger.topLimit = 12.0f;
		dataLogger.bottomLimit = -0.5f;
		}


	public override void ResetView ()
		{
		dataLogger.rect = new Rect(0.0f, -0.5f, 30.0f, 12.5f);
		}


	public override void SetupChannels ()
		{
		// Channels will be drawn in the same order they're created

		m_slip = dataLogger.NewChannel("Slip");
		m_slip.color = GColor.Alpha(GColor.gray, 0.2f);
		m_slip.SetOriginAndSpan(3.0f, 1.0f, 10.0f);
		m_slip.valueFormat = "0.0 m/s";

		// m_slip.scale = 2.0f;
		// m_slip.valueFormat = "0.00 %";

		m_brakePedal = dataLogger.NewChannel("Brake pedal");
		m_brakePedal.color = GColor.Alpha(GColor.red, 0.6f);
		m_brakePedal.alphaBlend = true;
		m_brakePedal.valueFormat = "0 %";
		m_brakePedal.scale = 2.5f;
		m_brakePedal.captionPositionY = 2;

		m_brakeTorque = dataLogger.NewChannel("Brake torque");
		m_brakeTorque.color = GColor.Alpha(GColor.orange, 1.0f);
		m_brakeTorque.SetOriginAndSpan(0.0f, 2.5f, maxBrakeTorque);
		m_brakeTorque.alphaBlend = true;
		m_brakeTorque.valueFormat = "0 Nm";

		m_longitudinalG = dataLogger.NewChannel("Acceleration");
		m_longitudinalG.color = GColor.blue;
		m_longitudinalG.SetOriginAndSpan(10.0f, 3.0f, reference.maxAccelerationG);
		m_longitudinalG.valueFormat = "0.00 G";
		m_longitudinalG.captionPositionY = 0;

		VehicleBase.WheelState ws = vehicle.wheelState[monitoredWheel];
		m_wheelSpin = dataLogger.NewChannel(ws.wheelCol.name);
		m_wheelSpin.color = GColor.Alpha(wheelChartColors[monitoredWheel], 0.7f);
		m_wheelSpin.SetOriginAndSpan(4.0f, 5.0f, reference.maxSpeed * 3.6f);
		m_wheelSpin.alphaBlend = true;
		m_wheelSpin.valueFormat = "0.0 km/h";
		m_wheelSpin.captionPositionY = 2;

		m_speed = dataLogger.NewChannel("Reference Speed");
		m_speed.color = GColor.cyan;
		m_speed.SetOriginAndSpan(4.0f, 5.0f, reference.maxSpeed * 3.6f);
		m_speed.valueFormat = "0.0 km/h";
		}


	public override void RecordData ()
		{
		// Gather information

		int[] vehicleData = vehicle.data.Get(Channel.Vehicle);
		float speed = vehicleData[VehicleData.Speed] / 1000.0f;
		m_speed.Write(speed * 3.6f);

		int[] inputData = vehicle.data.Get(Channel.Input);
		float brake = inputData[InputData.Brake] / 10000.0f;
		m_brakePedal.Write(brake);

		VehicleBase.WheelState ws = vehicle.wheelState[monitoredWheel];
		m_wheelSpin.Write(ws.angularVelocity * ws.wheelCol.radius * 3.6f);
		m_brakeTorque.Write(ws.brakeTorque);

		m_longitudinalG.Write(vehicle.localAcceleration.z / Gravity.reference);

		// Slip ratio. Not cool: ratios result lower with higher speeds.

		// float vf = ws.localWheelVelocity.y;
		// float vw = ws.angularVelocity * ws.wheelCol.radius;
		// float brakeSlip = Mathf.Clamp01((vf - vw) / vf);
		// float tractionSlip = Mathf.Clamp01((vw - vf) / vw);
		// if (vf > 0.1f && vf - vw > 0.1f)
			// m_slip.Write(brakeSlip);

		if (ws.tireSlip.y > -0.1f)
			m_slip.Write(ws.tireSlip.y);
		}
	}
*/


// Axle suspension


public class AxleSuspensionChart : PerformanceChart
	{
	DataLogger.Channel m_steerAngle;
	DataLogger.Channel m_yawRate;
	DataLogger.Channel m_roll;
	DataLogger.Channel m_yawRateVsSteer;

	DataLogger.Channel m_leftCompression;
	DataLogger.Channel m_rightCompression;
	DataLogger.Channel m_compressionDiff;
	DataLogger.Channel m_leftSpring;
	DataLogger.Channel m_rightSpring;

	int m_monitoredAxle = 0;


	public override string Title ()
		{
		return "Axle Suspension";
		}


	public override void Initialize ()
		{
		dataLogger.topLimit = 6.5f;
		dataLogger.bottomLimit = -0.5f;
		}


	public override void ResetView ()
		{
		dataLogger.rect = new Rect(0.0f, -0.5f, 20.0f, 7.0f);
		}



	public override void SetupChannels ()
		{
		m_compressionDiff = dataLogger.NewChannel("Compression Diff");
		m_compressionDiff.color = GColor.gray;
		m_compressionDiff.SetOriginAndSpan(4.5f, 1.0f, reference.maxSuspensionDistance * 1000.0f);
		m_compressionDiff.valueFormat = "0 mm";
		m_compressionDiff.captionPositionY = 1;

		m_steerAngle = dataLogger.NewChannel("Steer Angle (avg)");
		m_steerAngle.color = GColor.Alpha(Color.Lerp(GColor.teal, GColor.green, 0.75f), 0.7f);
		m_steerAngle.SetOriginAndSpan(4.5f, -1.0f, 35.0f);
		m_steerAngle.valueFormat = "0.00 °";
		m_steerAngle.alphaBlend = true;
		m_steerAngle.captionPositionY = 2;

		m_roll = dataLogger.NewChannel("Roll");
		m_roll.color = GColor.Alpha(GColor.teal, 0.7f);
		m_roll.SetOriginAndSpan(4.5f, -1.0f, 10.0f);
		m_roll.valueFormat = "0.00 °";
		m_roll.alphaBlend = true;
		m_roll.captionPositionY = 0;

		m_yawRate = dataLogger.NewChannel("Turn Rate");
		m_yawRate.color = GColor.Alpha(GColor.red, 0.6f);
		m_yawRate.SetOriginAndSpan(4.5f, -1.0f, 35.0f);
		m_yawRate.valueFormat = "0.0 °/s";
		// m_yawRate.alphaBlend = true;
		m_yawRate.captionPositionY = -1;

		m_yawRateVsSteer = dataLogger.NewChannel("Turn Rate vs. Steering");
		m_yawRateVsSteer.color = GColor.gray;
		m_yawRateVsSteer.SetOriginAndSpan(2.0f, 1.0f, 15.0f);
		m_yawRateVsSteer.valueFormat = "0.00";

		// Channels will be drawn in the same order they're created

		m_leftCompression = dataLogger.NewChannel("Left Contact Depth");
		m_leftCompression.color = GColor.Alpha(GColor.accentYellow, 0.7f);
		m_leftCompression.SetOriginAndSpan(1.0f, 1.0f, reference.maxSuspensionDistance * 1000.0f);
		m_leftCompression.alphaBlend = true;
		m_leftCompression.valueFormat = "0 mm";
		m_leftCompression.captionPositionY = 2;

		m_rightCompression = dataLogger.NewChannel("Right Contact Depth");
		m_rightCompression.color = GColor.Alpha(GColor.accentLightBlue, 0.7f);
		m_rightCompression.SetOriginAndSpan(1.0f, 1.0f, reference.maxSuspensionDistance * 1000.0f);
		m_rightCompression.alphaBlend = true;
		m_rightCompression.valueFormat = "0 mm";
		m_rightCompression.captionPositionY = 1;

		m_leftSpring = dataLogger.NewChannel("Effective Left Spring");
		m_leftSpring.color = GColor.Alpha(GColor.yellowA100, 0.7f);
		m_leftSpring.SetOriginAndSpan(0.0f, 0.9f, reference.maxSpringRate * 2.0f);
		m_leftSpring.alphaBlend = true;
		m_leftSpring.valueFormat = "0";
		m_leftSpring.captionPositionY = 2;

		m_rightSpring = dataLogger.NewChannel("Effective Right Spring");
		m_rightSpring.color = GColor.Alpha(GColor.lightBlue200, 0.7f);
		m_rightSpring.SetOriginAndSpan(0.0f, 0.9f, reference.maxSpringRate * 2.0f);
		m_rightSpring.alphaBlend = true;
		m_rightSpring.valueFormat = "0";
		m_rightSpring.captionPositionY = 1;
		}


	public override void RecordData ()
		{
		// Gather information

		int leftWheel = vehicle.GetWheelIndex(m_monitoredAxle, VehicleBase.WheelPos.Left);
		int rightWheel = vehicle.GetWheelIndex(m_monitoredAxle, VehicleBase.WheelPos.Right);

		VehicleBase.WheelState wsLeft = vehicle.wheelState[leftWheel];
		VehicleBase.WheelState wsRight = vehicle.wheelState[rightWheel];

		float averageSteerAngle = 0.5f * (wsLeft.steerAngle + wsRight.steerAngle);
		float yawRate = vehicle.cachedRigidbody.angularVelocity.y * Mathf.Rad2Deg;

		float roll = vehicle.cachedRigidbody.rotation.eulerAngles.z;
		if (roll > 180.0f) roll -= 360.0f;

		// Write to log

		m_steerAngle.Write(averageSteerAngle);
		m_yawRate.Write(yawRate);

		if (Mathf.Abs(roll) < 15.0f)
			m_roll.Write(roll);

		if (wsLeft.grounded) m_leftCompression.Write(wsLeft.contactDepth * 1000.0f);
		if (wsRight.grounded) m_rightCompression.Write(wsRight.contactDepth * 1000.0f);
		m_compressionDiff.Write((wsRight.contactDepth - wsLeft.contactDepth) * 1000.0f);

		if (MathUtility.FastAbs(averageSteerAngle) > 1.0f)
			m_yawRateVsSteer.Write(yawRate / averageSteerAngle);

		m_leftSpring.Write(wsLeft.wheelCol.lastRuntimeSpringRate);
		m_rightSpring.Write(wsRight.wheelCol.lastRuntimeSpringRate);
		}
	}


public class SuspensionAnalysisChart : PerformanceChart
	{
	DataLogger.Channel m_contactDepth;
	DataLogger.Channel m_contactSpeed;
	DataLogger.Channel m_suspensionForce;
	DataLogger.Channel m_damperForce;
	DataLogger.Channel m_suspensionTravel;

	int monitoredWheel = 1;


	public override string Title ()
		{
		return "Suspension Analysis";
		}


	public override void Initialize ()
		{
		dataLogger.topLimit = 32.0f;
		dataLogger.bottomLimit = -24.0f;
		}


	public override void ResetView ()
		{
		dataLogger.rect = new Rect(0.0f, -0.5f, 20.0f, 8.0f);
		}


	public override void SetupChannels ()
		{
		m_suspensionTravel = dataLogger.NewChannel("Suspension travel");
		m_suspensionTravel.color = GColor.Alpha(GColor.lime, 0.7f);
		m_suspensionTravel.SetOriginAndSpan(3.0f, 1.0f, reference.maxSuspensionDistance * 1000.0f);
		m_suspensionTravel.alphaBlend = true;
		m_suspensionTravel.valueFormat = "0 mm";
		m_suspensionTravel.captionPositionY = 0;

		m_contactSpeed = dataLogger.NewChannel("Contact Speed");
		m_contactSpeed.color = GColor.Alpha(GColor.lightBlue, 0.7f);
		m_contactSpeed.SetOriginAndSpan(3.0f, 0.5f, reference.maxSuspensionDistance * 1000.0f);
		m_contactSpeed.alphaBlend = true;
		m_contactSpeed.scale = 1.0f / (reference.maxSuspensionDistance * 1000.0f) * 0.5f;
		m_contactSpeed.valueFormat = "0 mm/s";
		m_contactSpeed.captionPositionY = 1;

		m_contactDepth = dataLogger.NewChannel("Contact Depth");
		m_contactDepth.color = GColor.Alpha(GColor.accentTeal, 0.7f);
		m_contactDepth.SetOriginAndSpan(3.0f, 1.0f, reference.maxSuspensionDistance * 1000.0f);
		m_contactDepth.alphaBlend = true;
		m_contactDepth.valueFormat = "0 mm";
		m_contactDepth.captionPositionY = 2;

		m_damperForce = dataLogger.NewChannel("Damper force");
		m_damperForce.color = GColor.Alpha(GColor.purple, 0.7f);
		m_damperForce.SetOriginAndSpan(3.0f, 2.0f, reference.maxSuspensionDistance * reference.maxSpringRate);
		m_damperForce.alphaBlend = true;
		m_damperForce.valueFormat = "0 N";
		m_damperForce.captionPositionY = -2;

		m_suspensionForce = dataLogger.NewChannel("Suspension force");
		m_suspensionForce.color = GColor.Alpha(GColor.red, 0.7f);
		m_suspensionForce.SetOriginAndSpan(3.0f, 2.0f, reference.maxSuspensionDistance * reference.maxSpringRate);
		m_suspensionForce.alphaBlend = true;
		m_suspensionForce.valueFormat = "0 N";
		m_suspensionForce.captionPositionY = -1;
		}


	public override void RecordData ()
		{
		// Gather information

		VehicleBase.WheelState ws = vehicle.wheelState[monitoredWheel];

		// Write to log

		if (ws.grounded)
			{
			m_contactDepth.Write(ws.contactDepth * 1000.0f);
			m_contactSpeed.Write(ws.contactSpeed * 1000.0f);
			m_suspensionForce.Write(ws.suspensionLoad);
			m_damperForce.Write(ws.damperForce);
			}

		m_suspensionTravel.Write(ws.wheelCol.suspensionDistance * 1000.0f);
		}
	}


/*
// Kinetic Energy Chart


public class KineticEnergyChart : PerformanceChart
	{
	// Channels

	// Energy per unit (assuming mass = 1, inertia = Identity)

	DataLogger.Channel m_totalEnergy;
	DataLogger.Channel m_linearEnergy;
	DataLogger.Channel m_angularEnergy;

	DataLogger.Channel m_totalEnergyDelta;
	DataLogger.Channel m_linearEnergyDelta;
	DataLogger.Channel m_angularEnergyDelta;


	float m_lastTotalEnergy;
	float m_lastLinearEnergy;
	float m_lastAngularEnergy;


	public override string Title ()
		{
		return "Kinetic Energy";
		}


	public override void Initialize ()
		{
		dataLogger.topLimit = 25.0f;
		dataLogger.bottomLimit = -12.5f;

		m_lastTotalEnergy = RigidbodyUtility.GetNormalizedKineticEnergy(vehicle.cachedRigidbody);
		m_lastLinearEnergy = RigidbodyUtility.GetNormalizedLinearKineticEnergy(vehicle.cachedRigidbody);
		m_lastAngularEnergy = RigidbodyUtility.GetNormalizedAngularKineticEnergy(vehicle.cachedRigidbody);
		}


	public override void ResetView ()
		{
		dataLogger.rect = new Rect(0.0f, -0.5f, 30.0f, 13.5f);
		}


	public override void SetupChannels ()
		{
		// Channels will be drawn in the same order they're created

		string energyFormat = "0.0 J";
		float energyScale = 0.5f*reference.maxSpeed*reference.maxSpeed;

		m_linearEnergy = dataLogger.NewChannel("Linear");
		m_linearEnergy.color = GColor.accentGreen;
		m_linearEnergy.SetOriginAndSpan(0.0f, 12.0f, energyScale);
		m_linearEnergy.valueFormat = energyFormat;

		m_angularEnergy = dataLogger.NewChannel("Angular");
		m_angularEnergy.color = GColor.accentCyan;
		m_angularEnergy.SetOriginAndSpan(0.0f, 12.0f, energyScale);
		m_angularEnergy.valueFormat = energyFormat;
		m_angularEnergy.captionPositionY = 2;

		m_totalEnergy = dataLogger.NewChannel("Total");
		m_totalEnergy.color = GColor.accentRed;
		m_totalEnergy.SetOriginAndSpan(0.0f, 12.0f, energyScale);
		m_totalEnergy.valueFormat = energyFormat;
		m_totalEnergy.captionPositionY = 3;

		m_linearEnergyDelta = dataLogger.NewChannel("Linear Δ");
		m_linearEnergyDelta.color = GColor.green;
		m_linearEnergyDelta.SetOriginAndSpan(8.0f, 4.0f, energyScale / 32.0f);
		m_linearEnergyDelta.valueFormat = energyFormat;

		m_angularEnergyDelta = dataLogger.NewChannel("Angular Δ");
		m_angularEnergyDelta.color = GColor.cyan;
		m_angularEnergyDelta.SetOriginAndSpan(8.0f, 4.0f, energyScale / 32.0f);
		m_angularEnergyDelta.valueFormat = energyFormat;
		m_angularEnergyDelta.captionPositionY = 2;

		m_totalEnergyDelta = dataLogger.NewChannel("Total Δ");
		m_totalEnergyDelta.color = GColor.red;
		m_totalEnergyDelta.SetOriginAndSpan(8.0f, 4.0f, energyScale / 32.0f);
		m_totalEnergyDelta.valueFormat = energyFormat;
		m_totalEnergyDelta.captionPositionY = 3;
		}


	public override void RecordData ()
		{
		float totalEnergy = RigidbodyUtility.GetNormalizedKineticEnergy(vehicle.cachedRigidbody);
		m_totalEnergy.Write(totalEnergy);
		m_totalEnergyDelta.Write(totalEnergy - m_lastTotalEnergy);
		m_lastTotalEnergy = totalEnergy;

		float linearEnergy = RigidbodyUtility.GetNormalizedLinearKineticEnergy(vehicle.cachedRigidbody);
		m_linearEnergy.Write(linearEnergy);
		m_linearEnergyDelta.Write(linearEnergy - m_lastLinearEnergy);
		m_lastLinearEnergy = linearEnergy;

		float angularEnergy = RigidbodyUtility.GetNormalizedAngularKineticEnergy(vehicle.cachedRigidbody);
		m_angularEnergy.Write(angularEnergy);
		m_angularEnergyDelta.Write(angularEnergy - m_lastAngularEnergy);
		m_lastAngularEnergy = angularEnergy;
		}
	}
*/

	//PID Graph

	public class PIDChart : PerformanceChart
	{
		DataLogger.Channel m_error;

		public override string Title()
		{
			return "PID Error Display";
		}

		public override void Initialize()
		{
			dataLogger.topLimit = 40.0f;
			dataLogger.bottomLimit = -10.0f;
		}

		public override void ResetView()
		{
			dataLogger.rect = new Rect(0.0f, -0.5f, 30.0f, 12.5f);
		}

		public override void SetupChannels()
		{
			m_error = dataLogger.NewChannel("Error");
			m_error.color = GColor.blue;
			m_error.SetOriginAndSpan(9.0f, 6.0f, 100.0f);
			m_error.valueFormat = "0.00";
			m_error.captionPositionY = 0;
		}

		public override void RecordData()
		{
			float errorDistance = PID.height;
			m_error.Write(errorDistance);
		}
	}
#endif
}
