using Cinemachine;
using UnityEngine;

public class ClosestCamera : MonoBehaviour
{
    public Transform target;

    private CinemachineVirtualCamera[] cameras;
    private void OnEnable()
    {
        cameras = this.GetComponentsInChildren<CinemachineVirtualCamera>();

        foreach (var camera in cameras)
        {
            camera.LookAt = target;
        }
    }

    private void Update()
    {
        //float minDistance = Mathf.Infinity;
        //CinemachineVirtualCamera nextCamera = null;

        foreach (var camera in cameras)
        {
            float compareDistance = (target.transform.position - camera.transform.position).sqrMagnitude;
            camera.Priority = Mathf.CeilToInt(100000f/compareDistance);
            //if (compareDistance < minDistance)
            //{
            //    minDistance = compareDistance;
            //    nextCamera = camera;
            //}
        }

        //nextCamera.Priority = 10;
    }
}
