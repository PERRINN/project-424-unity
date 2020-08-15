﻿//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;
using System;
using System.Collections.Generic;
using VehiclePhysics.UI;


namespace VehiclePhysics.Timing
{

public class LapTimer : MonoBehaviour
	{
	[Range(1,10)]
	public int sectors = 3;
	public float minLapTime = 30.0f;
	public bool startCounting = true;

	[Space(5)]
	public KeyCode resetKey = KeyCode.Escape;
	public bool enableTestKeys = false;

	[Space(5)]
	public bool showGUI = true;
	public float positionX = -200.0f;
	public float positionY = 8.0f;
	public Font font;

	[Space(5)]
	public TimerDisplay externalDisplay;
	#if !VPP_LIMITED
	public VPReplay replayComponent;
	#endif

	// Non-serialized allows to use DontDestroyOnLoad
	// and the component resetting itself on reloading the scene
	// except for the best time, which is preserved.

	[NonSerialized] int m_currentSector = 0;
	[NonSerialized] float m_trackStartTime = 0.0f;
	[NonSerialized] float m_sectorStartTime = 0.0f;
	[NonSerialized] bool m_invalidLap = false;

	// Latest laps and best lap

	[NonSerialized] List<float> m_laps = new List<float>();
	[NonSerialized] float m_lastTime = 0.0f;
	float m_bestTime = 0.0f;

	// Sector times

	[NonSerialized] float[] m_sectors = new float[0];

	// Text

	GUIStyle m_style = new GUIStyle();
	GUIStyle m_bigStyle = new GUIStyle();



	void OnValidate ()
		{
		if (sectors < 1) sectors = 1;
		UpdateTextProperties();
		}


	void OnEnable ()
		{
		m_sectors = new float[sectors];
		UpdateTextProperties();

		// Time.time is zero on application startup

		if (startCounting)
			m_trackStartTime = Time.time + 0.0001f;

		if (externalDisplay != null)
			externalDisplay.AllocateSectors(sectors);

		// Clear continuity flag

		m_invalidLap = false;
		#if !VPP_LIMITED
		if (replayComponent != null)
			replayComponent.continuityFlag = true;
		#endif
        }


	void Update ()
		{
		if (enableTestKeys)
			{
			if (Input.GetKeyDown(KeyCode.Alpha1)) OnTimerHit(0, Time.time);
			if (Input.GetKeyDown(KeyCode.Alpha2)) OnTimerHit(1, Time.time);
			if (Input.GetKeyDown(KeyCode.Alpha3)) OnTimerHit(2, Time.time);
			}

		if (Input.GetKey(resetKey))
			{
			m_trackStartTime = startCounting? Time.time : 0.0f;
			m_currentSector = 0;
			m_sectorStartTime = 0.0f;
			m_invalidLap = false;
			}

		if (externalDisplay != null)
			{
			externalDisplay.invalidLap = m_invalidLap;

			if (m_trackStartTime > 0.0f)
				{
				float t = Time.time - m_trackStartTime;

				// The contact calculation actually "predicts" the exact time the car will be
				// traspassing the detector, as physics typically report the collision in advance.
				// This causes a briefly negative value of t here. Avoid showing it.

				externalDisplay.lapTime = t > 0.0f? t : 0.0f;
				}
			else
				{
				externalDisplay.lapTime = 0.0f;
				}
			}

		#if !VPP_LIMITED
		// If the replay compromises the replay continuity, invalidate the lap.

		if (replayComponent != null && !replayComponent.continuityFlag)
			InvalidateLap();
		#endif
		}


	void NewLap (float t)
		{
		m_laps.Add(t);

		m_lastTime = t;

		foreach (float lapTime in m_laps)
			{
			if (m_bestTime == 0.0f || lapTime < m_bestTime)
				{
				// Best lap

				m_bestTime = lapTime;
				}
			}

		if (externalDisplay != null)
			{
			// NewLap always is called on valid laps

			externalDisplay.invalidLap = false;
			externalDisplay.lapTime = t;
			externalDisplay.LapPass();
			}
		}


	void SectorPass (int sector, float splitTime)
		{
		if (externalDisplay != null)
			{
			// Nomenclature translation
			//
			// Here:
			//	- "sector 0 pass" is the start line
			//	- "sector 1 pass" means first sector completed
			//	- Sector times are stored per sector
			//
			// There:
			//	- "sector 0" means the time for the first sector
			//	- Sector times are calculated out of current lap split time
			//
			// Thus we send "sector-1 pass" on sector > 0 events using current lap time

			if (sector > 0)
				{
				externalDisplay.lapTime = splitTime;
				externalDisplay.SectorPass(sector - 1);
				}
			}
		}


