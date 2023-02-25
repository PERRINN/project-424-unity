
using UnityEngine;
using VehiclePhysics;
using VehiclePhysics.InputManagement;
using EdyCommonTools;


namespace Perrinn424
{

public class Perrinn424GenericTelemetry : VehicleBehaviour
	{
	public bool emitTelemetry = true;


	public override void OnEnableVehicle ()
		{
		// Adjust the vehicle specifications according to what we know about Project 424

		vehicle.telemetry.specs.maxSpeed = 100.0f;
		vehicle.telemetry.specs.maxGearPosition = 1;
		vehicle.telemetry.specs.minGearPosition = -1;
		vehicle.telemetry.specs.maxAcceleration = 6 * Gravity.reference;
		vehicle.telemetry.specs.maxAngularAcceleration = 4000 * Mathf.Deg2Rad;
		vehicle.telemetry.specs.maxWheelTorque = 3000;
		vehicle.telemetry.specs.maxSuspensionTravel = 0.08f;
		vehicle.telemetry.specs.maxSuspensionLoad = 2000.0f * Gravity.magnitude;
		vehicle.telemetry.specs.maxTireForce = 30000.0f;
		vehicle.telemetry.specs.maxEngineRpm = 21000.0f;
		vehicle.telemetry.specs.maxEnginePowerKw = 500.0f;
		vehicle.telemetry.specs.minEnginePowerKw = -500.0f;
		vehicle.telemetry.specs.maxEngineTorque = 650.0f;
		vehicle.telemetry.specs.minEngineTorque = -650.0f;

		vehicle.telemetry.ApplySpecifications();

		// Adjust specific semantics to our vehicle's capabilities

		// Angular velocity in the range [-90, +90]

		Telemetry.SemanticInfo angularVelocitySemantic = vehicle.telemetry.semantics[(int)Telemetry.Semantic.AngularVelocity];
		angularVelocitySemantic.SetRange(-90.0f * Mathf.Deg2Rad, 90.0f * Mathf.Deg2Rad);

		// Weight in newtons (original) instead of kilograms (default)

		Telemetry.SemanticInfo weightSemantic = vehicle.telemetry.semantics[(int)Telemetry.Semantic.Weight];
		weightSemantic.displayMultiplier = 1.0f;
		weightSemantic.displayUnitsSuffix = " N";

		// Max tire forces. TODO: Remove when it's properly applied from specifications above.
		Telemetry.SemanticInfo tireForceSemantic = vehicle.telemetry.semantics[(int)Telemetry.Semantic.TireForce];
		tireForceSemantic.SetRange(-vehicle.telemetry.specs.maxTireForce, vehicle.telemetry.specs.maxTireForce);

		// Steering angle

		Steering.Settings steering = vehicle.GetInternalObject(typeof(Steering.Settings)) as Steering.Settings;
		Telemetry.SemanticInfo steeringWheelAngleSemantic = vehicle.telemetry.semantics[(int)Telemetry.Semantic.SteeringWheelAngle];
		steeringWheelAngleSemantic.SetRange(-steering.steeringWheelRange * 0.5f, steering.steeringWheelRange * 0.5f);
		}



	public override bool EmitTelemetry ()
		{
		return emitTelemetry;
		}


	public override void RegisterTelemetry ()
		{
		vehicle.telemetry.Register<Perrinn424Inputs>(vehicle);
		vehicle.telemetry.Register<Perrinn424Differential>(vehicle);
		vehicle.telemetry.Register<Perrinn424Chassis>(vehicle);
		vehicle.telemetry.Register<Perrinn424Tires>(vehicle);
		vehicle.telemetry.Register<Perrinn424Distance>(vehicle);
		vehicle.telemetry.Register<Perrinn424ForceFeedback>(vehicle);
		vehicle.telemetry.Register<Perrinn424Positions>(vehicle);
		}


