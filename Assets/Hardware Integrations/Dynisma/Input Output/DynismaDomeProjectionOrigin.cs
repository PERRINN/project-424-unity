
// Apply eyepoint correction to DomeProjection's origin.
//
// Requirements:
//
//	- Origin and Main Camera under the same parent.
//	- Camera position and rotation 0,0,0, so it can be driven by the parent.
//	- Origin must be positioned at the relative origin from the camera base position.
//
// For example: if the camera's base position for the projection is 0,1,0, then the camera must be
// positioned at 0,0,0 and the origin at 0,-1,0.


using UnityEngine;


public class DynismaDomeProjectionOrigin : MonoBehaviour
	{
	public static Vector3 eyePointPos;
	public static Vector3 eyePointRot;

	// Test mode for manually specifying the eye point values

	public bool testMode = false;
	public Vector3 testEyePointPos;
	public Vector3 testEyePointRot;


	Vector3 m_offset;


	void OnEnable ()
		{
		// Retrieve current position as the projection offset from the camera to the origin

		m_offset = transform.localPosition;
		}


	void Update ()
		{
		if (testMode)
			{
			eyePointPos = testEyePointPos;
			eyePointRot = testEyePointRot;
			}

		// Apply eyepoint correction to the origin. The rotation is relative to the camera position.

		Quaternion rotation = Quaternion.Inverse(Quaternion.Euler(eyePointRot * Mathf.Rad2Deg));
		transform.localPosition = -eyePointPos + rotation * m_offset;
		transform.localRotation = rotation;
		}


	void OnDisable ()
		{
		transform.localPosition = m_offset;
		}
	}

