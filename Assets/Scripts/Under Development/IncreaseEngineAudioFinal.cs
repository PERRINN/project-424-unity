using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VehiclePhysics;


public class IncreaseEngineAudioFinal : MonoBehaviour
{
    public VehicleBase vehicle;
    public float increasingValueTRQ;
    public float increaseingValueRPM;
    public float basePitch;
    public float baseVolume;
    public AudioSource engineAudio;

    float frontRpm;
    float frontMechanical;
    float rearRpm;
    float rearMechanical;

    void Start()
    {
        
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
        engineAudio.pitch = basePitch + Mathf.Abs(frontRpm) * increaseingValueRPM;
        engineAudio.volume = baseVolume + Mathf.Abs(frontMechanical) * increasingValueTRQ;
    }

    public void UpdateRearEngine()
    {
        engineAudio.pitch = basePitch + Mathf.Abs(rearRpm) * increaseingValueRPM;
        engineAudio.volume = baseVolume + Mathf.Abs(rearMechanical) * increasingValueTRQ;
    }
}
