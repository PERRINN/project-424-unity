
using UnityEngine;
using VehiclePhysics;


namespace Perrinn424
{

public class LiftAndCoastInputKey : VehicleBehaviour
	{
	public KeyCode key = KeyCode.L;

	Perrinn424CarController m_vehicle;

	public override void OnEnableVehicle ()
		{
		m_vehicle = vehicle as Perrinn424CarController;
		if (m_vehicle == null)
			enabled = false;
		}

	public override void UpdateVehicle ()
		{
		if (Input.GetKeyDown(key))
			m_vehicle.mguLiftAndCoast = !m_vehicle.mguLiftAndCoast;
		}
	}

}
