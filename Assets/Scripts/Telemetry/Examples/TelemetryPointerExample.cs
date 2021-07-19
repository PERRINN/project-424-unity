
using UnityEngine;
using VehiclePhysics;
using EdyCommonTools;
using System.Text;


public class TelemetryPointerExample : VehicleBehaviour
	{
    public GUITextBox.Settings widget = new GUITextBox.Settings();

	// Private members

	VPTelemetryDisplay m_telemetryDisplay;
	GUITextBox m_textBox = new GUITextBox();
	StringBuilder m_text = new StringBuilder(1024);

    int m_positionXChannel;
    int m_positionYChannel;
    int m_positionZChannel;


	public override void OnEnableVehicle ()
		{
		// Locate the VPTelemetryDisplay component

		m_telemetryDisplay = vehicle.GetComponentInChildren<VPTelemetryDisplay>();
		if (m_telemetryDisplay == null)
			{
			enabled = false;
			return;
			}

		// Initialize

		m_textBox.settings = widget;
		m_textBox.header = "Telemetry Pointer Example";
		m_positionXChannel = -1;
		}


	public override void UpdateAfterFixedUpdate ()
		{
		string text = "";

		// Find the channel IDs. Using PositionX to monitor initialization.

		if (m_positionXChannel < 0)
			{
			m_positionXChannel = vehicle.telemetry.GetChannelIndex("PositionX");
			m_positionYChannel = vehicle.telemetry.GetChannelIndex("PositionY");
			m_positionZChannel = vehicle.telemetry.GetChannelIndex("PositionZ");
			}

		// Get the telemetry entry pointed by the cursor in the telemetry display

		int telemetryEntry = m_telemetryDisplay.GetPointerEntry();
		if (telemetryEntry >= 0)
			{
			// Retrieve channel values from the pointed entry

			float positionX = m_telemetryDisplay.GetChannelValue(m_positionXChannel, telemetryEntry);
			float positionY = m_telemetryDisplay.GetChannelValue(m_positionYChannel, telemetryEntry);
			float positionZ = m_telemetryDisplay.GetChannelValue(m_positionZChannel, telemetryEntry);

			// If all three values are available, use them

			if (!float.IsNaN(positionX) && !float.IsNaN(positionY) && !float.IsNaN(positionZ))
				{
				// We have three valid values. Show them

				string posXvalue = vehicle.telemetry.FormatChannelValue(m_positionXChannel, positionX);
				string posYvalue = vehicle.telemetry.FormatChannelValue(m_positionYChannel, positionY);
				string posZvalue = vehicle.telemetry.FormatChannelValue(m_positionZChannel, positionZ);
				text = $"PositionX: {posXvalue}\nPositionY: {posYvalue}\nPositionZ: {posZvalue}";
				}
			else
				{
				// Some value is not available (NaN)

				text = "PositionX: n/a\nPositionY: n/a\nPositionZ: n/a";
				}
			}
		else
			{
			// Pointer is not in use (i.e. LIVE mode or the pointer is not over the telemetry data).

			text = "PositionX: -\nPositionY: -\nPositionZ: -";
			}

		// Pass the text to the widget

		m_textBox.text = text;
		}


	void OnGUI ()
		{
		m_textBox.OnGUI();
		}
	}
