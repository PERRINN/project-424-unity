using UnityEngine;
using VehiclePhysics;

public class DRSActivation : VehicleBehaviour
{
    public float closedAngle = 0.0f;
    public float openAngle = -90.0f;


    float drsPosition;


    public override void UpdateVehicle ()
    {
        drsPosition = vehicle.data.Get(Channel.Custom, Perrinn424Data.DrsPosition) / 1000.0f;
        drsPosition = Mathf.Clamp01(drsPosition);

        float drsAngle = Mathf.Lerp(closedAngle, openAngle, drsPosition);
        transform.localRotation = Quaternion.Euler(drsAngle, 0.0f, 0.0f);
    }
}
