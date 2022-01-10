//--------------------------------------------------------------
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
	public bool debugLog = false;

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

	// Event delegate called on each valid lap registered

	public Action<float, bool, float[], bool[]> onLap;
	public Action<int, float> onSector;
	public Action onBeginLap;

	// Current lap and sector times for visual purposes

	public float currentLapTime => Time.time - m_trackStartTime;
	public float currentSectorTime => Time.time - m_sectorStartTime;

	// Current sectors

	public int currentSector => m_currentSector;
	public IReadOnlyList<float> currentSectors => m_sectors;
	public IReadOnlyList<bool> currentValidSectors => m_validSectors;

	// Non-serialized allows to use DontDestroyOnLoad
	// and the component resetting itself on reloading the scene
	// except for the best time, which is preserved.

	[NonSerialized] int m_currentSector = 0;
	[NonSerialized] float m_trackStartTime = 0.0f;
	[NonSerialized] float m_sectorStartTime = 0.0f;
	[NonSerialized] bool m_invalidSector = false;
	[NonSerialized] bool m_invalidLap = false;

	// Latest laps and best lap

	[NonSerialized] List<float> m_laps = new List<float>();
	[NonSerialized] float m_lastTime = 0.0f;
	float m_bestTime = 0.0f;

	// Sector times

	[NonSerialized] float[] m_sectors = new float[0];
	[NonSerialized] bool[] m_validSectors = new bool[0];

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
		m_validSectors = new bool[sectors];
		UpdateTextProperties();

		// Time.fixedTime is zero on application startup

		if (startCounting)
			m_trackStartTime = Time.fixedTime + 0.0001f;

		if (externalDisplay != null)
			externalDisplay.AllocateSectors(sectors);

		// Clear continuity flag

		m_invalidLap = false;
		m_invalidSector = false;
		#if !VPP_LIMITED
		if (replayComponent != null)
			replayComponent.continuityFlag = true;
		#endif
		}


	void Update ()
		{
		if (enableTestKeys)
			{
			if (Input.GetKeyDown(KeyCode.Alpha1)) DebugOnTimerHit(0, Time.fixedTime, 0.0f);
			if (Input.GetKeyDown(KeyCode.Alpha2)) DebugOnTimerHit(1, Time.fixedTime, 0.0f);
			if (Input.GetKeyDown(KeyCode.Alpha3)) DebugOnTimerHit(2, Time.fixedTime, 0.0f);
			if (Input.GetKeyDown(KeyCode.Alpha4)) DebugOnTimerHit(3, Time.fixedTime, 0.0f);
			if (Input.GetKeyDown(KeyCode.Alpha5)) DebugOnTimerHit(4, Time.fixedTime, 0.0f);
			if (Input.GetKeyDown(KeyCode.Alpha6)) DebugOnTimerHit(5, Time.fixedTime, 0.0f);
			if (Input.GetKeyDown(KeyCode.Alpha7)) DebugOnTimerHit(6, Time.fixedTime, 0.0f);
			if (Input.GetKeyDown(KeyCode.Alpha8)) DebugOnTimerHit(7, Time.fixedTime, 0.0f);
			if (Input.GetKeyDown(KeyCode.Alpha9)) DebugOnTimerHit(8, Time.fixedTime, 0.0f);

			if (Input.GetKeyDown(KeyCode.Alpha0)) InvalidateLap();
			}

		if (Input.GetKey(resetKey))
			{
			m_trackStartTime = startCounting? Time.fixedTime : 0.0f;
			m_currentSector = 0;
			m_sectorStartTime = 0.0f;
			m_invalidSector = false;
			m_invalidLap = false;
			}

		if (externalDisplay != null)
			{
			externalDisplay.invalidLap = m_invalidLap;

			if (m_trackStartTime > 0.0f)
				{
				// For visual purposes use Time.time instead of Time.fixedTime

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

		// If the replay compromises the replay continuity, invalidate the lap.

		#if !VPP_LIMITED
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


	public void OnTimerHit (VehicleBase vehicle, int sector, float hitTime, float hitDistance)
		{
		int sectorLen = m_sectors.Length;

		if (!isActiveAndEnabled) return;
		if (sector >= sectorLen) return;

		if (debugLog)
			Debug.Log("Sector hit: " + sector);

		if (sector == 0)
			{
			float lapTime = hitTime - m_trackStartTime;

			// Start line hit
			// -------------------------------------------------------------------------------------

			if (m_currentSector == sectorLen-1)
				{
				// Lap completed (previous hit was last sector)

				if (lapTime > minLapTime)
					{
					m_sectors[sectorLen-1] = hitTime - m_sectorStartTime;
					m_validSectors[sectorLen-1] = !m_invalidSector;

					onSector?.Invoke(sectorLen, m_sectors[sectorLen - 1]);
					onLap?.Invoke(lapTime, !m_invalidLap, m_sectors, m_validSectors);

					if (debugLog)
						{
						string dbg = m_invalidLap? $"[{FormatLapTime(lapTime)}] " : $" {FormatLapTime(lapTime)}  ";
						for (int i = 0; i < m_sectors.Length; i++) dbg += $"S{i+1}:{FormatSectorTime(m_sectors[i], m_validSectors[i])} ";
						Debug.Log(dbg);
						}

					if (!m_invalidLap)
						{
						// Okey - we have a lap time
						// Report last sector and new lap

						SectorPass(sectorLen, hitTime - m_trackStartTime);
						NewLap(lapTime);
						}
					else
						{
						// Starting or invalid lap

						ClearSectors();
						}
					}

				vehicle.telemetry.SetMarkerFlag(m_invalidLap);
				}
			else
				{
				// Hitting sector 0 multiple times

				if (m_currentSector != 0)
					{
					// Bad - missed some sector. Restart sector times.

					ClearSectors();
					}

				vehicle.telemetry.SetMarkerFlag(true);
				}

			// Feed telemetry and restart vehicle counters

			vehicle.telemetry.SetMarker(Telemetry.Marker.StartLinePass);
			vehicle.telemetry.SetMarkerTime(lapTime);
			vehicle.telemetry.ResetTime(Time.fixedTime - hitTime);
			vehicle.telemetry.ResetDistance(-hitDistance);
			vehicle.telemetry.segmentNumber = m_laps.Count + 1;

			if (debugLog)
				Debug.Log($"[{gameObject.name}] Telemetry feed: StartLinePass MarkerTime: {lapTime:0.000} ResetTime: {Time.fixedTime - hitTime:0.000} ResetDistance: {-hitDistance:0.000}");

			// Restart timming

			m_currentSector = 0;
			m_trackStartTime = hitTime;
			m_sectorStartTime = hitTime;
			m_invalidSector = false;
			m_invalidLap = false;

			onBeginLap?.Invoke();

			// Also clear continuity flag

			#if !VPP_LIMITED
			if (replayComponent != null)
				replayComponent.continuityFlag = true;
			#endif
			}
		else
			{
			// Sector line hit
			// -------------------------------------------------------------------------------------

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
				m_validSectors[sector-1] = !m_invalidSector;
				onSector?.Invoke(sector, m_sectors[sector-1]);

				vehicle.telemetry.SetMarkerFlag(m_invalidSector);

				m_sectorStartTime = hitTime;
				m_invalidSector = false;
				}
			else
				{
				// Multiple hits at the same sector are allowed, but only the first hit counts.

				if (sector != m_currentSector)
					{
					// Bad - some sector missing

					m_invalidLap = true;

					// Clear missed sector(s)

					for (int i = m_currentSector; i < sector; i ++)
						{
						m_sectors[i] = 0.0f;
						m_validSectors[i] = false;
						}

					// Restart sector timing

					m_currentSector = sector;
					m_sectorStartTime = hitTime;
					m_invalidSector = false;
					}

				vehicle.telemetry.SetMarkerFlag(true);
				}

			// Feed telemetry

			vehicle.telemetry.SetMarker(Telemetry.Marker.SectorLinePass);
			vehicle.telemetry.SetMarkerTime(hitTime - m_sectorStartTime);
			}
		}


	public void InvalidateLap ()
		{
		// Invalidate both lap and current sector

		m_invalidLap = true;
		m_invalidSector = true;
		}


	void ClearSectors ()
		{
		for (int i = 0, c = m_sectors.Length; i < c; i++)
			{
			m_sectors[i] = 0.0f;
			m_validSectors[i] = false;
			}
		}


	void OnGUI ()
		{
		if (!showGUI) return;

		string smallText = "";
		string currentTimeText = "";

		if (m_trackStartTime > 0.0f)
			{
			// For visual purposes use Time.time instead of Time.fixedTime

			float t = Time.time - m_trackStartTime;

			currentTimeText += m_invalidLap? "**" : "S"+(m_currentSector+1);
			currentTimeText += " " + FormatLapTime(t);
			}

		int sectorLen = m_sectors.Length;

		if (sectorLen > 1)
			{
			for (int i=0, c=m_sectors.Length; i<c; i++)
				{
				if (m_sectors[i] > 0.0f) smallText += $"S{i+1}: {FormatSectorTime(m_sectors[i], m_validSectors[i])}";

				smallText += "\n";
				}
			}

		smallText += "\n";
		smallText += "\n\nBest " + (m_bestTime > 0.0f ? FormatLapTime(m_bestTime) : "-") + "\n\n";

		float heightDelta = (sectorLen - 3) * m_style.lineHeight;
		float boxWidth = 180;
		float boxHeight = 180 + heightDelta;

		float xPos = positionX < 0? Screen.width + positionX - boxWidth : positionX;
		float yPos = positionY < 0? Screen.height + positionY - boxHeight : positionY;

		Rect pos = new Rect(xPos, yPos, boxWidth, boxHeight);

		GUI.Box(pos, "");
		GUI.Label(new Rect (pos.x+16, pos.y+8, pos.width, 40), currentTimeText, m_bigStyle);
		GUI.Label(new Rect (pos.x+16, pos.y+40, pos.height, pos.height), smallText, m_style);

		string lastLap = m_lastTime > 0.0f? FormatLapTime(m_lastTime) : "-";
		GUI.Label(new Rect (pos.x+16, pos.y+116+heightDelta, pos.width, 40), lastLap, m_bigStyle);
		}


	string FormatLapTime (float t)
		{
		int seconds = Mathf.FloorToInt(t);

		int m = seconds / 60;
		int s = seconds % 60;
		int mm = Mathf.RoundToInt(t * 1000.0f) % 1000;

		return string.Format("{0,3}:{1,2:00}:{2,3:000}", m, s, mm);
		}


	string FormatSectorTime (float t, bool valid)
		{
		if (t > 0.0f)
			{
			if (t > 90.0f)
				return FormatLapTime(t);

			int s =  Mathf.FloorToInt(t);
			int mm = Mathf.RoundToInt(t * 1000.0f) % 1000;

			return string.Format(valid? "    {0,2:00}:{1,3:000}" : "  **{0,2:00}:{1,3:000}", s, mm);
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


	void DebugOnTimerHit (int sector, float hitTime, float hitDistance)
		{
		// Find some vehicle in the scene to use as debug.

		VehicleBase vehicle = FindObjectOfType<VehicleBase>();
		if (vehicle != null)
			OnTimerHit(vehicle, sector, hitTime, hitDistance);
		}
	}

}
