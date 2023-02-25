

using UnityEngine;
using System.Text;
using VehiclePhysics;
using EdyCommonTools;


public class Perrinn424MguTelemetry : VehicleBehaviour
	{
	public bool emitTelemetry = true;

	public bool showWidget = false;
	public GUITextBox.Settings widget = new GUITextBox.Settings();

	GUITextBox m_textBox = new GUITextBox();
	StringBuilder m_text = new StringBuilder(1024);


	// Telemetry channels


	public override bool EmitTelemetry ()
		{
		return emitTelemetry;
		}


	public override void RegisterTelemetry ()
		{
		vehicle.telemetry.Register<Perrinn424Powertrain>(vehicle);
		}


	public override void UnregisterTelemetry ()
		{
		vehicle.telemetry.Unregister<Perrinn424Powertrain>(vehicle);
		}


	public class Perrinn424Powertrain : Telemetry.ChannelGroup
		{
		public override int GetChannelCount ()
			{
			return 13;
			}


		public override Telemetry.PollFrequency GetPollFrequency ()
			{
			return Telemetry.PollFrequency.Normal;
			}


		public override void GetChannelInfo (Telemetry.ChannelInfo[] channelInfo, Object instance)
			{
			channelInfo[0].SetNameAndSemantic("MotorRpmFront", Telemetry.Semantic.EngineRpm);
			channelInfo[1].SetNameAndSemantic("MotorRpmRear", Telemetry.Semantic.EngineRpm);
			channelInfo[2].SetNameAndSemantic("PowerElectricalFront", Telemetry.Semantic.EnginePower);
			channelInfo[3].SetNameAndSemantic("PowerElectricalRear", Telemetry.Semantic.EnginePower);
			channelInfo[4].SetNameAndSemantic("PowerElectricalTotal", Telemetry.Semantic.EnginePower);
			channelInfo[5].SetNameAndSemantic("PowerBalance", Telemetry.Semantic.Ratio);
			channelInfo[6].SetNameAndSemantic("WheelTorqueBalance", Telemetry.Semantic.Ratio);
			channelInfo[7].SetNameAndSemantic("EfficiencyFront", Telemetry.Semantic.Ratio);
			channelInfo[8].SetNameAndSemantic("EfficiencyRear", Telemetry.Semantic.Ratio);
			channelInfo[9].SetNameAndSemantic("TorqueMechanicalFront", Telemetry.Semantic.EngineTorque);
			channelInfo[10].SetNameAndSemantic("TorqueMechanicalRear", Telemetry.Semantic.EngineTorque);
			channelInfo[11].SetNameAndSemantic("TorqueRotorFront", Telemetry.Semantic.EngineTorque);
			channelInfo[12].SetNameAndSemantic("TorqueRotorRear", Telemetry.Semantic.EngineTorque);
			}


		public override void PollValues (float[] values, int index, Object instance)
			{
			VehicleBase vehicle = instance as VehicleBase;
			int[] custom = vehicle.data.Get(Channel.Custom);

			float frontRpm = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.Rpm] / 1000.0f;
			float frontEfficiency = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.Efficiency] / 1000.0f;
			float frontElectricalPower = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.ElectricalPower] / 1000.0f;
			float frontMechanical = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.MechanicalTorque] / 1000.0f;
			float frontRotor = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.RotorTorque] / 1000.0f;
			float frontWheels = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.WheelsTorque] / 1000.0f;

			float rearRpm = custom[Perrinn424Data.RearMguBase + Perrinn424Data.Rpm] / 1000.0f;
			float rearEfficiency = custom[Perrinn424Data.RearMguBase + Perrinn424Data.Efficiency] / 1000.0f;
			float rearElectricalPower = custom[Perrinn424Data.RearMguBase + Perrinn424Data.ElectricalPower] / 1000.0f;
			float rearMechanical = custom[Perrinn424Data.RearMguBase + Perrinn424Data.MechanicalTorque] / 1000.0f;
			float rearRotor = custom[Perrinn424Data.RearMguBase + Perrinn424Data.RotorTorque] / 1000.0f;
			float rearWheels = custom[Perrinn424Data.RearMguBase + Perrinn424Data.WheelsTorque] / 1000.0f;

			values[index+0] = frontRpm;
			values[index+1] = rearRpm;
			values[index+2] = frontElectricalPower;
			values[index+3] = rearElectricalPower;
			values[index+4] = frontElectricalPower + rearElectricalPower;
			values[index+5] = GetBalance(frontElectricalPower, rearElectricalPower);
			values[index+6] = GetBalance(frontWheels, rearWheels);
			values[index+7] = frontEfficiency;
			values[index+8] = rearEfficiency;
			values[index+9] = frontMechanical;
			values[index+10] = rearMechanical;
			values[index+11] = frontRotor;
			values[index+12] = rearRotor;
			}
		}


	// On-screen widget


	// Trick to assign a default font to the GUI box. Configure it at the script settings.

	[HideInInspector] public Font defaultFont;

	void OnValidate ()
		{
		if (widget.font == null)
			widget.font = defaultFont;
		}


	public override void OnEnableVehicle ()
		{
		m_textBox.settings = widget;
		m_textBox.header = "424 Telemetry";
		}


	public override void UpdateAfterFixedUpdate ()
		{
		if (showWidget)
			UpdateTelemetryText();
		}


	void UpdateTelemetryText ()
		{
		// Gather all data

		int[] input = vehicle.data.Get(Channel.Input);
		int[] custom = vehicle.data.Get(Channel.Custom);

		float throttlePosition = input[InputData.Throttle] / 10000.0f;
		float brakePosition = input[InputData.Brake] / 10000.0f;
		float throttleInput = custom[Perrinn424Data.ThrottleInput] / 1000.0f;
		float brakePressure = custom[Perrinn424Data.BrakePressure] / 1000.0f;

		float frontRpm = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.Rpm] / 1000.0f;
		float frontLoad = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.Load] / 1000.0f;
		float frontEfficiency = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.Efficiency] / 1000.0f;
		float frontPower = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.ElectricalPower] / 1000.0f;
		float frontElectrical = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.ElectricalTorque] / 1000.0f;
		float frontMechanical = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.MechanicalTorque] / 1000.0f;
		float frontStator = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.StatorTorque] / 1000.0f;
		float frontRotor = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.RotorTorque] / 1000.0f;
		float frontShafts = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.ShaftsTorque] / 1000.0f;
		float frontWheels = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.WheelsTorque] / 1000.0f;

		float rearRpm = custom[Perrinn424Data.RearMguBase + Perrinn424Data.Rpm] / 1000.0f;
		float rearLoad = custom[Perrinn424Data.RearMguBase + Perrinn424Data.Load] / 1000.0f;
		float rearEfficiency = custom[Perrinn424Data.RearMguBase + Perrinn424Data.Efficiency] / 1000.0f;
		float rearPower = custom[Perrinn424Data.RearMguBase + Perrinn424Data.ElectricalPower] / 1000.0f;
		float rearElectrical = custom[Perrinn424Data.RearMguBase + Perrinn424Data.ElectricalTorque] / 1000.0f;
		float rearMechanical = custom[Perrinn424Data.RearMguBase + Perrinn424Data.MechanicalTorque] / 1000.0f;
		float rearStator = custom[Perrinn424Data.RearMguBase + Perrinn424Data.StatorTorque] / 1000.0f;
		float rearRotor = custom[Perrinn424Data.RearMguBase + Perrinn424Data.RotorTorque] / 1000.0f;
		float rearShafts = custom[Perrinn424Data.RearMguBase + Perrinn424Data.ShaftsTorque] / 1000.0f;
		float rearWheels = custom[Perrinn424Data.RearMguBase + Perrinn424Data.WheelsTorque] / 1000.0f;

		// Compose text

		m_text.Clear();
		m_text.Append("                    Throttle      Brake    \n");
		m_text.Append($"Pedal Position        {throttlePosition*100,3:0.} %        {brakePosition*100,3:0.} %   \n");
		m_text.Append($"Input                 {throttleInput*100,3:0.} %      {brakePressure,5:0.0} bar \n\n");
		m_text.Append("                    MGU Front    MGU Rear    Balance (%)\n");
		m_text.Append($"Rpm                  {frontRpm,6:0.}      {rearRpm,6:0.}        {GetBalanceStr(frontRpm, rearRpm),5}\n");
		m_text.Append($"Load (%)             {frontLoad*100,6:0.0}      {rearLoad*100,6:0.0}        {GetBalanceStr(frontLoad, rearLoad),5}\n\n");
		m_text.Append($"Electrical (kW)      {frontPower,6:0.0}      {rearPower,6:0.0}        {GetBalanceStr(frontPower, rearPower),5}\n");
		m_text.Append($"Electrical (Nm)      {frontElectrical,6:0.}      {rearElectrical,6:0.}        {GetBalanceStr(frontElectrical, rearElectrical),5}\n");
		m_text.Append($"Efficiency (%)       {frontEfficiency*100,6:0.0}      {rearEfficiency*100,6:0.0}        {GetBalanceStr(frontEfficiency, rearEfficiency),5}  \n\n");
		m_text.Append($"Mechanical (Nm)      {frontMechanical,6:0.}      {rearMechanical,6:0.}        {GetBalanceStr(frontMechanical, rearMechanical),5}\n");
		m_text.Append($"Stator (Nm)          {frontStator,6:0.}      {rearStator,6:0.}        {GetBalanceStr(frontStator, rearStator),5}\n");
		m_text.Append($"Rotor (Nm)           {frontRotor,6:0.}      {rearRotor,6:0.}        {GetBalanceStr(frontRotor, rearRotor),5}\n");
		m_text.Append($"Drive Shafts (Nm) ×2 {frontShafts,6:0.}      {rearShafts,6:0.}        {GetBalanceStr(frontShafts, rearShafts),5}\n");
		m_text.Append($"Wheels Total (Nm) ×2 {frontWheels,6:0.}      {rearWheels,6:0.}        {GetBalanceStr(frontWheels, rearWheels),5}");

		m_textBox.text = m_text.ToString();
		}


	static string GetBalanceStr (float front, float rear)
		{
		// This also covers front == rear == 0
		if (front == rear) return "50.0";
		if (front != 0.0f && rear != 0.0f && Mathf.Sign(front) != Mathf.Sign(rear)) return "-  ";

		return (front / (front + rear) * 100).ToString("0.0");
		}


	static float GetBalance (float front, float rear)
		{
		if (front == rear) return 0.5f;
		if (front != 0.0f && rear != 0.0f && Mathf.Sign(front) != Mathf.Sign(rear)) return float.NaN;
		return front / (front + rear);
		}


	void OnGUI ()
		{
		if (showWidget)
			m_textBox.OnGUI();
		}

	}

