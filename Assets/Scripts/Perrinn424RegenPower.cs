
using System;
using UnityEngine;
﻿using VehiclePhysics;
using VehiclePhysics.Timing;


namespace Perrinn424
{

public class Perrinn424RegenPower : VehicleBehaviour
	{
	[System.Serializable]
	public class RegenPowerArray
		{
		public float segmentStart = 0.0f;
		public float segmentEnd = 500.0f;

		[Range(0,1)]
		public float regenPowerGain = 1.0f;
		}

	public bool emitTelemetry = true;

	// Regen power segments configuration
	[Space(5)]
	public RegenPowerArray[] regenPowerSegments = new RegenPowerArray[0];


	// Private members
	Perrinn424CarController m_vehicle;
	float m_currentGain = 1.0f;
	LapTimer m_lapTimer = null;
	bool m_lapStarted;


	float GetRegenPowerGain (float lapDistance)
		{
		for (int i = 0; i < regenPowerSegments.Length; i++)
			{
			if (lapDistance > regenPowerSegments[i].segmentStart && lapDistance < regenPowerSegments[i].segmentEnd)
				return regenPowerSegments[i].regenPowerGain;
			}

		return 1.0f;
		}


	void BeginLap ()
		{
		m_lapStarted = true;
		}


	public override void OnEnableVehicle()
		{
		m_vehicle = vehicle as Perrinn424CarController;

		m_lapTimer = FindObjectOfType<LapTimer>();
		if (m_lapTimer != null)
			{
			m_lapTimer.onBeginLap += BeginLap;
			m_lapStarted = false;
			}
		else
			{
			m_lapStarted = true;
			}
		}


	public override void OnDisableVehicle ()
		{
		if (m_lapTimer != null)
			m_lapTimer.onBeginLap -= BeginLap;
		}


	public override void FixedUpdateVehicle()
		{
		// Getting traveled distance in current lap

		Telemetry.DataRow telemetryDataRow = vehicle.telemetry.latest;
		float distance = m_lapStarted? (float)telemetryDataRow.distance : 0.0f;

		// check if car is in regen power gain segment and return the override value

		m_currentGain = GetRegenPowerGain(distance);

		// Apply regen power override to the vehicle

		m_vehicle.regenPowerGain = m_currentGain;
		}


	// Telemetry


	public override bool EmitTelemetry()
		{
		return emitTelemetry;
		}


	public override void RegisterTelemetry()
		{
		vehicle.telemetry.Register<RegenPowerTelemetry>(this);
		}


	public override void UnregisterTelemetry()
		{
		vehicle.telemetry.Unregister<RegenPowerTelemetry>(this);
		}


	public class RegenPowerTelemetry : Telemetry.ChannelGroup
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

			channelInfo[0].SetNameAndSemantic("RegenPowerGain", Telemetry.Semantic.Ratio);
			}


		public override void PollValues(float[] values, int index, UnityEngine.Object instance)
			{
			Perrinn424RegenPower regenPower = instance as Perrinn424RegenPower;

			values[index + 0] = regenPower.m_currentGain;
			}
		}
	}

}