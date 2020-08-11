using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using VehiclePhysics;

public struct GhostTransform
{
    public Vector3 position;
    public Quaternion rotation;

    public GhostTransform(Transform transform)
    {
        this.position = transform.position;
        this.rotation = transform.rotation;
    }
}

public class GhostManager : MonoBehaviour
{
    public Transform vehicle;
    public Transform ghostVehicle;
    
    public bool recording;
    public bool playing;

    private List<GhostTransform> recordedGhostTransform = new List<GhostTransform>();
    private GhostTransform lastRecordedGhostTransform;

    private void Start()
    {
        
    }

    void Update()
    {
        if (recording)
        {
            if (vehicle.position != lastRecordedGhostTransform.position || vehicle.rotation != lastRecordedGhostTransform.rotation)
            {
                var newGhostTransform = new GhostTransform(vehicle);
                recordedGhostTransform.Add(newGhostTransform);

                lastRecordedGhostTransform = newGhostTransform;
            }

        }

        if (playing)
        {
            Play();
        }
    }

    void SaveRecording()
    {
        //PlayerPrefs.SetString("ReplayRecord", lastRecordedGhostTransform);
    }

    void Play()
    {
        ghostVehicle.gameObject.SetActive(true);
        StartCoroutine(StartGhost());

        playing = false;
    }

    IEnumerator StartGhost()
    {
        for (int i = 0; i < recordedGhostTransform.Count; i++)
        {
            ghostVehicle.position = recordedGhostTransform[i].position;
            ghostVehicle.rotation = recordedGhostTransform[i].rotation;
            yield return new WaitForFixedUpdate();
            
        }
    }
}
