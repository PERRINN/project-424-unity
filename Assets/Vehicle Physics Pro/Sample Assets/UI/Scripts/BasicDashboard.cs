//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;


namespace VehiclePhysics.UI
{

public class BasicDashboard : MonoBehaviour
	{
	public VehicleBase target;
	[Header("UI")]
	public Text speedKph;
	public Text speedMph;
	public Text gear;
	public Text rpm;


	void Update ()
		{
		if (target == null) return;

		int[] vehicleData = target.data.Get(Channel.Vehicle);

		// Speed

		float speed = vehicleData[VehicleData.Speed] / 1000.0f;

		if (speedKph != null)
			speedKph.text = (speed * 3.6f).ToString("0");

		if (speedMph != null)
			speedMph.text = (speed * 2.237f).ToString("0") + " mph";

		// Gear

		if (gear != null)
			{
			int gearId = vehicleData[VehicleData.GearboxGear];
			bool switchingGear = vehicleData[VehicleData.GearboxShifting] != 0;

			if (gearId == 0)
				gear.text = switchingGear? " " : "N";
			else
			if (gearId > 0)
				gear.text = gearId.ToString();
			else
				{
				if (gearId == -1)
					gear.text = "R";
				else
					gear.text = "R" + (-gearId).ToString();
				}
			}

		// Rpm

		if (rpm != null)
			{
			int rpmValue = vehicleData[VehicleData.EngineRpm] / 1000;
			rpm.text = rpmValue.ToString();
			}
		}
	}

}