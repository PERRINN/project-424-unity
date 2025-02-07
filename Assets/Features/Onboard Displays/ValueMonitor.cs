
using UnityEngine;
using UnityEngine.UI;


namespace VehiclePhysics.UI
{

public class ValueMonitor : VehicleBehaviour
	{
	public string throttleChannel = "Throttle";
	public Image throttleBar;
	public Text throttleText;
	[Space(5)]
	public string brakeChannel = "Brake";
	public Image brakeBar;
	public Text brakeText;
	[Space(5)]
	public string batteryChannel = "BatteryTemperature";
	public Image batteryBar;
	public Text batteryText;
	public float maxBatteryTemp = 75.0f;
	[Space(5)]
	public float refreshInterval = 0.05f;


	int m_throttleId;
	int m_brakeId;
	int m_batteryId;
	float m_nextRefreshTime;


	public override void OnEnableVehicle ()
		{
		m_throttleId = -1;
		m_brakeId = -1;
		m_batteryId = -1;
		m_nextRefreshTime = 0.0f;
		}


	public override void UpdateAfterFixedUpdate ()
		{
		if (Time.time >= m_nextRefreshTime)
			{
			m_nextRefreshTime = Time.time + refreshInterval;
			UpdateChannelIds();
			UpdateValues();
			}
		}


	void UpdateValues ()
		{
		float[] values = vehicle.telemetry.latest.values;

		if (m_throttleId >= 0)
			{
			float throttle = values[m_throttleId];
			UITools.SetText(throttleText, $"{throttle*100:F0}");
			UITools.SetImageFill(throttleBar, throttle);
			}

		if (m_brakeId >= 0)
			{
			float brake = values[m_brakeId];
			UITools.SetText(brakeText, $"{brake*100:F0}");
			UITools.SetImageFill(brakeBar, brake);
			}

		if (m_batteryId >= 0)
			{
			float battery = values[m_batteryId];
			if (!float.IsNaN(battery))
				{
				UITools.SetText(batteryText, $"{battery:F1}°");
				UITools.SetImageFill(batteryBar, Mathf.Clamp01(battery / maxBatteryTemp));
				}
			else
				{
				UITools.SetText(batteryText, "-");
				UITools.SetImageFill(batteryBar, 0.0f);
				}
			}
		}


	public void UpdateChannelIds ()
		{
		if (m_throttleId < 0)
			m_throttleId = vehicle.telemetry.GetChannelIndex(throttleChannel);

		if (m_brakeId < 0)
			m_brakeId = vehicle.telemetry.GetChannelIndex(brakeChannel);

		if (m_batteryId < 0)
			m_batteryId = vehicle.telemetry.GetChannelIndex(batteryChannel);
		}
	}

}
