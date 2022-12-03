
// This component applies zoom and clip planes to the TV cameras controlled with Cinemachine,
// as I haven't been able to make the "Follow Zoom" extension of ClearShot work properly.
//
// When Cinemachine is enabled this component enables the CameraFovController component and/or
// apply specific properties of the virtual camera:
//
// - Set camera's FoV to Lens > FieldOfView
// - Set camera's clipping planes to Lens clipping planes
// - If the camera does not have a dash "-" in the name, then enable the CameraFovController component.
// - Restore the camera's original FoV when switching cameras
//


using UnityEngine;
using VehiclePhysics;
using EdyCommonTools;
using Cinemachine;


namespace Perrinn424
{

public class TVCameraZoomController : MonoBehaviour
	{
    public Transform tvCameraSystem;
	public Camera mainCamera;
	[Space(5)]
	public bool debugLog = false;


	CinemachineBrain m_cinemachine;
	VPCameraController m_vppCamera;
	CameraFovController m_fovController;


	bool m_active;
	float m_originalNearPlane;
	float m_originalFarPlane;

	ICinemachineCamera m_currentCamera;


	void OnEnable ()
		{
		// All objects required, otherwise -> exceptions.

		m_cinemachine = tvCameraSystem.GetComponent<CinemachineBrain>();
		m_vppCamera = tvCameraSystem.GetComponent<VPCameraController>();
		m_fovController = mainCamera.GetComponent<CameraFovController>();

		m_active = false;
		m_currentCamera = null;
		}


	void OnDisable ()
		{
		RestoreCameraSettings();
		}


	void Update ()
		{
		if (!m_active && m_cinemachine.enabled)
			{
			// Cinemachine is now enabled.
			//	- Store current FoV and clipping planes.
			//	- Get ready for applying the settings of the current camera

			BackupCameraSettings();
			m_active = true;
			m_currentCamera = null;
			}
		else
		if (m_active && !m_cinemachine.enabled)
			{
			// Cinemachine is now disabled. Disable FoV controller and restore camera values;

			m_fovController.enabled = false;
			RestoreCameraSettings();
			m_active = false;
			}

		if (m_active)
			{
			// Get the currently active virtual camera

			ICinemachineCamera activeCamera = m_cinemachine.ActiveVirtualCamera;
			if (activeCamera is CinemachineClearShot)
				activeCamera = (activeCamera as CinemachineClearShot).LiveChild;

			// Has changed?

			if (activeCamera != m_currentCamera && activeCamera != null)
				{
				if (debugLog)
					Debug.Log($"TVCameraZoomController: Using camera [{activeCamera.Name}] (RefLookAtPos: {activeCamera.State.ReferenceLookAt} LookAt: {activeCamera.LookAt})");

				m_currentCamera = activeCamera;

				// Apply lens settings to our camera

				LensSettings lens = m_currentCamera.State.Lens;
				mainCamera.nearClipPlane = lens.NearClipPlane;
				mainCamera.farClipPlane = lens.FarClipPlane;
				mainCamera.fieldOfView = lens.FieldOfView;

				// Check for the camera type in the camera's name.
				// Unfortunately, this seems the only sane way to know if this camera has fixed zoom or not.
				//
				// If the name contains a dash "-" it's fixed zoom.

				m_fovController.enabled = m_currentCamera.Name.IndexOf('-') < 0;
				m_fovController.target = m_currentCamera.LookAt;
				}
			}
		}


	void BackupCameraSettings ()
		{
		m_originalNearPlane = mainCamera.nearClipPlane;
		m_originalFarPlane = mainCamera.farClipPlane;
		}


	void RestoreCameraSettings ()
		{
		mainCamera.nearClipPlane = m_originalNearPlane;
		mainCamera.farClipPlane = m_originalFarPlane;
		}
	}

}
