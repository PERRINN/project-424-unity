
using System.Collections;
using UnityEngine;
using VehiclePhysics;


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

		bool modifier = !keysRequireAlt || Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

		if (modifier)
			{
			if (enableAlphaKeys)
				{
				if (Input.GetKeyDown(KeyCode.Alpha1)) Respawn(0);
				if (Input.GetKeyDown(KeyCode.Alpha2)) Respawn(1);
				if (Input.GetKeyDown(KeyCode.Alpha3)) Respawn(2);
				if (Input.GetKeyDown(KeyCode.Alpha4)) Respawn(3);
				if (Input.GetKeyDown(KeyCode.Alpha5)) Respawn(4);
				if (Input.GetKeyDown(KeyCode.Alpha6)) Respawn(5);
				if (Input.GetKeyDown(KeyCode.Alpha7)) Respawn(6);
				if (Input.GetKeyDown(KeyCode.Alpha8)) Respawn(7);
				if (Input.GetKeyDown(KeyCode.Alpha9)) Respawn(8);
				if (Input.GetKeyDown(KeyCode.Alpha0)) Respawn(9);
				}

			if (enableNumpadKeys)
				{
				if (Input.GetKeyDown(KeyCode.KeypadPlus)) NextPoint();
				if (Input.GetKeyDown(KeyCode.KeypadMinus)) PrevPoint();
				if (Input.GetKeyDown(KeyCode.KeypadMultiply)) ResetPoint();
				}
			}
		}
	}