	public override void UnregisterTelemetry ()
		{
		vehicle.telemetry.Unregister<Perrinn424Inputs>(vehicle);
		vehicle.telemetry.Unregister<Perrinn424Differential>(vehicle);
		vehicle.telemetry.Unregister<Perrinn424Chassis>(vehicle);
		vehicle.telemetry.Unregister<Perrinn424Tires>(vehicle);
		vehicle.telemetry.Unregister<Perrinn424Distance>(vehicle);
		vehicle.telemetry.Unregister<Perrinn424ForceFeedback>(vehicle);
		vehicle.telemetry.Unregister<Perrinn424Positions>(vehicle);
		}


	public class Perrinn424Inputs : Telemetry.ChannelGroup
		{
		VehicleBase m_vehicle;
		Steering.Settings m_steering;
		Perrinn424CarController m_controller;


		public override int GetChannelCount ()
			{
			return 5;
			}


		public override Telemetry.PollFrequency GetPollFrequency ()
			{
			return Telemetry.PollFrequency.Normal;
			}


		public override void GetChannelInfo (Telemetry.ChannelInfo[] channelInfo, Object instance)
			{
			// Access to information in the vehicle

			m_vehicle = instance as VehicleBase;
			m_steering = m_vehicle.GetInternalObject(typeof(Steering.Settings)) as Steering.Settings;
			m_controller = m_vehicle.GetComponent<Perrinn424CarController>();

			// Fill-in channel information

			channelInfo[0].SetNameAndSemantic("Gear", Telemetry.Semantic.Gear);
			channelInfo[1].SetNameAndSemantic("SteeringAngle", Telemetry.Semantic.SteeringWheelAngle);
			channelInfo[2].SetNameAndSemantic("Throttle", Telemetry.Semantic.Ratio);
			channelInfo[3].SetNameAndSemantic("BrakePressure", Telemetry.Semantic.BrakePressure);
			channelInfo[4].SetNameAndSemantic("Speed", Telemetry.Semantic.Speed);
			}


		public override void PollValues (float[] values, int index, Object instance)
			{
			values[index+0] = m_controller.gear;
			values[index+1] = m_controller.steerAngle;
			values[index+2] = m_controller.throttleInput;
			values[index+3] = m_controller.brakePressure;
			values[index+4] = m_vehicle.speed;
			}
		}


	public class Perrinn424Differential : Telemetry.ChannelGroup
		{
		VehicleBase m_vehicle;
		VehicleBase.WheelState m_wheelFL;
		VehicleBase.WheelState m_wheelFR;
		VehicleBase.WheelState m_wheelRL;
		VehicleBase.WheelState m_wheelRR;


		public override int GetChannelCount ()
			{
			return 6;
			}


		public override Telemetry.PollFrequency GetPollFrequency ()
			{
			return Telemetry.PollFrequency.Normal;
			}


		public override void GetChannelInfo (Telemetry.ChannelInfo[] channelInfo, Object instance)
			{
			m_vehicle = instance as VehicleBase;

			// Retrieve states for the four monitored wheels

			m_wheelFL = m_vehicle.wheelState[m_vehicle.GetWheelIndex(0, VehicleBase.WheelPos.Left)];
			m_wheelFR = m_vehicle.wheelState[m_vehicle.GetWheelIndex(0, VehicleBase.WheelPos.Right)];
			int rearAxle = m_vehicle.GetAxleCount() - 1;
			m_wheelRL = m_vehicle.wheelState[m_vehicle.GetWheelIndex(rearAxle, VehicleBase.WheelPos.Left)];
			m_wheelRR = m_vehicle.wheelState[m_vehicle.GetWheelIndex(rearAxle, VehicleBase.WheelPos.Right)];

			// Fill-in channel information

			channelInfo[0].SetNameAndSemantic("TorqueDiffFront", Telemetry.Semantic.WheelTorque);
			channelInfo[1].SetNameAndSemantic("TorqueDiffRear", Telemetry.Semantic.WheelTorque);
			channelInfo[2].SetNameAndSemantic("SpeedDiffFront", Telemetry.Semantic.Speed);
			channelInfo[3].SetNameAndSemantic("SpeedDiffRear", Telemetry.Semantic.Speed);
			channelInfo[4].SetNameAndSemantic("TorqueFrictionFront", Telemetry.Semantic.WheelTorque);
			channelInfo[5].SetNameAndSemantic("TorqueFrictionRear", Telemetry.Semantic.WheelTorque);
			}


