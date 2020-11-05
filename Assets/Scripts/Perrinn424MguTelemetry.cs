

using UnityEngine;
using VehiclePhysics;
using EdyCommonTools;


public class Perrinn424MguTelemetry : VehicleBehaviour
	{
	public GUITextBox.Settings overlay = new GUITextBox.Settings();

	// Trick to apply a default font to the telemetry box. Configure it at the script settings.
	[HideInInspector] public Font defaultFont;

	GUITextBox m_textBox = new GUITextBox();


	public override void OnEnableVehicle ()
		{
		m_textBox.settings = overlay;
		m_textBox.header = "424 Telemetry";

		if (overlay.font == null)
			overlay.font = defaultFont;
		}


	public override void UpdateVehicle ()
		{
		// m_textStyle.font = font;
		// m_textStyle.fontSize = fontSize;
		// m_textStyle.normal.textColor = fontColor;

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

		string text  = "                    Throttle      Brake    \n";
		text += $"Pedal Position        {throttlePosition*100,3:0.} %        {brakePosition*100,3:0.} %   \n";
		text += $"Input                 {throttleInput*100,3:0.} %      {brakePressure,5:0.0} bar \n\n";
		text += "                    MGU Front    MGU Rear    Balance (%)\n";
		text += $"Rpm                  {frontRpm,6:0.}      {rearRpm,6:0.}        {GetBalanceStr(frontRpm, rearRpm),5}\n";
		text += $"Load (%)             {frontLoad*100,6:0.0}      {rearLoad*100,6:0.0}        {GetBalanceStr(frontLoad, rearLoad),5}\n\n";
		text += $"Electrical (kW)      {frontPower,6:0.0}      {rearPower,6:0.0}        {GetBalanceStr(frontPower, rearPower),5}\n";
		text += $"Electrical (Nm)      {frontElectrical,6:0.}      {rearElectrical,6:0.}        {GetBalanceStr(frontElectrical, rearElectrical),5}\n";
		text += $"Efficiency (%)       {frontEfficiency*100,6:0.0}      {rearEfficiency*100,6:0.0}        {GetBalanceStr(frontEfficiency, rearEfficiency),5}  \n\n";
		text += $"Mechanical (Nm)      {frontMechanical,6:0.}      {rearMechanical,6:0.}        {GetBalanceStr(frontMechanical, rearMechanical),5}\n";
		text += $"Stator (Nm)          {frontStator,6:0.}      {rearStator,6:0.}        {GetBalanceStr(frontStator, rearStator),5}\n";
		text += $"Rotor (Nm)           {frontRotor,6:0.}      {rearRotor,6:0.}        {GetBalanceStr(frontRotor, rearRotor),5}\n";
		text += $"Drive Shafts (Nm) ×2 {frontShafts,6:0.}      {rearShafts,6:0.}        {GetBalanceStr(frontShafts, rearShafts),5}\n";
		text += $"Wheels Total (Nm) ×2 {frontWheels,6:0.}      {rearWheels,6:0.}        {GetBalanceStr(frontWheels, rearWheels),5}";

		m_textBox.UpdateText(text);
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
		/*
		// Compute box size

		Vector2 contentSize = m_textStyle.CalcSize(new GUIContent(m_text));
		float margin = m_textStyle.lineHeight * 1.2f;
		float headerHeight = GUI.skin.box.lineHeight;

		m_boxWidth = contentSize.x + margin;
		m_boxHeight = contentSize.y + headerHeight + margin / 2;

		// Compute box position

		float xPos = position.x < 0? Screen.width + position.x - m_boxWidth : position.x;
		float yPos = position.y < 0? Screen.height + position.y - m_boxHeight : position.y;

		// Draw telemetry box

		GUI.Box(new Rect (xPos, yPos, m_boxWidth, m_boxHeight), "424 MGUs Telemetry");
		GUI.Label(new Rect (xPos + margin / 2, yPos + margin / 2 + headerHeight, Screen.width, Screen.height), m_text, m_textStyle);
		*/
		}

	}

