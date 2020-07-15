//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;
using EdyCommonTools;


namespace VehiclePhysics.Utility
{

[AddComponentMenu("Vehicle Physics/Utility/Auto-brake Zone", 40)]
public class AutoBrakeZone : MonoBehaviour
	{
	public VehicleBase[] vehicles;
	public bool autoFindVehicles = false;

	public float targetSpeed = 10.0f;
	public Vector3 range = new Vector3(20.0f, 10.0f, 300.0f);

    public float maxAccelerationG = 1.7f;
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
		float distance = relativePos.z;

		if (distance > 0.0f && distance < range.z
			&& MathUtility.FastAbs(relativePos.x) < range.x * 0.5f
			&& MathUtility.FastAbs(relativePos.y) < range.y * 0.5f)
			{
			float speed = -m_transform.InverseTransformDirection(vehicle.cachedRigidbody.velocity).z;
			distance -= speed * speed * Time.deltaTime;
			if (distance < 0.0f) distance = 0.0f;

			float targetBrakeSpeed = targetSpeed + Mathf.Sqrt(2.0f * maxAccelerationG * 9.8f * distance);
			float deltaSpeed = targetBrakeSpeed - speed;

			// Debug.Log("Speed: " + speed.ToString("0.00") + " Target: " + targetBrakeSpeed.ToString("0.00") + " Delta: " + deltaSpeed.ToString("0.00"));

			float targetThrottleInput = Mathf.Clamp01(deltaSpeed * inputRamp);
			float targetBrakeInput = Mathf.Clamp01(-deltaSpeed * inputRamp);

			float vehicleThrottle = vehicle.data.Get(Channel.Input, InputData.Throttle) / 10000.0f;
			float vehicleBrake = vehicle.data.Get(Channel.Input, InputData.Brake) / 10000.0f;

			vehicleThrottle = Mathf.Min(vehicleThrottle, targetThrottleInput);
			vehicleBrake = Mathf.Max(vehicleBrake, targetBrakeInput);

			vehicle.data.Set(Channel.Input, InputData.Throttle, (int)(vehicleThrottle * 10000));
			vehicle.data.Set(Channel.Input, InputData.Brake, (int)(vehicleBrake * 10000));
			}
		}


	void OnDrawGizmosSelected ()
		{
		if (!isActiveAndEnabled) return;

		Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

		Gizmos.color = GColor.Alpha(GColor.accentRed, 0.5f);
        Gizmos.DrawCube(Vector3.zero, new Vector3(range.x, range.y, 0));

		Gizmos.color = GColor.Alpha(GColor.accentGreen, 0.9f);
        Gizmos.DrawWireCube(0.5f * range.z * Vector3.forward, range);

		#if UNITY_EDITOR
		// string text = string.Format("{0,0:0.0} m/s\n{1,0:0.0} km/h", targetSpeed, targetSpeed * 3.6f);
		// UnityEditor.Handles.Label(transform.position, text);
		#endif
		}
	}

}