

using UnityEngine;
using System.Text;
using VehiclePhysics;
using EdyCommonTools;


public class Perrinn424MguTelemetry : VehicleBehaviour
	{
	public GUITextBox.Settings overlay = new GUITextBox.Settings();

	// Trick to configure a default font to the telemetry box. Configure it at the script settings.
	[HideInInspector] public Font defaultFont;

	GUITextBox m_textBox = new GUITextBox();
	StringBuilder m_text = new StringBuilder(1024);
	int m_fixedStep;
	int m_lastFixedStep;


	public override void OnEnableVehicle ()
		{
		m_fixedStep = 0;
		m_lastFixedStep = -1;

		if (overlay.font == null)
			overlay.font = defaultFont;

		m_textBox.settings = overlay;
		m_textBox.header = "424 Telemetry";
		}


	public override void FixedUpdateVehicle()
		{
		m_fixedStep++;
		}


	public override void UpdateVehicle ()
		{
		if (m_fixedStep > m_lastFixedStep)
			{
			UpdateTelemetryText();
			m_lastFixedStep = m_fixedStep;
			}
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

		m_textBox.UpdateText(m_text.ToString());
		}


	string GetBalanceStr (float front, float rear)
		{
		// This also covers front == rear == 0
		if (front == rear) return "50.0";
		if (front != 0.0f && rear != 0.0f && Mathf.Sign(front) != Mathf.Sign(rear)) return "-  ";

		return (front / (front + rear) * 100).ToString("0.0");
		}


	void OnGUI ()
		{
		m_textBox.OnGUI();
		}

	}

