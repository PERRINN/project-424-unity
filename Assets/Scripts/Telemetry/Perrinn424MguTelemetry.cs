

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
			return 16;
			}


		public override Telemetry.PollFrequency GetPollFrequency ()
			{
			return Telemetry.PollFrequency.Normal;
			}


		public override void GetChannelInfo (Telemetry.ChannelInfo[] channelInfo, Object instance)
			{
			channelInfo[0].SetNameAndSemantic("MotorRpmFront", Telemetry.Semantic.EngineRpm);
			channelInfo[1].SetNameAndSemantic("MotorRpmRear", Telemetry.Semantic.EngineRpm);
			channelInfo[2].SetNameAndSemantic("ElectricalPowerFront", Telemetry.Semantic.EnginePower);
			channelInfo[3].SetNameAndSemantic("ElectricalPowerRear", Telemetry.Semantic.EnginePower);
			channelInfo[4].SetNameAndSemantic("ElectricalPowerTotal", Telemetry.Semantic.EnginePower);
			channelInfo[5].SetNameAndSemantic("PowerBalanceFeedForward", Telemetry.Semantic.Ratio);
			channelInfo[6].SetNameAndSemantic("PowerBalanceOffset", Telemetry.Semantic.Ratio);
			channelInfo[7].SetNameAndSemantic("PowerBalance", Telemetry.Semantic.Ratio);
			channelInfo[8].SetNameAndSemantic("PowerBalanceDelivered", Telemetry.Semantic.Ratio);
			channelInfo[9].SetNameAndSemantic("WheelTorqueBalance", Telemetry.Semantic.Ratio);
			channelInfo[10].SetNameAndSemantic("EfficiencyFront", Telemetry.Semantic.Ratio);
			channelInfo[11].SetNameAndSemantic("EfficiencyRear", Telemetry.Semantic.Ratio);
			channelInfo[12].SetNameAndSemantic("MguTorqueFront", Telemetry.Semantic.EngineTorque);
			channelInfo[13].SetNameAndSemantic("MguTorqueRear", Telemetry.Semantic.EngineTorque);
			channelInfo[14].SetNameAndSemantic("MguRotorTorqueFront", Telemetry.Semantic.EngineTorque);
			channelInfo[15].SetNameAndSemantic("MguRotorTorqueRear", Telemetry.Semantic.EngineTorque);
			}


		public override void PollValues (float[] values, int index, Object instance)
			{
			VehicleBase vehicle = instance as VehicleBase;
			int[] custom = vehicle.data.Get(Channel.Custom);

			float frontRpm = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.Rpm] / 1000.0f;
			float frontEfficiency = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.Efficiency] / 1000.0f;
			float frontElectricalPower = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.ElectricalPower] / 1000.0f;
			float frontMguTorque = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.MguTorque] / 1000.0f;
			float frontMguRotorTorque = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.MguRotorTorque] / 1000.0f;
			float frontWheelsTorque = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.WheelsTorque] / 1000.0f;

			float rearRpm = custom[Perrinn424Data.RearMguBase + Perrinn424Data.Rpm] / 1000.0f;
			float rearEfficiency = custom[Perrinn424Data.RearMguBase + Perrinn424Data.Efficiency] / 1000.0f;
			float rearElectricalPower = custom[Perrinn424Data.RearMguBase + Perrinn424Data.ElectricalPower] / 1000.0f;
			float rearMguTorque = custom[Perrinn424Data.RearMguBase + Perrinn424Data.MguTorque] / 1000.0f;
			float rearMguRotorTorque = custom[Perrinn424Data.RearMguBase + Perrinn424Data.MguRotorTorque] / 1000.0f;
			float rearWheelsTorque = custom[Perrinn424Data.RearMguBase + Perrinn424Data.WheelsTorque] / 1000.0f;

			float powerBalanceFeedForward = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.PowerBalanceFeedForward] / 1000.0f;
			float powerBalance = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.PowerBalance] / 1000.0f;

			values[index+0] = frontRpm;
			values[index+1] = rearRpm;
			values[index+2] = frontElectricalPower;
			values[index+3] = rearElectricalPower;
			values[index+4] = frontElectricalPower + rearElectricalPower;
			values[index+5] = powerBalanceFeedForward;
			values[index+6] = powerBalance - powerBalanceFeedForward;
			values[index+7] = powerBalance;
			values[index+8] = GetBalance(frontElectricalPower, rearElectricalPower);
			values[index+9] = GetBalance(frontWheelsTorque, rearWheelsTorque);
			values[index+10] = frontEfficiency;
			values[index+11] = rearEfficiency;
			values[index+12] = frontMguTorque;
			values[index+13] = rearMguTorque;
			values[index+14] = frontMguRotorTorque;
			values[index+15] = rearMguRotorTorque;
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

		int[] custom = vehicle.data.Get(Channel.Custom);

		float throttlePosition = custom[Perrinn424Data.ThrottlePosition] / 1000.0f;
		float brakePosition = custom[Perrinn424Data.BrakePosition] / 1000.0f;

		float frontRpm = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.Rpm] / 1000.0f;
		float frontLoad = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.Load] / 1000.0f;
		float frontBalance = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.PowerBalance] / 1000.0f;
		float frontEfficiency = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.Efficiency] / 1000.0f;
		float frontElectricalPower = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.ElectricalPower] / 1000.0f;
		float frontMguTorque = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.MguTorque] / 1000.0f;
		float frontMguStatorTorque = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.MguStatorTorque] / 1000.0f;
		float frontMguRotorTorque = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.MguRotorTorque] / 1000.0f;
		float frontShaftsTorque = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.ShaftsTorque] / 1000.0f;
		float frontWheelsTorque = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.WheelsTorque] / 1000.0f;

		float rearRpm = custom[Perrinn424Data.RearMguBase + Perrinn424Data.Rpm] / 1000.0f;
		float rearLoad = custom[Perrinn424Data.RearMguBase + Perrinn424Data.Load] / 1000.0f;
		float rearBalance = custom[Perrinn424Data.RearMguBase + Perrinn424Data.PowerBalance] / 1000.0f;
		float rearEfficiency = custom[Perrinn424Data.RearMguBase + Perrinn424Data.Efficiency] / 1000.0f;
		float rearElectricalPower = custom[Perrinn424Data.RearMguBase + Perrinn424Data.ElectricalPower] / 1000.0f;
		float rearMguTorque = custom[Perrinn424Data.RearMguBase + Perrinn424Data.MguTorque] / 1000.0f;
		float rearMguStatorTorque = custom[Perrinn424Data.RearMguBase + Perrinn424Data.MguStatorTorque] / 1000.0f;
		float rearMguRotorTorque = custom[Perrinn424Data.RearMguBase + Perrinn424Data.MguRotorTorque] / 1000.0f;
		float rearShaftsTorque = custom[Perrinn424Data.RearMguBase + Perrinn424Data.ShaftsTorque] / 1000.0f;
		float rearWheelsTorque = custom[Perrinn424Data.RearMguBase + Perrinn424Data.WheelsTorque] / 1000.0f;

		// Compose text

		m_text.Clear();
		m_text.Append("                      Throttle      Brake    \n");
		m_text.Append($"Pedal Position          {throttlePosition*100,3:0.} %       {brakePosition*100,3:0.} %   \n\n");
		m_text.Append("                     MGU Front    MGU Rear    Balance (%)\n");
		m_text.Append($"Rpm                   {frontRpm,6:0.}      {rearRpm,6:0.}        {GetBalanceStr(frontRpm, rearRpm),5}\n");
		m_text.Append($"Load (%)              {frontLoad*100,6:0.0}      {rearLoad*100,6:0.0}        {GetBalanceStr(frontLoad, rearLoad),5}\n\n");
		m_text.Append($"Electrical Power (kW) {frontElectricalPower,6:0.0}      {rearElectricalPower,6:0.0}        {GetBalanceStr(frontElectricalPower, rearElectricalPower),5}\n");
		m_text.Append($"Efficiency (%)        {frontEfficiency*100,6:0.0}      {rearEfficiency*100,6:0.0}        {GetBalanceStr(frontEfficiency, rearEfficiency),5}\n");
		m_text.Append($"Power Balance (%)     {frontBalance*100,6:0.0}      {rearBalance*100,6:0.0}        \n\n");
		m_text.Append($"Mgu Torque (Nm)       {frontMguTorque,6:0.}      {rearMguTorque,6:0.}        {GetBalanceStr(frontMguTorque, rearMguTorque),5}\n");
		m_text.Append($"Stator (Nm)           {frontMguStatorTorque,6:0.}      {rearMguStatorTorque,6:0.}        {GetBalanceStr(frontMguStatorTorque, rearMguStatorTorque),5}\n");
		m_text.Append($"Rotor (Nm)            {frontMguRotorTorque,6:0.}      {rearMguRotorTorque,6:0.}        {GetBalanceStr(frontMguRotorTorque, rearMguRotorTorque),5}\n");
		m_text.Append($"Drive Shafts (Nm) ×2  {frontShaftsTorque,6:0.}      {rearShaftsTorque,6:0.}        {GetBalanceStr(frontShaftsTorque, rearShaftsTorque),5}\n");
		m_text.Append($"Wheels Total (Nm) ×2  {frontWheelsTorque,6:0.}      {rearWheelsTorque,6:0.}        {GetBalanceStr(frontWheelsTorque, rearWheelsTorque),5}");

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

