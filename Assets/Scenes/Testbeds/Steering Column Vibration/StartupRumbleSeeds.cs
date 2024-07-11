
using UnityEngine;
using VehiclePhysics;


public class StartupRumbleSeeds : VehicleBehaviour
	{
	public float frontLeft = 0.0f;
	public float frontRight = 0.0f;
	public float rearLeft = 0.0f;
	public float rearRight = 0.0f;

	public bool disableRumbleOnRears = false;



	public override void OnEnableVehicle ()
		{
		vehicle.wheelState[0].effectsDistance = frontLeft;
		vehicle.wheelState[1].effectsDistance = frontRight;
		vehicle.wheelState[2].effectsDistance = rearLeft;
		vehicle.wheelState[3].effectsDistance = rearRight;

		vehicle.wheelState[0].effectsPosition = Vector2.one * frontLeft;
		vehicle.wheelState[1].effectsPosition = Vector2.one * frontRight;
		vehicle.wheelState[2].effectsPosition = Vector2.one * rearLeft;
		vehicle.wheelState[3].effectsPosition = Vector2.one * rearRight;
		}


	public override void UpdateVehicleSuspension ()
		{
		if (disableRumbleOnRears)
			{
			Vector3 deltaPosition = vehicle.wheelState[2].wheelContact.pointVelocity * Time.deltaTime;
			vehicle.wheelState[2].effectsPosition = -deltaPosition;
			vehicle.wheelState[2].effectsDistance = -deltaPosition.magnitude;

			deltaPosition = vehicle.wheelState[3].wheelContact.pointVelocity * Time.deltaTime;
			vehicle.wheelState[3].effectsPosition = -deltaPosition;
			vehicle.wheelState[3].effectsDistance = -deltaPosition.magnitude;
			}
		}

	}

