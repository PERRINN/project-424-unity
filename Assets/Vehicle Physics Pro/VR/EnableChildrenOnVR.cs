
using UnityEngine;
using UnityEngine.XR;


namespace VehiclePhysics
{

public class EnableChildrenOnVR : MonoBehaviour
	{
	bool m_vrEnabled = false;


	void OnEnable ()
		{
		SetChildrenEnabled(false);
		}


	void OnDisable ()
		{
		m_vrEnabled = false;
		SetChildrenEnabled(false);
		}


	void Update ()
		{
		if (!m_vrEnabled && XRSettings.isDeviceActive)
			{
			m_vrEnabled = true;
			SetChildrenEnabled(true);
			}
		else
		if (m_vrEnabled && !XRSettings.isDeviceActive)
			{
			m_vrEnabled = false;
			SetChildrenEnabled(false);
			}
		}


	void SetChildrenEnabled (bool enabled)
		{
		foreach (Transform child in transform)
			child.gameObject.SetActive(enabled);
		}
	}

}
