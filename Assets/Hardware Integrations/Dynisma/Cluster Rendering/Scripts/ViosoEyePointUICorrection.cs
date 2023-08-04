
using UnityEngine;


namespace Perrinn424
{

public class ViosoEyePointUICorrection : MonoBehaviour
	{
	public float displacementFactor = 0.2953f;

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