		public override void PollValues (float[] values, int index, Object instance)
			{
			values[index+0] = m_wheelFL.driveTorque - m_wheelFR.driveTorque;
			values[index+1] = m_wheelRL.driveTorque - m_wheelRR.driveTorque;
			values[index+2] = m_wheelFL.angularVelocity * m_wheelFL.wheelCol.radius - m_wheelFR.angularVelocity * m_wheelFR.wheelCol.radius;
			values[index+3] = m_wheelRL.angularVelocity * m_wheelRL.wheelCol.radius - m_wheelRR.angularVelocity * m_wheelRR.wheelCol.radius;

			int[] custom = m_vehicle.data.Get(Channel.Custom);
			float frontTorqueFiction = custom[Perrinn424Data.FrontDiffFriction] / 1000.0f;
			float rearTorqueFiction = custom[Perrinn424Data.RearDiffFriction] / 1000.0f;

			values[index+4] = frontTorqueFiction;
			values[index+5] = rearTorqueFiction;
			}
		}


	public class Perrinn424Chassis : Telemetry.ChannelGroup
		{
		public override int GetChannelCount ()
			{
			return 6;
			}


		public override Telemetry.PollFrequency GetPollFrequency ()
			{
			return Telemetry.PollFrequency.High;
			}


		public override void GetChannelInfo (Telemetry.ChannelInfo[] channelInfo, Object instance)
			{
			channelInfo[0].SetNameAndSemantic("RideHeightFront", Telemetry.Semantic.SuspensionTravel);
			channelInfo[1].SetNameAndSemantic("RideHeightRear", Telemetry.Semantic.SuspensionTravel);
			channelInfo[2].SetNameAndSemantic("RollAngleFront", Telemetry.Semantic.BankAngle);
			channelInfo[3].SetNameAndSemantic("RollAngleRear", Telemetry.Semantic.BankAngle);
			channelInfo[4].SetNameAndSemantic("GroundSlope", Telemetry.Semantic.BankAngle);
			channelInfo[5].SetNameAndSemantic("GroundGrade", Telemetry.Semantic.SignedRatio);
			}


		public override void PollValues (float[] values, int index, Object instance)
			{
			VehicleBase vehicle = instance as VehicleBase;
			int[] custom = vehicle.data.Get(Channel.Custom);

			values[index+0] = custom[Perrinn424Data.FrontRideHeight] / 1000.0f;
			values[index+1] = custom[Perrinn424Data.RearRideHeight] / 1000.0f;
			values[index+2] = custom[Perrinn424Data.FrontRollAngle] / 1000.0f;
			values[index+3] = custom[Perrinn424Data.RearRollAngle] / 1000.0f;
			values[index+4] = custom[Perrinn424Data.GroundAngle] / 1000.0f;
			values[index+5] = custom[Perrinn424Data.GroundSlope] / 1000.0f;
			}
		}


	public class Perrinn424Tires : Telemetry.ChannelGroup
		{
		Perrinn424CarController m_controller;

		VehicleBase.WheelState m_wheelFL;
		VehicleBase.WheelState m_wheelFR;
		VehicleBase.WheelState m_wheelRL;
		VehicleBase.WheelState m_wheelRR;


		public override int GetChannelCount ()
			{
			return 9;
			}


		public override Telemetry.PollFrequency GetPollFrequency ()
			{
			return Telemetry.PollFrequency.Normal;
			}


