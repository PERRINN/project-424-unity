
using UnityEngine;


namespace Perrinn424
{

public class ViosoEyePointUICorrection : MonoBehaviour
	{
	public float displacementFactor = 0.295f;

	[Header("VIOSO Eye Point (read only)")]
	public Vector3 eyePointPos;
	public Vector3 eyePointRot;

	[Header("Debug")]
	public bool applyRotOffset = false;
	public Vector3 rotOffset;

	void LateUpdate ()
		{
		eyePointPos = VIOSOCamera.eyePointPos;
		eyePointRot = VIOSOCamera.eyePointRot;

		Vector3 localPos = -displacementFactor * VIOSOCamera.eyePointPos;
		Vector3 localRot = -Mathf.Rad2Deg * VIOSOCamera.eyePointRot;
		localRot.x = -localRot.x;
		localRot.z = -localRot.z;
		if (applyRotOffset) localRot += rotOffset;

		transform.localPosition = localPos;
		transform.localRotation = Quaternion.Euler(localRot);
		}



	}
}