	public void OnTimerHit (int sector, float hitTime)
		{
		if (!isActiveAndEnabled) return;

		// Debug.Log("Sector hit: " + sector);

		if (sector == 0)
			{
			// Start line

			if (m_currentSector == sectors-1)
				{
				float lapTime = hitTime - m_trackStartTime;

				if (!m_invalidLap && lapTime > minLapTime)
					{
					// Okey - we have a lap time
					// Report last sector and new lap

					SectorPass(sectors, hitTime - m_trackStartTime);
					NewLap(lapTime);

					m_sectors[sectors-1] = hitTime - m_sectorStartTime;
					}
				else
					{
					// Starting or invalid lap

					ClearSectors();
					}
				}
			else
				{
				// Multiple hits at the same sector are allowed

				if (sector != m_currentSector)
					{
					// Bad - missed some sector

					ClearSectors();
					}
				}

			// Anyways: restart timming

			m_currentSector = 0;
			m_trackStartTime = hitTime;
			m_sectorStartTime = m_trackStartTime;
			m_invalidLap = false;

			#if !VPP_LIMITED
			// Also clear continuity flag

			if (replayComponent != null)
				replayComponent.continuityFlag = true;
			#endif
			}
		else
			{
			if (sector == m_currentSector+1)
				{
				// Okey - passed thru the correct sector gate

				m_currentSector = sector;

				float splitTime = hitTime - m_trackStartTime;
				SectorPass(sector, splitTime);

				// Sector times: clean up times on first sector gate

                if (sector == 1)
					{
					ClearSectors();
					}

				m_sectors[sector-1] = hitTime - m_sectorStartTime;
				m_sectorStartTime = hitTime;
				}
			else
				{
				// Multiple hits at the same sector are allowed

				if (sector != m_currentSector)
					{
					// Bad - some sector missing

					m_invalidLap = true;
					ClearSectors();
					}
				}
			}
		}


	public void InvalidateLap ()
		{
		m_invalidLap = true;
		}


	void ClearSectors ()
		{
		for (int i=0,c=m_sectors.Length; i<c; i++)
			m_sectors[i] = 0.0f;
		}


	void OnGUI ()
		{
		if (!showGUI) return;

		string smallText = "";
		string currentTimeText = "";

		if (m_trackStartTime > 0.0f)
			{
			float t = Time.time - m_trackStartTime;

			currentTimeText += m_invalidLap? "**" : "S"+(m_currentSector+1);
			currentTimeText += " " + FormatLapTime(t);
			}

		if (sectors > 1)
			{
			for (int i=0, c=m_sectors.Length; i<c; i++)
				{
				if (m_sectors[i] > 0.0f) smallText += FormatSectorTime(m_sectors[i]);

				smallText += "\n";
				}
			}

		smallText += "\n";
		smallText += "\n\nBest " + (m_bestTime > 0.0f ? FormatLapTime(m_bestTime) : "-") + "\n\n";

		float xPos = positionX < 0? Screen.width + positionX - 180 : positionX;
		float yPos = positionY < 0? Screen.height + positionY - 180 : positionY;

		Rect pos = new Rect(xPos, yPos, 180, 180);

		GUI.Box(pos, "");
		GUI.Label(new Rect (pos.x+16, pos.y+8, pos.width, 40), currentTimeText, m_bigStyle);
		GUI.Label(new Rect (pos.x+16, pos.y+40, pos.height, pos.height), smallText, m_style);

		string lastLap = m_lastTime > 0.0f? FormatLapTime(m_lastTime) : "-";
		GUI.Label(new Rect (pos.x+16, pos.y+116, pos.width, 40), lastLap, m_bigStyle);
		}


	string FormatLapTime (float t)
		{
		int seconds = Mathf.FloorToInt(t);

		int m = seconds / 60;
		int s = seconds % 60;
		int mm = Mathf.RoundToInt(t * 1000.0f) % 1000;

		return string.Format("{0,3}:{1,2:00}:{2,3:000}", m, s, mm);
		}


	string FormatSectorTime (float t)
		{
		if (t > 0.0f)
			{
			if (t > 90.0f)
				return FormatLapTime(t);

			int s =  Mathf.FloorToInt(t);
			int mm = Mathf.RoundToInt(t * 1000.0f) % 1000;

			return string.Format("    {0,2:00}:{1,3:000}", s, mm);
			}

		return "";
		}


	void UpdateTextProperties ()
		{
		m_style.font = font;
		m_style.fontSize = 16;
		m_style.normal.textColor = Color.white;

		m_bigStyle.font = font;
		m_bigStyle.fontSize = 20;
		m_bigStyle.normal.textColor = Color.white;
		}

	}

}