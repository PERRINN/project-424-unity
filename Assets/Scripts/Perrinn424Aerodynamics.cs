

using UnityEngine;
using VehiclePhysics;
using System;


public class Perrinn424Aerodynamics : VehicleBehaviour
	{
	[Serializable]
	public class AeroSettings
		{
		public Transform applicationPoint;

		public float dragCoefficient = 1.0f;
		public float downforceCoefficient = 1.0f;
		}

	public AeroSettings front = new AeroSettings();
	public AeroSettings rear = new AeroSettings();
	public AeroSettings drag = new AeroSettings();


	public override void FixedUpdateVehicle ()
		{
		Rigidbody rb = vehicle.cachedRigidbody;
		Vector3 vDrag = rb.velocity * rb.velocity.magnitude;
		float vSquared = rb.velocity.sqrMagnitude;

		if (front.applicationPoint != null)
			{
			Vector3 dragForce = -front.dragCoefficient * vDrag;
			Vector3 downforce = -front.downforceCoefficient * vSquared * front.applicationPoint.up;

			rb.AddForceAtPosition(dragForce + downforce, front.applicationPoint.position);
			}

		if (rear.applicationPoint != null)
			{
			Vector3 dragForce = -rear.dragCoefficient * vDrag;
			Vector3 downforce = -rear.downforceCoefficient * vSquared * rear.applicationPoint.up;

			rb.AddForceAtPosition(dragForce + downforce, rear.applicationPoint.position);
			}

		if (drag.applicationPoint != null)
			{
			Vector3 dragForce = -drag.dragCoefficient * vDrag;
			Vector3 downforce = -drag.downforceCoefficient * vSquared * drag.applicationPoint.up;

			rb.AddForceAtPosition(dragForce + downforce, drag.applicationPoint.position);
			}
		}
	}

