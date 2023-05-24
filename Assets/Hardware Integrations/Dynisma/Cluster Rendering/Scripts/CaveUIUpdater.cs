
using UnityEngine;
using UnityEngine.UI;
using VehiclePhysics.UI;
using EdyCommonTools;


namespace Perrinn424.UI
{

public class CaveUIUpdater : MonoBehaviour
	{
	public LapTimeTable lapTimeTable;

	[Space(5)]
	public Text sector1Label;
	public Text sector2Label;
	public Text sector3Label;
	public Text sector4Label;
	public Text sector5Label;
	public Text lapTimeLabel;


	void Update ()
		{
		if (lapTimeTable != null)
			{
			Utilities.LapTimeTable timeTable = lapTimeTable.timeTable;

			if (timeTable != null)
				{
				Utilities.LapTime idealLap = timeTable.GetIdealLap();
				UpdateLabel(sector1Label, idealLap, 0);
				UpdateLabel(sector2Label, idealLap, 1);
				UpdateLabel(sector3Label, idealLap, 2);
				UpdateLabel(sector4Label, idealLap, 3);
				UpdateLabel(sector5Label, idealLap, 4);
				UpdateLabel(lapTimeLabel, idealLap, 5, fullTime: true);
				}
			}
		}


	void UpdateLabel (Text label, Utilities.LapTime lapTime, int id, bool fullTime = false)
		{
		if (label != null)
			{
			if (lapTime.sectorCount > id)
				{
				label.text = StringUtility.FormatTime(lapTime[id], baseUnits: fullTime? StringUtility.BaseUnits.Minutes : StringUtility.BaseUnits.Seconds);
				}
			else
				{
				label.text = "";
				}
			}
		}
	}
}