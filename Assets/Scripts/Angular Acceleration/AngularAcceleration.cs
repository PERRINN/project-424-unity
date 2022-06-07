using Perrinn424.TelemetryLapSystem;
using UnityEngine;
using VehiclePhysics;

public class AngularAcceleration : VehicleBehaviour
{
    Rigidbody rb;
    public AngularVelocityDifferentiation angularVelocityDifferentiation;

    public override void OnEnableVehicle()
    {
        rb = vehicle.cachedRigidbody;
        angularVelocityDifferentiation = new AngularVelocityDifferentiation(rb);
    }


    public override void UpdateVehicle()
    {
    }

    public override void FixedUpdateVehicle()
    {
        angularVelocityDifferentiation.Compute(Time.deltaTime);
        Draw();
    }

    public void Draw() 
    {
        Draw(vehicle.localAngularAcceleration, DebugGraph.DefaultBlue);
        Draw(angularVelocityDifferentiation.localAngularAcceleration, DebugGraph.DefaultGreen);
    }

    public void Draw(Vector3 localAngularAcceleration, Color color)
    {
        DebugGraph.MultiLog("Yaw rad / s²", color, localAngularAcceleration.y);
        DebugGraph.MultiLog("Roll rad / s²", color, localAngularAcceleration.z);
        DebugGraph.MultiLog("Pitch rad / s²", color, localAngularAcceleration.x);
    }
}
