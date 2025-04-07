
// Ad-hoc component for costdown measurement.
//	- Play the scene and set D mode.
//	- Exit play mode to stop recording the data to the CSV file.
//	- File is stored in the root of the project (the parent of the Assets folder).


using UnityEngine;
using VehiclePhysics;
using EdyCommonTools;
using Perrinn424;


public class CostdownTest : VehicleBehaviour
	{
	public float speedKph = 200.0f;
	public string fileName = "CostdownTest.csv";


	Perrinn424Input m_input;
	CsvFileWriter m_writer;
	float m_startTime;


	public override void OnEnableVehicle ()
		{
		m_input = vehicle.GetComponentInChildren<Perrinn424Input>();

		m_writer = new CsvFileWriter(fileName);
		CsvRow row = new CsvRow();
		row.Add("TIME");
		row.Add("SPEED");
		m_writer.WriteRow(row);

		Application.runInBackground = true;
		}


	public override void OnDisableVehicle ()
		{
		m_writer.Close();
		}


	public override void FixedUpdateVehicle ()
		{
		m_input.externalThrottle = 1.0f;

		if (!m_input.externalLiftAndCoast && vehicle.speed > speedKph / 3.6f)
			{
			m_input.externalLiftAndCoast = true;
			m_startTime = vehicle.time;
			}

		if (m_input.externalLiftAndCoast)
			{
			CsvRow row = new CsvRow();
		 	row.Add(FormatTime(vehicle.time - m_startTime));
			row.Add(FormatSpeed(vehicle.speed));
			m_writer.WriteRow(row);
			}
		}


	string FormatTime (float time)
		{
		return time.ToString("0.000");
		}


	string FormatSpeed (float speed)
		{
		return (speed * 3.6f).ToString("0.0000");
		}
	}
