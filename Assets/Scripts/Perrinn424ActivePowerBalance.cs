using VehiclePhysics;
using UnityEngine;
using System;


public class Perrinn424ActivePowerBalance : VehicleBehaviour
	{
	public float rpmRatioFrontToRear = 1.0f;
	public float gain = 0.0005f;
	public float authority = 0.1f;
	[Space(5)]
	public bool emitTelemetry;

	// Private members

	Perrinn424CarController m_vehicle;
	float m_nAxleDelta;


	public override void OnEnableVehicle ()
		{
		m_vehicle = vehicle as Perrinn424CarController;

		// Use this event so the power balance is applied within current timestep
		m_vehicle.onBeforeIntegrationStep += UpdatePowerBalance;
		}


	public override void OnDisableVehicle ()
		{
		m_vehicle.onBeforeIntegrationStep -= UpdatePowerBalance;
		}


	void UpdatePowerBalance ()
		{
		// Retrieve MGU rpm

		int[] customData = m_vehicle.data.Get(Channel.Custom);
		float frontRpm = customData[Perrinn424Data.FrontMguBase + Perrinn424Data.Rpm] / 1000.0f;
		float rearRpm = customData[Perrinn424Data.RearMguBase + Perrinn424Data.Rpm] / 1000.0f;

		// Compute power balance offset

		m_nAxleDelta = frontRpm * rpmRatioFrontToRear - rearRpm;
		float powerBalanceOffset = Mathf.Max(-authority, Mathf.Min(authority, m_nAxleDelta * (m_vehicle.brakePosition > 0.0f? gain : -gain)));

		// Apply to the vehicle

		m_vehicle.powerBalanceOffset = powerBalanceOffset;
		}


	// Telemetry


	public override bool EmitTelemetry()
		{
		return emitTelemetry;
		}


	public override void RegisterTelemetry()
		{
		vehicle.telemetry.Register<ActivePowerBalanceTelemetry>(this);
		}


	public override void UnregisterTelemetry()
		{
		vehicle.telemetry.Unregister<ActivePowerBalanceTelemetry>(this);
		}


	public class ActivePowerBalanceTelemetry : Telemetry.ChannelGroup
		{
		public override int GetChannelCount()
			{
			return 1;
			}


		public override Telemetry.PollFrequency GetPollFrequency()
			{
			return Telemetry.PollFrequency.Normal;
			}


		public override void GetChannelInfo (Telemetry.ChannelInfo[] channelInfo, UnityEngine.Object instance)
			{
			// Fill-in channel information

			channelInfo[0].SetNameAndSemantic("nAxleDelta", Telemetry.Semantic.EngineRpm);
			}


		public override void PollValues(float[] values, int index, UnityEngine.Object instance)
			{
			Perrinn424ActivePowerBalance speedLimiter = instance as Perrinn424ActivePowerBalance;

			values[index + 0] = speedLimiter.m_nAxleDelta;
			}
		}
	}
