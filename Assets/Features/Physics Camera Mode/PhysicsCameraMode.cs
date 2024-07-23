
using UnityEngine;
using VehiclePhysics;


namespace Perrinn424.CameraSystem
{

public class PhysicsCameraMode : MonoBehaviour
	{
	public GameObject[] disableGameObjects = new GameObject[0];
	public VPKinematicWheelCollider[] wheelsToShowGizmos = new VPKinematicWheelCollider[0];


	void OnEnable ()
		{
		SetGameObjectsActive(disableGameObjects, false);
		SetGizmosEnabled(wheelsToShowGizmos, true);
		}


	void OnDisable ()
		{
		SetGameObjectsActive(disableGameObjects, true);
		SetGizmosEnabled(wheelsToShowGizmos, false);
		}


	static void SetGameObjectsActive (GameObject[] disableGameObjects, bool active)
		{
		for (int i = 0, c = disableGameObjects.Length; i < c; i++)
			disableGameObjects[i].SetActive(active);
		}


	static void SetGizmosEnabled (VPKinematicWheelCollider[] wheels, bool show)
		{
		for (int i = 0, c = wheels.Length; i < c; i++)
			wheels[i].alwaysShowGizmos = show;
		}
	}

}