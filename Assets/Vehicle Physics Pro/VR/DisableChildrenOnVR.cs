
using UnityEngine;
using UnityEngine.XR;


namespace VehiclePhysics
{

public class DisableChildrenOnVR : MonoBehaviour
	{
	bool m_vrEnabled = false;


	void OnEnable ()
		{
		SetChildrenEnabled(true);
		}


	void OnDisable ()
		{
		m_vrEnabled = false;
		SetChildrenEnabled(true);
		}


	void Update ()
		{
		if (!m_vrEnabled && XRSettings.isDeviceActive)
			{
			m_vrEnabled = true;
			SetChildrenEnabled(false);
			}
		else
		if (m_vrEnabled && !XRSettings.isDeviceActive)
			{
			m_vrEnabled = false;
			SetChildrenEnabled(true);
			}
		}


	void SetChildrenEnabled (bool enabled)
		{
		foreach (Transform child in transform)
			child.gameObject.SetActive(enabled);
		}
	}

}
