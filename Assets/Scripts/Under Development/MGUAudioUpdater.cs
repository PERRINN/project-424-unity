using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VehiclePhysics;


public class MGUAudioUpdater : MonoBehaviour
{
    public VehicleBase vehicle;
    public float trqGainFrontMGU;
    public float rpmGainFrontMGU;
    public float trqGainRearMGU;
    public float rpmGainRearMGU;
    public float basePitchOfFrontMGU;
    public float baseVolumeOfFrontMGU;
    public float basePitchOfRearMGU;
    public float baseVolumeOfRearMGU;
    public AudioSource frontEngineAudio;
    public AudioSource rearEngineAudio;

    float frontRpm;
    float frontMechanical;
    float rearRpm;
    float rearMechanical;

    void Start()
    {
        frontEngineAudio.pitch = basePitchOfFrontMGU;
        frontEngineAudio.volume = baseVolumeOfFrontMGU;
        rearEngineAudio.pitch = basePitchOfRearMGU;
        rearEngineAudio.volume = baseVolumeOfRearMGU;
    }

    void Update()
    {
        int[] custom = vehicle.data.Get(Channel.Custom);
        frontRpm = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.Rpm] / 1000.0f;
        frontMechanical = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.MechanicalTorque] / 1000.0f;
        rearRpm = custom[Perrinn424Data.RearMguBase + Perrinn424Data.Rpm] / 1000.0f;
        rearMechanical = custom[Perrinn424Data.RearMguBase + Perrinn424Data.MechanicalTorque] / 1000.0f;

        //print(frontMechanical);

        UpdateFrontEngine();
        UpdateRearEngine();
    }

    public void UpdateFrontEngine()
    {
        frontEngineAudio.pitch = basePitchOfFrontMGU + Mathf.Abs(frontRpm) * rpmGainFrontMGU;
        frontEngineAudio.volume = baseVolumeOfFrontMGU + Mathf.Abs(frontMechanical) * trqGainFrontMGU;
    }

    public void UpdateRearEngine()
    {
        rearEngineAudio.pitch = basePitchOfRearMGU + Mathf.Abs(rearRpm) * rpmGainRearMGU;
        rearEngineAudio.volume = baseVolumeOfRearMGU + Mathf.Abs(rearMechanical) * trqGainRearMGU;
    }
}
