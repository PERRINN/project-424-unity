
using System.Collections;
using UnityEngine;
using VehiclePhysics;
using VersionCompatibility;


public class VehicleSpawnPoints : MonoBehaviour
	{
	public VehicleBase target;
	public bool hardReposition = true;
	public bool resetVehicle = false;
	[Space(5)]
	public bool enableAlphaKeys = true;
	public bool enableNumpadKeys = true;
	public bool keysRequireAlt = true;

	int m_lastPoint = -1;


	public void Respawn (int spawnPointIndex)
		{
		if (target != null && spawnPointIndex >= 0 && spawnPointIndex < transform.childCount)
			{
			Transform spawnPoint = transform.GetChild(spawnPointIndex);

			if (hardReposition)
				target.HardReposition(spawnPoint.position, spawnPoint.rotation, resetVehicle);
			else
				target.Reposition(spawnPoint.position, spawnPoint.rotation);

			m_lastPoint = spawnPointIndex;
			}
		}


	public void NextPoint ()
		{
		m_lastPoint++;
		if (m_lastPoint >= transform.childCount)
			m_lastPoint = 0;

		Respawn(m_lastPoint);
		}


	public void PrevPoint ()
		{
		m_lastPoint--;
		if (m_lastPoint < 0)
			m_lastPoint = transform.childCount - 1;

		Respawn(m_lastPoint);
		}


	public void ResetPoint ()
		{
		Respawn(m_lastPoint);
		}


	void Update()
		{

		bool modifier = !keysRequireAlt || UnityInput.altKeyPressed;

		if (modifier)
			{
			if (enableAlphaKeys)
				{
				if (UnityInput.GetKeyDown(UnityKey.Digit1)) Respawn(0);
				if (UnityInput.GetKeyDown(UnityKey.Digit2)) Respawn(1);
				if (UnityInput.GetKeyDown(UnityKey.Digit3)) Respawn(2);
				if (UnityInput.GetKeyDown(UnityKey.Digit4)) Respawn(3);
				if (UnityInput.GetKeyDown(UnityKey.Digit5)) Respawn(4);
				if (UnityInput.GetKeyDown(UnityKey.Digit6)) Respawn(5);
				if (UnityInput.GetKeyDown(UnityKey.Digit7)) Respawn(6);
				if (UnityInput.GetKeyDown(UnityKey.Digit8)) Respawn(7);
				if (UnityInput.GetKeyDown(UnityKey.Digit9)) Respawn(8);
				if (UnityInput.GetKeyDown(UnityKey.Digit0)) Respawn(9);
				}

			if (enableNumpadKeys)
				{
				if (UnityInput.GetKeyDown(UnityKey.NumpadPlus)) NextPoint();
				if (UnityInput.GetKeyDown(UnityKey.NumpadMinus)) PrevPoint();
				if (UnityInput.GetKeyDown(UnityKey.NumpadMultiply)) ResetPoint();
				}
			}
		}
	}
