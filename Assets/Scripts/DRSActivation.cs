using UnityEngine;
using VehiclePhysics;

public class DRSActivation : MonoBehaviour
{
    public float closedAngle = 0.0f;
    public float openAngle = -90.0f;


    VehicleBase target;

    float drsPosition;

    void OnEnable()
    {
        target = GetComponentInParent<VehicleBase>();

    }
    // Update is called once per frame
    void Update()
    {
        drsPosition = target.data.Get(Channel.Custom, Perrinn424Data.DrsPosition) / 1000.0f;
        drsPosition = Mathf.Clamp01(drsPosition);

        float drsAngle = Mathf.Lerp(closedAngle, openAngle, drsPosition);
        transform.localRotation = Quaternion.Euler(drsAngle, 0.0f, 0.0f);
    }
}
