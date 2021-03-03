using VehiclePhysics;
using UnityEngine;

public class FlapRotation : MonoBehaviour
{
    VehicleBase target;
    Perrinn424Aerodynamics m_aero;

    void OnEnable()
    {
        target = GetComponentInParent<VehicleBase>();
        m_aero = target.GetComponentInChildren<Perrinn424Aerodynamics>();
        transform.Rotate(new Vector3(m_aero.frontFlapStaticAngle, 0, 0));
    }

    void Update()
    {
        float flapStatic   = m_aero.frontFlapStaticAngle;
        float flapPosition = m_aero.flapAngle;  // already clipped at aerodynamics
        float flapNorm  = (flapPosition - flapStatic) / ((flapStatic + m_aero.frontFlapFlexDeltaAngle) - flapStatic);
        float flapAngle = Mathf.Lerp(flapStatic, flapStatic + m_aero.frontFlapFlexDeltaAngle, flapNorm);
        transform.localRotation = Quaternion.Euler(flapAngle, 0.0f, 0.0f);
    }
}
