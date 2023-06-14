//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;
using System;
using EdyCommonTools;


namespace VehiclePhysics.Timing
{

public struct LapTime
	{
	// Authority time is milliseconds

	int m_timeMs;
	readonly int[] m_sectorMs;


	// Exposed time in seconds

	public bool isZero => m_timeMs == 0;
	public float time => MsToTime(m_timeMs);

	// Exposed sectors and sector times in seconds

	public int sectorCount => m_sectorMs != null? m_sectorMs.Length : 0;
	public float Sector (int s) => s >= 0 && s < sectorCount? MsToTime(m_sectorMs[s]) : 0.0f;

	// Exposed time and sectors in ms

	public int timeMs { get => m_timeMs; set { m_timeMs = value; } }
	public int[] sectorMs => m_sectorMs;


	// Create lap time

	public LapTime (float time = 0.0f, int sectors = 3)
		{
		if (sectors < 0) sectors = 0;

		m_timeMs = TimeToMs(time);
		m_sectorMs = new int[sectors];
		}

	public LapTime Copy ()
		{
		LapTime newLap = new LapTime(sectors: sectorCount);
		newLap.m_timeMs = m_timeMs;
		if (m_sectorMs != null) // Constructor might not have been called yet.
			Array.Copy(m_sectorMs, newLap.m_sectorMs, sectorCount);
		return newLap;
		}


	// Set lap data

	public void SetTime (float time)
		{
		m_timeMs = TimeToMs(time);
		}

	public void SetSector (int s, float time)
		{
		if (s >= 0 && s < sectorCount)
			m_sectorMs[s] = TimeToMs(time);
		}

	public void SetSectors (float[] sectors)
		{
		int c = Mathf.Min(sectorCount, sectors != null? sectors.Length : 0);

		for (int s = 0; s < c; s++)
			m_sectorMs[s] = TimeToMs(sectors[s]);
		}

	public void SetSectorsMs (int[] sectors)
		{
		int c = Mathf.Min(sectorCount, sectors != null? sectors.Length : 0);

		for (int s = 0; s < c; s++)
			m_sectorMs[s] = sectors[s];
		}


	// Utility methods

	public void RecalculateFromSectors ()
		{
		int timeMs = 0;

		for (int s = 0; s < sectorCount; s++)
			timeMs += m_sectorMs[s];

		m_timeMs = timeMs;
		}

	public void MergeBestSectors (LapTime otherLap)
		{
		int c = Mathf.Min(sectorCount, otherLap.sectorCount);

		for (int s = 0; s < c; s++)
			{
			if (m_sectorMs[s] > 0 && otherLap.m_sectorMs[s] > 0
				&& m_sectorMs[s] > otherLap.m_sectorMs[s])
				m_sectorMs[s] = otherLap.m_sectorMs[s];
			}
		}


	// Static coversion methods

	public static int TimeToMs (float time)
		{
		return Mathf.RoundToInt(time * 1000.0f);
		}

	public static float MsToTime (int timeMs)
		{
		return timeMs / 1000.0f;
		}


	// Formatting methods

	public string FormatTime ()
		{
		return StringUtility.FormatTime(time, baseUnits: StringUtility.BaseUnits.Minutes);
		}

	public string FormatSector (int s)
		{
		if (s >= 0 && s < sectorCount)
			return StringUtility.FormatTime(MsToTime(m_sectorMs[s]), autoExpandFormat:false);
		else
			return "";
		}

	public override string ToString ()
		{
		string str = FormatTime();

		if (sectorCount > 0)
			{
			str += " ";
			for (int s = 0, c = sectorCount; s < c; s++)
				str += $" {FormatSector(s)}";
			}

		return str;
		}
	}

}
