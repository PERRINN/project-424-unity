
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
		vehicle.telemetry.ApplySpecifications();
		}



	public override bool EmitTelemetry ()
		{
		return emitTelemetry;
		}


	public override void RegisterTelemetry ()
		{
		vehicle.telemetry.Register<Perrinn424Inputs>(vehicle);
		}


	public override void UnregisterTelemetry ()
		{
		vehicle.telemetry.Unregister<Perrinn424Inputs>(vehicle);
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

			// Fill-in the channel information.

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
	}
