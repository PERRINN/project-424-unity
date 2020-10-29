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
    public AudioSource frontMGUAudio;
    public AudioSource rearMGUAudio;

    float frontRpm;
    float frontMechanical;
    float rearRpm;
    float rearMechanical;
    bool playOnce = false;

    void Start()
    {
        frontMGUAudio.pitch = basePitchOfFrontMGU;
        frontMGUAudio.volume = baseVolumeOfFrontMGU;
        rearMGUAudio.pitch = basePitchOfRearMGU;
        rearMGUAudio.volume = baseVolumeOfRearMGU;
    }

    void FixedUpdate()
    {
        if(playOnce == false)
        {
            if (frontRpm > 0 && rearRpm > 0)
            {
                frontMGUAudio.Play();
                rearMGUAudio.Play();
                playOnce = true;
            }
            else
            {
                frontMGUAudio.Stop();
                rearMGUAudio.Stop();
            }
        }
        
    }

    void Update()
    {
        int[] custom = vehicle.data.Get(Channel.Custom);
        frontRpm = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.Rpm] / 1000.0f;
        frontMechanical = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.MechanicalTorque] / 1000.0f;
        rearRpm = custom[Perrinn424Data.RearMguBase + Perrinn424Data.Rpm] / 1000.0f;
        rearMechanical = custom[Perrinn424Data.RearMguBase + Perrinn424Data.MechanicalTorque] / 1000.0f;

        UpdateFrontMGU();
        UpdateRearMGU();

        if (frontMGUAudio.pitch < basePitchOfFrontMGU)
        {
            frontMGUAudio.pitch = basePitchOfFrontMGU;
        }
        if(frontMGUAudio.volume < baseVolumeOfFrontMGU)
        {
            frontMGUAudio.volume = baseVolumeOfFrontMGU;
        }
        if (rearMGUAudio.pitch < basePitchOfRearMGU)
        {
            rearMGUAudio.pitch = basePitchOfRearMGU;
        }
        if (rearMGUAudio.volume < baseVolumeOfRearMGU)
        {
            rearMGUAudio.volume = baseVolumeOfRearMGU;
        }
    }

    public void UpdateFrontMGU()
    {
        frontMGUAudio.pitch = basePitchOfFrontMGU + Mathf.Abs(frontRpm) * rpmGainFrontMGU;
        frontMGUAudio.volume = baseVolumeOfFrontMGU + Mathf.Abs(frontMechanical) * trqGainFrontMGU;
    }

    public void UpdateRearMGU()
    {
        rearMGUAudio.volume = baseVolumeOfRearMGU + Mathf.Abs(rearMechanical) * trqGainRearMGU;
    }
}