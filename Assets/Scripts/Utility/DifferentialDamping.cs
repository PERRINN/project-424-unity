
using UnityEngine;
using VehiclePhysics;


public class DifferentialDamping : VehicleBehaviour
    {
    [Range(0,1)]
    public float frontDamping = 1.0f;

    [Range(0,1)]
    public float rearDamping = 1.0f;


    Perrinn424CarController m_vehicle;


    public override void OnEnableVehicle ()
        {
        m_vehicle = vehicle as Perrinn424CarController;
        if (m_vehicle == null)
            {
            Debug.LogWarning("DifferentialDamping works with the 424 vehicle controller only.");
            enabled = false;
            }
        }


    // Using update because in the 424 physics run at 500 Hz while the display may work at 144 Hz max,
    // and this won't be updated offten.

    public override void UpdateVehicle ()
        {
        m_vehicle.SetDifferentialDamping(frontDamping, rearDamping);
        }
    }

