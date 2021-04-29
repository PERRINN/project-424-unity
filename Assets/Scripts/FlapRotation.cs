using VehiclePhysics;
using UnityEngine;

public class FlapRotation : VehicleBehaviour
{
    Perrinn424Aerodynamics m_aero;

    public override void OnEnableVehicle()
    {
        m_aero = vehicle.GetComponentInChildren<Perrinn424Aerodynamics>();
        if (m_aero == null)
        {
            enabled = false;
            return;
        }
        transform.Rotate(new Vector3(m_aero.frontFlapStaticAngle, 0, 0));
    }

    public override void UpdateVehicle()
    {
        float flapStatic   = m_aero.frontFlapStaticAngle;
        float flapPosition = m_aero.flapAngle;  // already clipped at aerodynamics
        float flapNorm  = (flapPosition - flapStatic) / ((flapStatic + m_aero.frontFlapFlexDeltaAngle) - flapStatic);
        float flapAngle = Mathf.Lerp(flapStatic, flapStatic + m_aero.frontFlapFlexDeltaAngle, flapNorm);
        transform.localRotation = Quaternion.Euler(flapAngle, 0.0f, 0.0f);
    }
}
