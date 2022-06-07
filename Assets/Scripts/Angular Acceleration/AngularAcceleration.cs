using Perrinn424.TelemetryLapSystem;
using UnityEngine;
using VehiclePhysics;

public class AngularAcceleration : VehicleBehaviour
{
    Rigidbody rb;
    public AngularVelocityDifferentiation angularVelocityDifferentiation;
    public QuaternionDifferentiation quaternionDifferentiation;

    public override void OnEnableVehicle()
    {
        rb = vehicle.cachedRigidbody;
        float[] coefficients = new float[0];
        float divisor = 1f;

        //backward coefficients
        coefficients = new float[] { 1f, -1f };
        //coefficients = new float[] { 1.5f, -2f, 0.5f };
        //coefficients = new float[] { 11/6f, -18/6f, 9/6f, -2/6f };
        //coefficients = new float[] { 25 / 12f, -48 / 12f, 36 / 12f, -16 / 12f, 3/12f };
        //coefficients = new float[] { 147 / 60f, -360 / 60f, 450 / 60f, -400 / 60f, 225/60f, -72/60f, 10/60f };

        //center coefficients
        //coefficients = new float[] { -1/12f, 8f/12f, 0f, -8/12f, 1f/12f }; //divisor = 1
        //coefficients = new float[] { 2f, -25, 150, -600, 2100, 0, -2100, 600, -150, 25, -2 }; //divisor = 2520
        //divisor = 2520f;

        for (int i = 0; i < coefficients.Length; i++)
        {
            coefficients[i] /= divisor;
        }

        angularVelocityDifferentiation.Init(rb, coefficients);

        quaternionDifferentiation.Init(rb);
    }


    public override void UpdateVehicle()
    {
        Draw();
    }

    public override void FixedUpdateVehicle()
    {
        angularVelocityDifferentiation.Compute(Time.deltaTime);
        quaternionDifferentiation.Compute(Time.deltaTime);
        //Draw();
    }

    public void Draw() 
    {
        Draw(vehicle.localAngularAcceleration, DebugGraph.DefaultBlue);
        Draw(angularVelocityDifferentiation.localAngularAcceleration, DebugGraph.DefaultGreen);
        Draw(quaternionDifferentiation.localAngularAcceleration, DebugGraph.DefaultRed);
    }

    public void Draw(Vector3 localAngularAcceleration, Color color)
    {
        DebugGraph.MultiLog("Yaw rad / s²", color, localAngularAcceleration.y);
        DebugGraph.MultiLog("Roll rad / s²", color, localAngularAcceleration.z);
        DebugGraph.MultiLog("Pitch rad / s²", color, localAngularAcceleration.x);
    }
}