		public override void GetChannelInfo (Telemetry.ChannelInfo[] channelInfo, Object instance)
			{
			VehicleBase vehicle = instance as VehicleBase;
			m_controller = vehicle as Perrinn424CarController;

			// Retrieve states for the four monitored wheels

			m_wheelFL = vehicle.wheelState[vehicle.GetWheelIndex(0, VehicleBase.WheelPos.Left)];
			m_wheelFR = vehicle.wheelState[vehicle.GetWheelIndex(0, VehicleBase.WheelPos.Right)];
			int rearAxle = vehicle.GetAxleCount() - 1;
			m_wheelRL = vehicle.wheelState[vehicle.GetWheelIndex(rearAxle, VehicleBase.WheelPos.Left)];
			m_wheelRR = vehicle.wheelState[vehicle.GetWheelIndex(rearAxle, VehicleBase.WheelPos.Right)];

			// Using custom SlipRatio semantic not yet available in the built-in collection.
			// TODO: use built-in semantic when available.

			var slipRatioSemantic = new Telemetry.SemanticInfo();
			slipRatioSemantic.SetRangeAndFormat(-1.0f, 1.0f, "0.0", " %", multiplier:100);

			// Fill-in channel information

			channelInfo[0].SetNameAndSemantic("SlipRatioFL", Telemetry.Semantic.Custom, slipRatioSemantic);
			channelInfo[1].SetNameAndSemantic("SlipRatioFR", Telemetry.Semantic.Custom, slipRatioSemantic);
			channelInfo[2].SetNameAndSemantic("SlipRatioRL", Telemetry.Semantic.Custom, slipRatioSemantic);
			channelInfo[3].SetNameAndSemantic("SlipRatioRR", Telemetry.Semantic.Custom, slipRatioSemantic);
			channelInfo[4].SetNameAndSemantic("SlipAngleFL", Telemetry.Semantic.SlipAngle);
			channelInfo[5].SetNameAndSemantic("SlipAngleFR", Telemetry.Semantic.SlipAngle);
			channelInfo[6].SetNameAndSemantic("SlipAngleRL", Telemetry.Semantic.SlipAngle);
			channelInfo[7].SetNameAndSemantic("SlipAngleRR", Telemetry.Semantic.SlipAngle);
			channelInfo[8].SetNameAndSemantic("Understeer", Telemetry.Semantic.SlipAngle);
			}


		public override void PollValues (float[] values, int index, Object instance)
			{
			FillData(m_wheelFL, values, index+0);
			FillData(m_wheelFR, values, index+1);
			FillData(m_wheelRL, values, index+2);
			FillData(m_wheelRR, values, index+3);
			values[index+8] = m_controller.understeerAngle;
			}


		void FillData (VehicleBase.WheelState ws, float[] values, int index)
			{
			if (ws.grounded)
				{
				values[index+0] = ws.slipRatio;
				values[index+4] = ws.slipAngle;
				}
			else
				{
				values[index+0] = float.NaN;
				values[index+4] = float.NaN;
				}
			}
		}


	public class Perrinn424Distance : Telemetry.ChannelGroup
		{
		public override int GetChannelCount ()
			{
			return 2;
			}


		public override Telemetry.PollFrequency GetPollFrequency ()
			{
			return Telemetry.PollFrequency.Normal;
			}


		public override void GetChannelInfo (Telemetry.ChannelInfo[] channelInfo, Object instance)
			{
			VehicleBase vehicle = instance as VehicleBase;

			// Custom distance semantic

			var distanceSemantic = new Telemetry.SemanticInfo();
			distanceSemantic.SetRangeAndFormat(0, 21000, "0.000", " km", multiplier:0.001f, quantization:1000);

			// Fill-in channel information

			channelInfo[0].SetNameAndSemantic("LapDistance", Telemetry.Semantic.Custom, distanceSemantic);
			channelInfo[1].SetNameAndSemantic("TotalDistance", Telemetry.Semantic.Custom, distanceSemantic);
			}


