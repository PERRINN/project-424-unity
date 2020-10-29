using UnityEngine;
using VehiclePhysics;

public class DRSActivation : MonoBehaviour
{
    VehicleBase target;
    Perrinn424Aerodynamics m_aero = new Perrinn424Aerodynamics();
    
    float drsPosition;
    bool rotating = false;
    bool DRSclosing;
    float degreesPerSecond;
    

    void OnEnable()
    {
        target = GetComponentInParent<VehicleBase>();
        m_aero = target.GetComponentInChildren<Perrinn424Aerodynamics>();
        degreesPerSecond = 90 / m_aero.dRSActivationTime;
        
    }
    // Update is called once per frame
    void Update()
    {
        drsPosition = target.data.Get(Channel.Custom, Perrinn424Data.DrsPosition) / 1000.0f;
        
        DRSclosing = m_aero.DRSclosing;
        
        if (drsPosition > 0 && drsPosition < 1)
            rotating = true;
        else
            rotating = false;

        if (rotating && !DRSclosing)
            transform.Rotate(new Vector3(-degreesPerSecond * Time.deltaTime, 0, 0));

        if (rotating && DRSclosing)
            transform.Rotate(new Vector3(degreesPerSecond * Time.deltaTime, 0, 0));
    }
}
