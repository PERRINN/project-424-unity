
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

		vehicle.telemetry.ApplySpecifications();

		// Adjust specific semantics to our vehicle's capabilities

		Telemetry.SemanticInfo angularVelocitySemantic = vehicle.telemetry.semantics[(int)Telemetry.Semantic.AngularVelocity];
		angularVelocitySemantic.rangeMin = -90.0f * Mathf.Deg2Rad;
		angularVelocitySemantic.rangeMax = 90.0f * Mathf.Deg2Rad;
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
		}


	public override void UnregisterTelemetry ()
		{
		vehicle.telemetry.Unregister<Perrinn424Inputs>(vehicle);
		vehicle.telemetry.Unregister<Perrinn424Differential>(vehicle);
		vehicle.telemetry.Unregister<Perrinn424Chassis>(vehicle);
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

	}