		public override void PollValues (float[] values, int index, Object instance)
			{
			VehicleBase vehicle = instance as VehicleBase;

			Telemetry.DataRow latest = vehicle.telemetry.latest;

			values[index+0] = (float)latest.distance;
			values[index+1] = (float)latest.totalDistance;
			}
		}


	public class Perrinn424ForceFeedback : Telemetry.ChannelGroup
		{
		Perrinn424Input m_input;


		public override int GetChannelCount ()
			{
			return 2;
			}


		public override Telemetry.PollFrequency GetPollFrequency ()
			{
			return Telemetry.PollFrequency.Normal;
			}


		public override void GetChannelInfo (Telemetry.ChannelInfo[] channelInfo, Object instance)
			{
			VehicleBase vehicle = instance as VehicleBase;
			m_input = vehicle.GetComponentInChildren<Perrinn424Input>();

			// Fill-in channel information

			channelInfo[0].SetNameAndSemantic("ForceFeedbackForceRatio", Telemetry.Semantic.SignedRatio);
			channelInfo[1].SetNameAndSemantic("ForceFeedbackDamperRatio", Telemetry.Semantic.Ratio);
			}


		public override void PollValues (float[] values, int index, Object instance)
			{
			values[index+0] = float.NaN;
			values[index+1] = float.NaN;

			if (m_input != null && m_input.isActiveAndEnabled)
				{
				InputDevice.ForceFeedback forceFeedback = m_input.GetForceFeedback();

				if (forceFeedback != null)
					{
					values[index+0] = forceFeedback.force? forceFeedback.forceMagnitude : 0.0f;
					values[index+1] = forceFeedback.damper? forceFeedback.damperCoefficient : 0.0f;
					}
				}
			}
		}


	public class Perrinn424Positions : Telemetry.ChannelGroup
		{
		Transform m_transform;


		public override int GetChannelCount ()
			{
			return 6;
			}


		public override Telemetry.PollFrequency GetPollFrequency ()
			{
			return Telemetry.PollFrequency.Normal;
			}


		public override void GetChannelInfo (Telemetry.ChannelInfo[] channelInfo, Object instance)
			{
			VehicleBase vehicle = instance as VehicleBase;
			m_transform = vehicle.cachedRigidbody.transform;

			// Custom position and height semantics

			var positionSemantic = new Telemetry.SemanticInfo();
			positionSemantic.SetRangeAndFormat(-3500, 3500, "0.0", " m", quantization:500);
			var heightSemantic = new Telemetry.SemanticInfo();
			heightSemantic.SetRangeAndFormat(-200, 200, "0.00", " m", quantization:20);
			var yawSemantic = new Telemetry.SemanticInfo();
			yawSemantic.SetRangeAndFormat(0, 360, "0.0", "°", quantization:20);

			channelInfo[0].SetNameAndSemantic("PositionX", Telemetry.Semantic.Custom, positionSemantic);
			channelInfo[1].SetNameAndSemantic("PositionY", Telemetry.Semantic.Custom, heightSemantic);
			channelInfo[2].SetNameAndSemantic("PositionZ", Telemetry.Semantic.Custom, positionSemantic);
			channelInfo[3].SetNameAndSemantic("RotationX", Telemetry.Semantic.BankAngle);
			channelInfo[4].SetNameAndSemantic("RotationY", Telemetry.Semantic.Custom, yawSemantic);
			channelInfo[5].SetNameAndSemantic("RotationZ", Telemetry.Semantic.BankAngle);
			}


		public override void PollValues (float[] values, int index, Object instance)
			{
			Vector3 position = m_transform.position;
			values[index+0] = position.x;
			values[index+1] = position.y;
			values[index+2] = position.z;

			Vector3 rotation = m_transform.rotation.eulerAngles;
			float pitch = rotation.x;
			if (pitch > 180) pitch -= 360;
			float roll = rotation.z;
			if (roll > 180) roll -= 360;

			values[index+3] = pitch;
			values[index+4] = rotation.y;
			values[index+5] = roll;
			}
		}
	}

}