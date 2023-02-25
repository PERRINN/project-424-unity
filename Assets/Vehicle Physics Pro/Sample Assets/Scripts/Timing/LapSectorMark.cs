//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

// The collider must be a BoxCollider with the Z direction facing the direction of movement in the track


using UnityEngine;
using EdyCommonTools;


namespace VehiclePhysics.Timing
{

[RequireComponent(typeof(BoxCollider))]
public class LapSectorMark : MonoBehaviour
	{
	[Range(0,9)]
	public int sector = 0;
	[Space(5)]
	public bool debugLog = false;


	const float minDoubleHitTime = 10.0f;


	LapTimer m_lapTimer;
	Plane m_detectionPlane;
	Collider m_thisCollider;


	void OnEnable ()
		{
		m_lapTimer = GetComponentInParent<LapTimer>();
		if (m_lapTimer == null)
			{
			Debug.LogError("LapSectorMark must be child of a LapTimer object. Component disabled");
			enabled = false;
			return;
			}

		BoxCollider collider = GetComponent<BoxCollider>();
		Vector3 planePoint = transform.TransformPoint(collider.center + Vector3.forward * collider.size.z * -0.5f);
		Vector3 planeNormal = transform.TransformDirection(Vector3.forward);

		m_detectionPlane = new Plane(planeNormal, planePoint);
		m_thisCollider = collider;
		}


	void OnTriggerEnter (Collider other)
		{
		if (!isActiveAndEnabled) return;

		VehicleBase vehicle = other.GetComponentInParent<VehicleBase>();
		if (vehicle == null) return;

		// We have a vehicle crossing the mark. Transponder position required.
		// Transponder is also used to prevent multiple-hits caused by several colliders of the car hitting the trigger.

		Transponder transponder = other.GetComponentInParent<Transponder>();
		if (transponder == null || !transponder.isActiveAndEnabled)
			{
			Debug.Log($"{this.name}: Transponder not found in \"{vehicle.gameObject.name}\". Must be in the root of the vehicle.");
			return;
			}

		// Verify that this transponder hasn't hit this trigger recenty

		if (transponder.lastColliderHit == m_thisCollider && Time.fixedTime - transponder.lastHitTime < minDoubleHitTime)
			return;

		// We have a valid collision

		transponder.lastColliderHit = m_thisCollider;
		transponder.lastHitTime = Time.fixedTime;

		// Compute the precise time the transpoder hits the trigger

		Vector3 detectionPoint;
		Vector3 velocity;
		transponder.GetPointAndVelocity(out detectionPoint, out velocity);

		Ray ray = new Ray(detectionPoint, velocity.normalized);
		float hitDistance;
		m_detectionPlane.Raycast(ray, out hitDistance);

		// hitDistance is negative if the ray doesn't point to the plane,
		// so all situations are properly matched.

		Vector3 hitPoint = detectionPoint + velocity.normalized * hitDistance;
		DebugUtility.DrawCrossMark(detectionPoint, transponder.transform, GColor.deepOrange, duration: 0.01f);
		Debug.DrawLine(detectionPoint, hitPoint, GColor.pink, 0.01f);

		// hitDistance is the distance to the detection plane. If negative, the detection point has already passed thru the plane.
		// contactTime is the time the detection point will touch the detection plane at current speed.
		//
		// Assume a minimum speed, otherwise the contact time might be measured in seconds.
		// This also prevents NaN on zero velocity, i.e. when the vehicle spawns in the trigger.

		float speed = Mathf.Max(velocity.magnitude, 1.0f);
		float contactTime = hitDistance / speed;
		if (debugLog)
			{
			Debug.Log($"[{gameObject.name}] Sector {sector} pass! Hit Time: {Time.fixedTime:0.000} + {contactTime:0.000} = {Time.fixedTime + contactTime:0.000} Hit Distance: {hitDistance:0.000}");
			}

		m_lapTimer.OnTimerHit(vehicle, sector, Time.fixedTime + contactTime, hitDistance);
		}


	public void OnDrawGizmos ()
		{
		BoxCollider collider = GetComponent<BoxCollider>();

		Vector3 planePoint = transform.TransformPoint(collider.center + Vector3.forward * collider.size.z * -0.5f);
		Vector3 planeSize = Vector3.Scale(collider.size, transform.lossyScale);

		Gizmos.color = GColor.Alpha(GColor.accentGreen, 0.5f);
		Gizmos.matrix = Matrix4x4.TRS(planePoint, transform.rotation, Vector3.one);
		Gizmos.DrawCube(Vector3.zero, new Vector3(planeSize.x, planeSize.y, 0));
		Gizmos.color = GColor.Alpha(GColor.lime, 0.5f);
		Gizmos.DrawWireCube(Vector3.zero, new Vector3(planeSize.x, planeSize.y, 0));
		}
	}

}