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


	LapTimer m_lapTimer;
	Plane m_detectionPlane;


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
		}


	void OnTriggerEnter (Collider other)
		{
		if (!isActiveAndEnabled) return;

		VehicleBase vehicle = other.GetComponentInParent<VehicleBase>();
		if (vehicle == null) return;

		// We have a vehicle crossing the mark

		Transponder transponder = other.GetComponentInParent<Transponder>();
		if (transponder != null && transponder.isActiveAndEnabled)
			{
			Vector3 detectionPoint;
			Vector3 velocity;
			transponder.GetPointAndVelocity(out detectionPoint, out velocity);

			Ray ray = new Ray(detectionPoint, velocity.normalized);
			float hitDistance;
			m_detectionPlane.Raycast(ray, out hitDistance);

			// hitDistance is negative if the ray doesn't point to the plane,
			// so all situations are properly matched.

			// Vector3 hitPoint = detectionPoint + velocity.normalized * hitDistance;
			// Debug.DrawLine(detectionPoint, hitPoint);

			// Allow up to two fixed time steps for the collision to be detected

			float maxDeltaTime = Time.fixedDeltaTime * 2.0f;
			float contactTime = Mathf.Clamp(hitDistance / velocity.magnitude, -maxDeltaTime, maxDeltaTime);
			m_lapTimer.OnTimerHit(vehicle, sector, Time.fixedTime + contactTime, hitDistance);

			// Debug.Log("Hit! Dist: " + hitDistance.ToString("0.000") + " Time: " + contactTime.ToString("0.000"));
			}
		else
			{
			m_lapTimer.OnTimerHit(vehicle, sector, Time.fixedTime, 0.0f);
			}
		}


	public void OnDrawGizmosSelected ()
		{
		BoxCollider collider = GetComponent<BoxCollider>();

		Vector3 planePoint = transform.TransformPoint(collider.center + Vector3.forward * collider.size.z * -0.5f);
		Vector3 planeSize = Vector3.Scale(collider.size, transform.lossyScale);

		Gizmos.color = GColor.Alpha(GColor.accentGreen, 0.5f);
		Gizmos.matrix = Matrix4x4.TRS(planePoint, transform.rotation, Vector3.one);

		Gizmos.DrawCube(Vector3.zero, new Vector3(planeSize.x, planeSize.y, 0));
		}
	}

}