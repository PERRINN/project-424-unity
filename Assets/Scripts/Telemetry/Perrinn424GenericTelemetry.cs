
using UnityEngine;
using VehiclePhysics;


public class Perrinn424GenericTelemetry : VehicleBehaviour
	{
	public bool emitTelemetry = true;


	public override void OnEnableVehicle ()
		{
		// Adjust the vehicle specifications according to what we know about Project 424

		vehicle.telemetry.specs.maxSpeed = 100.0f;
		vehicle.telemetry.specs.maxGearPosition = 1;
		vehicle.telemetry.specs.minGearPosition = -1;
		vehicle.telemetry.specs.maxAcceleration = 5 * Gravity.reference;
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
		angularVelocitySemantic.rangeMin = -90.0f * Mathf.Deg2Rad;
		angularVelocitySemantic.rangeMax = 90.0f * Mathf.Deg2Rad;

		// Weight in newtons (original) instead of kilograms (default)

		Telemetry.SemanticInfo weightSemantic = vehicle.telemetry.semantics[(int)Telemetry.Semantic.Weight];
		weightSemantic.displayMultiplier = 1.0f;
		weightSemantic.displayUnitsSuffix = " N";

		// Max tire forces. TODO: Remove when it's properly applied from specifications above.
		Telemetry.SemanticInfo tireForceSemantic = vehicle.telemetry.semantics[(int)Telemetry.Semantic.TireForce];
		tireForceSemantic.rangeMin = -vehicle.telemetry.specs.maxTireForce;
		tireForceSemantic.rangeMax = vehicle.telemetry.specs.maxTireForce;
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
		}


	public override void UnregisterTelemetry ()
		{
		vehicle.telemetry.Unregister<Perrinn424Inputs>(vehicle);
		vehicle.telemetry.Unregister<Perrinn424Differential>(vehicle);
		vehicle.telemetry.Unregister<Perrinn424Chassis>(vehicle);
		vehicle.telemetry.Unregister<Perrinn424Tires>(vehicle);
		vehicle.telemetry.Unregister<Perrinn424Distance>(vehicle);
		}


	public class Perrinn424Inputs : Telemetry.ChannelGroup
		{
		Steering.Settings m_steering;
		Perrinn424CarController m_controller;


		public override int GetChannelCount ()
			{
			return 4;
			}


		public override float GetPollFrequency ()
			{
			return 50.0f;
			}


		public override void GetChannelInfo (Telemetry.ChannelInfo[] channelInfo, Object instance)
			{
			// Access to information in the vehicle

			VehicleBase vehicle = instance as VehicleBase;
			m_steering = vehicle.GetInternalObject(typeof(Steering.Settings)) as Steering.Settings;
			m_controller = vehicle.GetComponent<Perrinn424CarController>();

			// Using custom semantics not yet available in the built-in collection.
			// TODO: use built-in semantics when available.

			float steeringRange = m_steering.steeringWheelRange * 0.5f;
			var steerAngleSemantic = new Telemetry.SemanticInfo();
			var brakePressureSemantic = new Telemetry.SemanticInfo();
			steerAngleSemantic.SetRangeAndFormat(-steeringRange, steeringRange, "0.0", "°", quantization:50, alternateFormat:"0");
			brakePressureSemantic.SetRangeAndFormat(0, 100, "0.0", " bar", quantization:10, alternateFormat:"0");

			// Fill-in channel information

			// channelInfo[0].SetNameAndSemantic("SteeringAngle", Telemetry.Semantic.SteerAngle);
			// channelInfo[0].SetNameAndSemantic("BrakePressure", Telemetry.Semantic.BrakePressure);
			channelInfo[0].SetNameAndSemantic("Gear", Telemetry.Semantic.Gear);
			channelInfo[1].SetNameAndSemantic("SteeringAngle", Telemetry.Semantic.Custom, steerAngleSemantic);
			channelInfo[2].SetNameAndSemantic("Throttle", Telemetry.Semantic.Ratio);
			channelInfo[3].SetNameAndSemantic("BrakePressure", Telemetry.Semantic.Custom, brakePressureSemantic);
			}


		public override void PollValues (float[] values, int index, Object instance)
			{
			// Gear

			values[index+0] = m_controller.gear;

			// Steer angle

			float steerInput = m_controller.data.Get(Channel.Vehicle, VehicleData.AidedSteer) / 10000.0f;
			float steerAngle = steerInput * m_steering.steeringWheelRange * 0.5f;
			values[index+1] = steerAngle;

			// Throttle and brake

			values[index+2] = m_controller.throttleInput;
			values[index+3] = m_controller.brakePressure;
			}
		}


	public class Perrinn424Differential : Telemetry.ChannelGroup
		{
		VehicleBase.WheelState m_wheelFL;
		VehicleBase.WheelState m_wheelFR;
		VehicleBase.WheelState m_wheelRL;
		VehicleBase.WheelState m_wheelRR;


		public override int GetChannelCount ()
			{
			return 4;
			}


		public override float GetPollFrequency ()
			{
			return 50.0f;
			}


		public override void GetChannelInfo (Telemetry.ChannelInfo[] channelInfo, Object instance)
			{
			VehicleBase vehicle = instance as VehicleBase;

			// Retrieve states for the four monitored wheels

			m_wheelFL = vehicle.wheelState[vehicle.GetWheelIndex(0, VehicleBase.WheelPos.Left)];
			m_wheelFR = vehicle.wheelState[vehicle.GetWheelIndex(0, VehicleBase.WheelPos.Right)];
			int rearAxle = vehicle.GetAxleCount() - 1;
			m_wheelRL = vehicle.wheelState[vehicle.GetWheelIndex(rearAxle, VehicleBase.WheelPos.Left)];
			m_wheelRR = vehicle.wheelState[vehicle.GetWheelIndex(rearAxle, VehicleBase.WheelPos.Right)];

			// Fill-in channel information

			channelInfo[0].SetNameAndSemantic("TorqueDiffFront", Telemetry.Semantic.WheelTorque);
			channelInfo[1].SetNameAndSemantic("TorqueDiffRear", Telemetry.Semantic.WheelTorque);
			channelInfo[2].SetNameAndSemantic("SpeedDiffFront", Telemetry.Semantic.Speed);
			channelInfo[3].SetNameAndSemantic("SpeedDiffRear", Telemetry.Semantic.Speed);
			}


		public override void PollValues (float[] values, int index, Object instance)
			{
			values[index+0] = m_wheelFL.driveTorque - m_wheelFR.driveTorque;
			values[index+1] = m_wheelRL.driveTorque - m_wheelRR.driveTorque;
			values[index+2] = m_wheelFL.angularVelocity * m_wheelFL.wheelCol.radius - m_wheelFR.angularVelocity * m_wheelFR.wheelCol.radius;
			values[index+3] = m_wheelRL.angularVelocity * m_wheelRL.wheelCol.radius - m_wheelRR.angularVelocity * m_wheelRR.wheelCol.radius;
			}
		}


	public class Perrinn424Chassis : Telemetry.ChannelGroup
		{
		public override int GetChannelCount ()
			{
			return 2;
			}


		public override float GetPollFrequency ()
			{
			return 50.0f;
			}


		public override void GetChannelInfo (Telemetry.ChannelInfo[] channelInfo, Object instance)
			{
			channelInfo[0].SetNameAndSemantic("PitchRate", Telemetry.Semantic.AngularVelocity);
			channelInfo[1].SetNameAndSemantic("RollRate", Telemetry.Semantic.AngularVelocity);
			}


		public override void PollValues (float[] values, int index, Object instance)
			{
			VehicleBase vehicle = instance as VehicleBase;
			Vector3 angularVelocity = vehicle.cachedRigidbody.angularVelocity;
			values[index+0] = angularVelocity.x;
			values[index+1] = angularVelocity.z;
			}
		}


	public class Perrinn424Tires : Telemetry.ChannelGroup
		{
		VehicleBase.WheelState m_wheelFL;
		VehicleBase.WheelState m_wheelFR;
		VehicleBase.WheelState m_wheelRL;
		VehicleBase.WheelState m_wheelRR;


		public override int GetChannelCount ()
			{
			return 8;
			}


		public override float GetPollFrequency ()
			{
			return 50.0f;
			}


		public override void GetChannelInfo (Telemetry.ChannelInfo[] channelInfo, Object instance)
			{
			VehicleBase vehicle = instance as VehicleBase;

			// Retrieve states for the four monitored wheels

			m_wheelFL = vehicle.wheelState[vehicle.GetWheelIndex(0, VehicleBase.WheelPos.Left)];
			m_wheelFR = vehicle.wheelState[vehicle.GetWheelIndex(0, VehicleBase.WheelPos.Right)];
			int rearAxle = vehicle.GetAxleCount() - 1;
			m_wheelRL = vehicle.wheelState[vehicle.GetWheelIndex(rearAxle, VehicleBase.WheelPos.Left)];
			m_wheelRR = vehicle.wheelState[vehicle.GetWheelIndex(rearAxle, VehicleBase.WheelPos.Right)];

			// Using custom SlipRatio semantic not yet available in the built-in collection.
			// TODO: use built-in semantic when available.

			var slipRatioSemantic = new Telemetry.SemanticInfo();
			slipRatioSemantic.SetRangeAndFormat(-1.0f, 1.0f, "0.0", " \\%", multiplier:100);

			// Fill-in channel information

			channelInfo[0].SetNameAndSemantic("SlipRatioFL", Telemetry.Semantic.Custom, slipRatioSemantic);
			channelInfo[1].SetNameAndSemantic("SlipRatioFR", Telemetry.Semantic.Custom, slipRatioSemantic);
			channelInfo[2].SetNameAndSemantic("SlipRatioRL", Telemetry.Semantic.Custom, slipRatioSemantic);
			channelInfo[3].SetNameAndSemantic("SlipRatioRR", Telemetry.Semantic.Custom, slipRatioSemantic);
			channelInfo[4].SetNameAndSemantic("SlipAngleFL", Telemetry.Semantic.SlipAngle);
			channelInfo[5].SetNameAndSemantic("SlipAngleFR", Telemetry.Semantic.SlipAngle);
			channelInfo[6].SetNameAndSemantic("SlipAngleRL", Telemetry.Semantic.SlipAngle);
			channelInfo[7].SetNameAndSemantic("SlipAngleRR", Telemetry.Semantic.SlipAngle);
			}


		public override void PollValues (float[] values, int index, Object instance)
			{
			values[index+0] = m_wheelFL.slipRatio;
			values[index+1] = m_wheelFR.slipRatio;
			values[index+2] = m_wheelRL.slipRatio;
			values[index+3] = m_wheelRR.slipRatio;
			values[index+4] = m_wheelFL.slipAngle;
			values[index+5] = m_wheelFR.slipAngle;
			values[index+6] = m_wheelRL.slipAngle;
			values[index+7] = m_wheelRR.slipAngle;
			}
		}


	public class Perrinn424Distance : Telemetry.ChannelGroup
		{
		public override int GetChannelCount ()
			{
			return 2;
			}


		public override float GetPollFrequency ()
			{
			return 50.0f;
			}


		public override void GetChannelInfo (Telemetry.ChannelInfo[] channelInfo, Object instance)
			{
			VehicleBase vehicle = instance as VehicleBase;

			// Custom distance semantic

			var distanceSemantic = new Telemetry.SemanticInfo();
			distanceSemantic.SetRangeAndFormat(0, 21000, "0.000", " km", multiplier:0.001f);

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
	}
