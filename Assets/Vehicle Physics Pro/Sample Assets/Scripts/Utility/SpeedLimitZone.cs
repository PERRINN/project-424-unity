//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;
using EdyCommonTools;


namespace VehiclePhysics.Utility
{

[AddComponentMenu("Vehicle Physics/Utility/Speed Limit Zone", 41)]
public class SpeedLimitZone : MonoBehaviour
	{
	public VehicleBase[] vehicles;
	public bool autoFindVehicles = false;
	public Vector3 range = new Vector3(20.0f, 10.0f, 300.0f);

	[Space(5)]
	public float entryTargetSpeed = 10.0f;
	public float exitTargetSpeed = 10.0f;

	[Space(5)]
	[Range(0,1)]
	public float fadeInZone = 0.1f;
	[Range(0,1)]
	public float fadeOutZone = 0.1f;

	[Space(5)]
	public float inputRamp = 1.0f;

	Transform m_transform;


	void OnEnable ()
		{
		m_transform = GetComponent<Transform>();

		if (autoFindVehicles)
			vehicles = FindObjectsOfType<VehicleBase>();
		}


	void FixedUpdate ()
		{
		foreach (VehicleBase vehicle in vehicles)
			Process(vehicle);
		}


	void Process (VehicleBase vehicle)
		{
		// Find out if the vehicle is inside the action range

		Vector3 relativePos = m_transform.InverseTransformPoint(vehicle.cachedTransform.position);
		float position = relativePos.z;

		if (position > 0.0f && position < range.z
			&& MathUtility.FastAbs(relativePos.x) < range.x * 0.5f
			&& MathUtility.FastAbs(relativePos.y) < range.y * 0.5f)
			{
			float fadeIn = fadeInZone > 0.0f? Mathf.InverseLerp(0.0f, range.z * fadeInZone, position) : 1.0f;
			float fadeOut = fadeOutZone > 0.0f? Mathf.InverseLerp(range.z, range.z * (1.0f - fadeOutZone), position) : 1.0f;
			float factor = Mathf.Min(fadeIn, fadeOut);

			float targetSpeed = Mathf.Lerp(entryTargetSpeed, exitTargetSpeed, position / range.z);
			float speedLimitThrottle = Mathf.Lerp(1.0f, Mathf.Clamp01((targetSpeed - vehicle.speed) * inputRamp), factor);

			float vehicleThrottle = vehicle.data.Get(Channel.Input, InputData.Throttle) / 10000.0f;
			vehicleThrottle = Mathf.Min(vehicleThrottle, speedLimitThrottle);
			vehicle.data.Set(Channel.Input, InputData.Throttle, (int)(vehicleThrottle * 10000));
			}
		}


	void OnDrawGizmosSelected ()
		{
		if (!isActiveAndEnabled) return;

		// Entry / exit planes

		Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

		Vector3 planeSize = new Vector3(range.x, range.y, 0);

		// Entry plane

		Gizmos.color = GColor.Alpha(GColor.lightBlue, 0.5f);
        Gizmos.DrawWireCube(new Vector3(0, 0, range.z * fadeInZone), planeSize);
        Gizmos.DrawCube(Vector3.zero, planeSize);

		// Exit plane

		Gizmos.color = GColor.Alpha(GColor.indigo, 0.5f);
        Gizmos.DrawWireCube(new Vector3(0, 0, range.z * (1.0f-fadeOutZone)), planeSize);
        Gizmos.DrawCube(new Vector3(0, 0, range.z), planeSize);

		// Volume wire

		Gizmos.color = GColor.Alpha(GColor.blue, 0.9f);
        Gizmos.DrawWireCube(0.5f * range.z * Vector3.forward, range);

		// Texts

		#if UNITY_EDITOR
		// string text1 = string.Format("Entry: {0,0:0.0} m/s\n{1,0:0.0} km/h", entryTargetSpeed, entryTargetSpeed * 3.6f);
		// string text2 = string.Format("Exit: {0,0:0.0} m/s\n{1,0:0.0} km/h", exitTargetSpeed, exitTargetSpeed * 3.6f);
		// UnityEditor.Handles.Label(transform.position, text1);
		// UnityEditor.Handles.Label(transform.position + transform.forward*range.z, text2);
		#endif
		}
	}

}