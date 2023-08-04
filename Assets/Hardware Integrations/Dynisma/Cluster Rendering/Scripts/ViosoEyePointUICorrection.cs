
// Correct the position and rotation of the static UI based on the eye point data applied to VIOSO cameras.


using UnityEngine;


namespace Perrinn424
{

public class ViosoEyePointUICorrection : MonoBehaviour
	{
	public float displacementFactor = 0.2953f;
	public bool debugInfo = false;

	void LateUpdate ()
		{
		Vector3 localPos = -displacementFactor * VIOSOCamera.eyePointPos;
		Vector3 localRot = -Mathf.Rad2Deg * VIOSOCamera.eyePointRot;
		localRot.x = -localRot.x;
		localRot.z = -localRot.z;

		transform.localRotation = Quaternion.Euler(localRot);
		transform.localPosition = transform.localRotation * localPos;
		}
	}

}
