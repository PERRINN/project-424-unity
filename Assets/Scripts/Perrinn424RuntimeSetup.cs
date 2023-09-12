
using UnityEngine;
using VehiclePhysics;
using System;
using System.Collections.Generic;



namespace Perrinn424
{

public class Perrinn424RuntimeSetup : VehicleBehaviour
	{
	public Setup setup = new Setup();

	[Serializable]
	public class Setup
		{
		public float comZLocation;

		public float packTorqueOverrun;
		public float powerBalanceDrive;
		public float powerBalanceOverrun;
		public float powerBalanceRegen;
		public float throttlePedalShape;
		public float brakePedalShape;
		public float hydraulicBrakeGain;
		public float hydraulicBrakeBalance;

		public float frontFlapAngleStatic;
		public float frontFlapAngleDRS;
		public float frontFlapSczAtZeroDeg;
		public float frontFlapSczPerDeg;
		public float frontFlapDeflectionPreload;
		public float frontFlapDeflectionStiffness;
		public float frontFlapDeflectionMax;
		}


	Perrinn424CarController m_target;
	Perrinn424Aerodynamics m_aero;


	public override void OnEnableComponent ()
		{
		m_target = vehicle as Perrinn424CarController;
		m_aero = GetComponentInChildren<Perrinn424Aerodynamics>();
		}


	void ReadSetupFromVehicle (Setup setup)
		{
		if (m_target.centerOfMass != null)
			setup.comZLocation = m_target.centerOfMass.localPosition.z;

		if (m_target.torqueMap != null)
			{
			DualMguTorqueMap.Settings torqueMap = (m_target.torqueMap as DualMguTorqueMapAsset).settings;

			setup.packTorqueOverrun = torqueMap.packTorqueOverrun;
			setup.powerBalanceDrive = torqueMap.powerBalanceDrive;
			setup.powerBalanceOverrun = torqueMap.powerBalanceOverrun;
			setup.powerBalanceRegen = torqueMap.powerBalanceRegen;
			setup.throttlePedalShape = torqueMap.throttleShape;
			setup.brakePedalShape = torqueMap.brakeShape;
			setup.hydraulicBrakeGain = torqueMap.hydraulicBrakeGain;
			setup.hydraulicBrakeBalance = torqueMap.hydraulicBrakeBalance;
			}

		if (m_aero != null)
			{
			setup.frontFlapAngleStatic = m_aero.frontFlapStaticAngle;
			setup.frontFlapAngleDRS = m_aero.frontFlapDRSAngle;
			setup.frontFlapSczAtZeroDeg = m_aero.frontFlapSCz0;
			setup.frontFlapSczPerDeg = m_aero.frontFlapSCz_perDeg;
			setup.frontFlapDeflectionPreload = m_aero.frontFlapDeflectionPreload;
			setup.frontFlapDeflectionStiffness = m_aero.frontFlapDeflectionStiffness;
			setup.frontFlapDeflectionMax = m_aero.frontFlapDeflectionMax;
			}
		}


	void WriteSetupToVehicle (Setup setup)
		{
		if (m_target.centerOfMass != null)
			{
			Vector3 localCom = m_target.centerOfMass.localPosition;
			localCom.z = setup.comZLocation;
			m_target.centerOfMass.localPosition = localCom;
			}

		if (m_target.torqueMap != null)
			{
			DualMguTorqueMap.Settings torqueMap = (m_target.torqueMap as DualMguTorqueMapAsset).settings;

			torqueMap.packTorqueOverrun = setup.packTorqueOverrun;
			torqueMap.powerBalanceDrive = setup.powerBalanceDrive;
			torqueMap.powerBalanceOverrun = setup.powerBalanceOverrun;
			torqueMap.powerBalanceRegen = setup.powerBalanceRegen;
			torqueMap.throttleShape = setup.throttlePedalShape;
			torqueMap.brakeShape = setup.brakePedalShape;
			torqueMap.hydraulicBrakeGain = setup.hydraulicBrakeGain;
			torqueMap.hydraulicBrakeBalance = setup.hydraulicBrakeBalance;
			}

		if (m_aero != null)
			{
			m_aero.frontFlapStaticAngle = setup.frontFlapAngleStatic;
			m_aero.frontFlapDRSAngle = setup.frontFlapAngleDRS;
			m_aero.frontFlapSCz0 = setup.frontFlapSczAtZeroDeg;
			m_aero.frontFlapSCz_perDeg = setup.frontFlapSczPerDeg;
			m_aero.frontFlapDeflectionPreload = setup.frontFlapDeflectionPreload;
			m_aero.frontFlapDeflectionStiffness = setup.frontFlapDeflectionStiffness;
			m_aero.frontFlapDeflectionMax = setup.frontFlapDeflectionMax;
			}
		}


	[ContextMenu("Read From Vehicle")]
	void ReadFromVehicle()
		{
		if (vehicle != null) ReadSetupFromVehicle(setup);
		}

	[ContextMenu("Write To Vehicle")]
	void WriteToVehicle()
		{
		if (vehicle != null) WriteSetupToVehicle(setup);
		}
	}

}