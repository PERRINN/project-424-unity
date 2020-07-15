//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System;
using EdyCommonTools;


namespace VehiclePhysics.UI
{

[ExecuteInEditMode]
public class TimerDisplay : MonoBehaviour
	{
	public double lapTime = 0.0f;
	public bool invalidLap = false;

	[Space(5)]
	public float infoDisplayTime = 10.0f;

	[Header("UI")]
	public Text mainLabel;
	public Text infoLabel;
	public Text sectorsLabel;
	public Text bestSectorsLabel;


	int m_lastLapTimeMs = 0;
	int m_bestLapTimeMs = 0;
	string m_lastTimeText = "";
	string m_bestTimeText = "";

	int[] m_bestSectorTimesMs = new int[0];
	int[] m_sectorTimesMs = new int[0];
	bool[] m_isBestSector = new bool[0];
	int m_lastSector = -1;

    float m_tempDisplayTime = 0.0f;


	void OnEnable ()
		{
		m_tempDisplayTime = -infoDisplayTime;
		ResetLapTimes();
		}


	void Update ()
		{
		if (invalidLap)
			m_tempDisplayTime = 0.0f;

		if (Time.unscaledTime - m_tempDisplayTime < infoDisplayTime)
			return;

		// Main display

		string lapTimeText = FormatTime(ToMilliseconds(lapTime));
		if (invalidLap)
			lapTimeText = FormatTextColor(lapTimeText, GColor.red);

		SetLabelText(mainLabel, lapTimeText);
		SetLabelText(infoLabel, "Last  " + m_lastTimeText + "\nBest  " + m_bestTimeText);
		}


	[ContextMenu("Lap Pass")]
	public void LapPass ()
		{
		if (invalidLap) return;

		// Get lap time and compare with best

		int lapTimeMs = ToMilliseconds(lapTime);
		int delta = 0;

		bool hasBestTime = m_bestLapTimeMs > 0;
		bool isBestTime = false;

		if (hasBestTime)
			{
			delta = lapTimeMs - m_bestLapTimeMs;

			if (delta < 0)
				{
				m_bestLapTimeMs = lapTimeMs;
				isBestTime = true;
				}
			}
		else
			{
			m_bestLapTimeMs = lapTimeMs;
			}

		m_lastLapTimeMs = lapTimeMs;
		m_lastTimeText = FormatTime(m_lastLapTimeMs, FormatMode.Lap, 14);
		if (isBestTime) m_lastTimeText = FormatTextColor(m_lastTimeText, GColor.accentGreen);

		// Lap time label

		if (isBestTime)
			SetLabelText(mainLabel, FormatTextColor(FormatTime(lapTimeMs), GColor.accentGreen));
		else
			SetLabelText(mainLabel, FormatTime(lapTimeMs));

		// Delta time

        string secondLine = "";

		if (hasBestTime)
			{
			secondLine = FormatTime(delta, FormatMode.Delta, 14);

			if (delta > 0)
				secondLine = FormatTextColor(secondLine, GColor.red);
			else
			if (delta < 0)
				secondLine = FormatTextColor(secondLine, GColor.accentGreen);
			}

		// Best time

		m_bestTimeText = FormatTime(m_bestLapTimeMs, FormatMode.Lap, 14);

		// If this is the first lap, we leave it running. Otherwise, show the delta with last lap.

		if (hasBestTime)
			{
			SetLabelText(infoLabel, secondLine + "\nBest  " + m_bestTimeText);
			m_tempDisplayTime = Time.unscaledTime;
			}

		UpdateSectors();
		}


	public void SectorPass (int sector)
		{
		if (invalidLap) return;

		if (sector >= 64) return;

		// Ensure to have enough room to store the sector time

		if (sector >= m_bestSectorTimesMs.Length)
			AllocateSectors(sector+1);

		// Get sector time: current lap time minus previous sectors

		int sectorTimeMs = ToMilliseconds(lapTime);
		for (int i = 0; i < sector; i++)
			sectorTimeMs -= m_sectorTimesMs[i];

		// Compare with best pass

		int bestSectorTimeMs = m_bestSectorTimesMs[sector];
		int delta = 0;

		bool hasBestTime = bestSectorTimeMs > 0;

		if (hasBestTime)
			{
			delta = sectorTimeMs - bestSectorTimeMs;

			if (delta < 0)
				{
				bestSectorTimeMs = sectorTimeMs;
				m_bestSectorTimesMs[sector] = bestSectorTimeMs;

				m_isBestSector[sector] = true;
				}
			else
				{
				m_isBestSector[sector] = false;
				}
			}
		else
			{
			// No best time - this is the first time for this sector.

			bestSectorTimeMs = sectorTimeMs;
			m_bestSectorTimesMs[sector] = bestSectorTimeMs;
			m_isBestSector[sector] = false;
			}

		// Store this sector time

		m_sectorTimesMs[sector] = sectorTimeMs;

		// Main label: sector and time

		string mainText = string.Format("S{0}   {1}", sector+1, FormatTime(sectorTimeMs, FormatMode.Sector));

		if (m_isBestSector[sector])
			SetLabelText(mainLabel, FormatTextColor(mainText, GColor.accentGreen));
		else
			SetLabelText(mainLabel, mainText);

		// Second line: delta

        string secondLine = "";

		if (hasBestTime)
			{
			secondLine = FormatTime(delta, FormatMode.Delta, 14);

			if (delta > 0)
				secondLine = FormatTextColor(secondLine, GColor.red);
			else
			if (delta < 0)
				secondLine = FormatTextColor(secondLine, GColor.accentGreen);
			}

		// Third line: best

		string infoText = string.Format("{0}\nBest S{1}   {2}", secondLine, sector+1, FormatTime(bestSectorTimeMs, FormatMode.Sector, 14));
		SetLabelText(infoLabel, infoText);

		m_tempDisplayTime = Time.unscaledTime;

		// Sectors

		m_lastSector = sector;
		UpdateSectors();
		}


