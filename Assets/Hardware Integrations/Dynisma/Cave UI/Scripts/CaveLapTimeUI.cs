
using UnityEngine;
using UnityEngine.UI;
using VehiclePhysics.Timing;
using EdyCommonTools;


namespace Perrinn424.UI
{

public class CaveLapTimeUI : MonoBehaviour
	{
	public string emptySectorText = "-";
	public string emptyLapText = "-";

	[Space(5)]
	public Text sector1Label;
	public Text sector2Label;
	public Text sector3Label;
	public Text sector4Label;
	public Text sector5Label;
	public Text lapTimeLabel;


	public void SetLapTime (LapTime lapTime)
		{
		UpdateSectorLabel(sector1Label, lapTime, 0);
		UpdateSectorLabel(sector2Label, lapTime, 1);
		UpdateSectorLabel(sector3Label, lapTime, 2);
		UpdateSectorLabel(sector4Label, lapTime, 3);
		UpdateSectorLabel(sector5Label, lapTime, 4);
		UpdateTimeLabel(lapTimeLabel, lapTime);
		}


	void UpdateTimeLabel (Text label, LapTime lapTime)
		{
		if (label != null)
		 	{
			if (lapTime.timeMs > 0)
				label.text = lapTime.FormatTime();
			else
				label.text = emptyLapText;
			}
		}


	void UpdateSectorLabel (Text label, LapTime lapTime, int sector)
		{
		if (label != null)
			{
			if (sector < lapTime.sectorMs.Length && lapTime.sectorMs[sector] > 0)
				label.text = lapTime.FormatSector(sector);
			else
				label.text = emptySectorText;
			}
		}
	}
}