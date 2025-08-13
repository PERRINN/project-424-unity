
using UnityEngine;
using VehiclePhysics;
using EdyCommonTools;
using System;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
using VersionCompatibility;


namespace Perrinn424
{

public class Perrinn424RuntimeSetup : VehicleBehaviour
	{
	[Tooltip("File is expected in the folder \"My Documents > PERRINN 424\"")]
	public string fileName = "RuntimeCarSetup.txt";

	[Tooltip("Read the setup file and apply the changes when the component is enabled")]
	public bool applyOnEnable = true;

	public UnityKey hotKeyApply = UnityKey.R;
	public bool ctrlModifier = true;

	[Header("Debug"), Tooltip("When reading the setup from file, the combined values applied to the vehicle will show here")]
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

		// Convert current setup to a text file

		public string ToSetupFile ()
			{
			// Convert current setup to a list of key=value pairs

			List<(string key, string value)> keyValuePairs = new List<(string key, string value)>();
			FieldInfo[] fields = Array.FindAll(this.GetType().GetFields(), f => f.MemberType == MemberTypes.Field && f.IsPublic && f.FieldType == typeof(float));

			foreach (FieldInfo field in fields)
				{
				string valueStr = ((float)field.GetValue(this)).ToString("0.000", CultureInfo.InvariantCulture);
				keyValuePairs.Add((field.Name, valueStr));
				}

			// Convert the pair to the lines of a setup file

			string str = "";
			foreach ((string key, string value) in keyValuePairs)
				{
				str += $"{key} = {value}".TrimEnd('0').TrimEnd('.');
				str += '\n';
				}

			return str;
			}

		// Load a setup file and assign the valid values to the current setup

		public int FromSetupFile (string setupFile)
			{
			int valuesSet = 0;

			// Convert the text to a list of key=value pairs

			List<(string key, string value)> keyValuePairs = new List<(string key, string value)>();

			string[] pairs = setupFile.Split('\n');
			foreach (string pair in pairs)
				{
				string pairRaw = pair.Trim();
				if (pairRaw != "")
					{
					string[] pairParts = pair.Split('=');
					if (pairParts.Length == 2)
						{
						string key = pairParts[0].Trim();
						string value = pairParts[1].Trim();
						if (key != "" && key[0] != '#' && value != "")
							keyValuePairs.Add((key, value));
						}
					}
				}

			// For each pair, find the corresponding field and assign the value

			FieldInfo[] fields = Array.FindAll(this.GetType().GetFields(), f => f.MemberType == MemberTypes.Field && f.IsPublic && f.FieldType == typeof(float));

			foreach ((string key, string value) in keyValuePairs)
				{
				FieldInfo field = Array.Find(fields, f => f.Name.Equals(key, StringComparison.OrdinalIgnoreCase));
				if (field == null) continue;

				float v;
				if (float.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out v))
					{
					field.SetValue(this, v);
					valuesSet++;
					}
				}

			return valuesSet;
			}
		}


	Perrinn424CarController m_target;
	Perrinn424Aerodynamics m_aero;
	Setup m_backup = null;


	public override void OnEnableComponent ()
		{
		m_target = vehicle as Perrinn424CarController;
		m_aero = GetComponentInChildren<Perrinn424Aerodynamics>();

		m_backup = new Setup();
		ReadSetupFromVehicle(m_backup);
		}


	public override void OnEnableVehicle ()
		{
		if (applyOnEnable)
			ReadAndApplySetupFile();
		}


	public override void OnDisableComponent ()
		{
		WriteSetupToVehicle(m_backup);
		}


	public override void UpdateVehicle ()
		{
		if (UnityInput.GetKeyDown(hotKeyApply) && ctrlModifier == UnityInput.ctrlKeyPressed)
			{
			ReadAndApplySetupFile();
			}
		}


	[ContextMenu("Read Setup From Text File and Apply")]
	public void ReadAndApplySetupFile ()
		{
		if (vehicle == null) return;

		string setupFile = "";

		try
			{
			setupFile = File.ReadAllText(GetFullFilePath());
			}
		catch (Exception)
			{
			return;
			}

		// Take the initial setup as base, then apply the modifications.
		// No modifications restore the initial values.

		ObjectUtility.CopyObjectOverwrite<Setup>(m_backup, ref setup);
		int values = setup.FromSetupFile(setupFile);
		WriteSetupToVehicle(setup);
		Debug.Log($"Perrinn424RuntimeSetup: applied {values} value(s) from [{fileName}]");
		}


	void ReadSetupFromVehicle (Setup setup)
		{
		if (vehicle == null) return;

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
		if (vehicle == null) return;

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


	string GetFullFilePath ()
		{
		string myDocumentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
		return Path.Combine(myDocumentsFolder, "PERRINN 424", fileName);
		}


	[ContextMenu("Debug: Write Debug Setup To Text File")]
	void WriteSetupToTextFile ()
		{
		string setupFile = setup.ToSetupFile();
		try
			{
			File.WriteAllText(GetFullFilePath(), setupFile);
			}
		catch (Exception e)
			{
			Debug.LogWarning($"Error writing setup file: {e.Message}");
			}
		}


	[ContextMenu("Debug: From Vehicle To Debug Setup")]
	void ReadFromVehicle ()
		{
		ReadSetupFromVehicle(setup);
		}


	[ContextMenu("Debug: Debug Setup To Vehicle")]
	void WriteToVehicle ()
		{
		WriteSetupToVehicle(setup);
		}


	[ContextMenu("Debug: Send Debug Setup to Console")]
	void DebugSetupFileInConsole ()
		{
		string setupFile = setup.ToSetupFile();
		Debug.Log(setupFile);
		}
	}

}