	void UpdateSectors ()
		{
		UpdateSectorsLabel(sectorsLabel, m_sectorTimesMs, false);
		UpdateSectorsLabel(bestSectorsLabel, m_bestSectorTimesMs, true);
		}


	void UpdateSectorsLabel (Text label, int[] sectors, bool showAll)
		{
		if (sectors.Length == 0 || label == null) return;

		string sectorsText = "";

		for (int i=0; i<sectors.Length; i++)
			{
			if (i > 0) sectorsText += "\n";

			int time = sectors[i];

			if ((showAll || i <= m_lastSector) && time > 0)
				{
				if (m_isBestSector[i])
					sectorsText += FormatTextColor(FormatTime(time, FormatMode.Sector, 14), GColor.accentGreen);
				else
					sectorsText += FormatTime(time, FormatMode.Sector, 14);
				}
			else
				{
				sectorsText += "-";
				}
			}

		label.text = sectorsText;
		}


	public void AllocateSectors (int sectors)
		{
		Array.Resize(ref m_bestSectorTimesMs, sectors);
		Array.Resize(ref m_sectorTimesMs, sectors);
		Array.Resize(ref m_isBestSector, sectors);
		UpdateSectors();
		}


	[ContextMenu("Sector 1 Pass")]
	public void Sector1Pass ()
		{
		SectorPass(0);
		}

	[ContextMenu("Sector 2 Pass")]
	public void Sector2Pass ()
		{
		SectorPass(1);
		}

	[ContextMenu("Sector 3 Pass")]
	public void Sector3Pass ()
		{
		SectorPass(2);
		}


	[ContextMenu("Reset lap times")]
	public void ResetLapTimes ()
		{
		string emptyTime = System.Text.RegularExpressions.Regex.Unescape("\\u2007\\u2007\\u2007-<size=10>\\u2008\\u2007\\u2007\\u2007</size>");

		m_lastLapTimeMs = 0;
		m_bestLapTimeMs = 0;
		m_lastTimeText = emptyTime;
		m_bestTimeText = emptyTime;

		Array.Clear(m_bestSectorTimesMs, 0, m_bestSectorTimesMs.Length);
		Array.Clear(m_sectorTimesMs, 0, m_sectorTimesMs.Length);
		Array.Clear(m_isBestSector, 0, m_isBestSector.Length);
		m_lastSector = -1;

		UpdateSectors();
		}


	// Internal utilities


	void SetLabelText (Text label, string text)
		{
		if (label != null)
			label.text = text;
		}


	enum FormatMode { Lap, Sector, Delta };


	string FormatTime (int milliseconds, FormatMode mode = FormatMode.Lap, int smallSize = 10)
		{
		bool negative = false;
		if (milliseconds < 0)
			{
			negative = true;
			milliseconds = -milliseconds;
			}

		int seconds = milliseconds / 1000;

		int m = seconds / 60;
		int s = seconds % 60;
		int mm = milliseconds % 1000;

		string prefix = negative? "-" : mode == FormatMode.Delta? "+" : "";

		if (m < 100)
			{
			if (seconds < 100 && mode != FormatMode.Lap)
				{
				return string.Format("{0}{1}<size={2}>.{3,3:000}</size>", prefix, seconds, smallSize, mm);
				}
			else
				{
				return string.Format("{0}{1}:{2,2:00}<size={3}>.{4,3:000}</size>", prefix, m, s, smallSize, mm);
				}
			}
		else
			{
			int h = m / 60;
			m = m % 60;

			int d = mm / 100;
			return string.Format("{0}{1}:{2,2:00}:{3,2:00}<size={4}>.{5}</size>", prefix, h, m, s, smallSize, d);
			}
		}


	string FormatTextColor (string text, Color color)
		{
        return string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB(color), text);
		}


	int ToMilliseconds (double t)
		{
		return RoundTime(t * 1000.0f);
		}


	static int RoundTime (double d)
		{
		if (d < 0)
			return (int)(d - 0.5);

		return (int)(d + 0.5);
		}
	}

}