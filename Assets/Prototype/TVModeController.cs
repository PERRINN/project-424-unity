using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CinemachineVirtualCameraBase))]
public class TVModeController : MonoBehaviour
{
    [SerializeField]
    private CinemachineVirtualCameraBase cinemachineVirtualCameraBase;


    public Vector3 microphoneLocalPosition;
    private GameObject microphone;

    private void OnEnable()
    {
        CreateMicrophone();
    }

    private void OnDisable()
    {
        RemoveMicrophone();
    }

    private void CreateMicrophone()
    {
        var player = cinemachineVirtualCameraBase.LookAt;
        microphone = new GameObject("Microphone", new[] { typeof(AudioListener) });
        microphone.transform.parent = player;
        microphone.transform.localPosition = microphoneLocalPosition;
    }

    private void RemoveMicrophone()
    {
        Destroy(microphone);
    }

    private void Reset()
    {
        cinemachineVirtualCameraBase = GetComponent<CinemachineVirtualCameraBase>();
    }
}
