

using UnityEngine;
using VehiclePhysics;
using EdyCommonTools;


namespace Perrinn424
{

[RequireComponent(typeof(MasterAudioSourceSettings))]
public class CockpitCameraAudioFilter : VehicleBehaviour
	{
	VPCameraController m_cameraController;
	MasterAudioSourceSettings m_audioSettings;


	public override void OnEnableVehicle ()
		{
		m_audioSettings = GetComponent<MasterAudioSourceSettings>();

		// Find the camera controller
		m_cameraController = (VPCameraController)FindObjectOfType(typeof(VPCameraController));
		}


	public override void UpdateVehicle ()
		{
		bool audioFilter = m_cameraController != null && m_cameraController.isInteriorCamera && m_cameraController.targetVehicle == vehicle;

		if (m_audioSettings.masterSettings.lowPassFilter != audioFilter)
			{
			m_audioSettings.masterSettings.lowPassFilter = audioFilter;
			m_audioSettings.Apply();
			}
		}


	}

}