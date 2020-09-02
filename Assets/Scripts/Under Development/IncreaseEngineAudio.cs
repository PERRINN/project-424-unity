using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VehiclePhysics;


public class IncreaseEngineAudio : MonoBehaviour
{
    //int oldRpm;
    int newRpm;
    int newTorque;
    //string oldRpmString;
    public Text textRpms;
    public Text textTorque;
    public AudioSource engineAudio;

    // Start is called before the first frame update
    void Start()
    {
        //engineAudio = GetComponent<AudioSource>();
        //textRpms = GetComponent<Text>();

        //oldRpmString = textRpms.text;
        //oldRpm = int.Parse(oldRpmString); //setting the base rpm when the program starts
        
        
    }

    void Update()
    {
        newTorque = int.Parse(textTorque.text);
        newRpm = int.Parse(textRpms.text);
        if(newRpm <= 20000)
        {
            engineAudio.pitch = 0.22f + newRpm * 0.000139f;
        }
        if(newTorque <= 20000)
        {
            engineAudio.volume = 0.45f + newTorque * 0.000139f;
        }
        


        //if (newRpm > oldRpm)//when the rpms are higher than the previous it will increase
        //{
            
        //    engineAudio.pitch += 0.10f;
            
        //}
        //if(newRpm < oldRpm) //when the rpms get smaller the pitch will decrease
        //{
        //    engineAudio.pitch -= 0.10f;
        //}
        //newRpm = oldRpm;

        //while(newRpm > oldRpm)
        //{
        //    engineAudio.pitch += 0.10f;
        //    newRpm = oldRpm;
        //}
    }

}

