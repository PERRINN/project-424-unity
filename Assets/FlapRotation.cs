using VehiclePhysics;
using UnityEngine;

public class FlapRotation : MonoBehaviour
{
    VehicleBase target;
    Perrinn424Aerodynamics m_aero = new Perrinn424Aerodynamics();

    void OnEnable()
    {
        target = GetComponentInParent<VehicleBase>();
        m_aero = target.GetComponentInChildren<Perrinn424Aerodynamics>();
        transform.Rotate(new Vector3(-m_aero.flapAngle, 0, 0));
    }
}
