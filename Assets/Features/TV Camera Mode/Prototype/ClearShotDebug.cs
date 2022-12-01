using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ClearShotDebug : MonoBehaviour
{
    public Vector3 speed;

    public CinemachineVirtualCamera [] cameras;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.Translate(speed * Time.deltaTime);
    }

    private void Reset()
    {
        cameras = FindObjectsOfType<CinemachineVirtualCamera>();
    }

    private void OnDrawGizmos()
    {
        CinemachineVirtualCamera closestCamera = null;
        float minDistance = Mathf.Infinity;

        foreach (var camera in cameras)
        {
            Draw(camera, Color.white, out float distance);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestCamera = camera;
            }
        }

        Draw(closestCamera, Color.green, out _);
    }

    private void Draw(CinemachineVirtualCamera camera, Color c, out float distance)
    {
        Vector3 from = this.transform.position;
        Vector3 to = camera.transform.position;
        Vector3 vector = to - from;
        distance = vector.magnitude;
        Vector3 labelPosition = from + vector * 0.5f;
        Draw(from, to, distance, labelPosition, c);
    }

    private void Draw(Vector3 from, Vector3 to, float distance, Vector3 labelPosition, Color color)
    {
#if UNITY_EDITOR
        Color cacheColor = UnityEditor.Handles.color;
        UnityEditor.Handles.color = color;
        UnityEditor.Handles.DrawDottedLine(from, to, 4f);
        GUIStyle style = new GUIStyle();
        style.normal.textColor = color;
        UnityEditor.Handles.Label(labelPosition, new GUIContent(distance.ToString("F2")), style);
        UnityEditor.Handles.color = cacheColor;

#endif

    }
}
