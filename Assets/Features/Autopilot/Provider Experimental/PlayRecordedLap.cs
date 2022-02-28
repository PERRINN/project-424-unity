using Perrinn424.AutopilotSystem;
using Perrinn424.TelemetryLapSystem;
using Perrinn424.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayRecordedLap : MonoBehaviour
{
    public RecordedLap lap;

    private float playingTime;
    private float dt;
    private CircularIndex circularIndex;
    
    public void OnEnable()
    {
        dt = 1f / lap.frequency;
        circularIndex = new CircularIndex(lap.Count);
    }


    private void FixedUpdate()
    {
        playingTime += Time.deltaTime;

        (int frameIndex, float interpolationKey) = GetKeyFrame(playingTime);
        circularIndex.Assign(frameIndex);
        Sample s = Sample.Lerp(lap[circularIndex], lap[circularIndex + 1], interpolationKey);
        Set(s);
    }


    private (int, float) GetKeyFrame(float time)
    {
        float key = time / dt;
        key = key % lap.Count;
        int frameIndex = Mathf.FloorToInt(key);
        float percentage = key - frameIndex;
        return (frameIndex, percentage);
    }



    //public CircularIndex index;
    //public Frequency frequency;

    //private void OnEnable()
    //{
    //    frequency.frequency = (int)lap.frequency;
    //    index = new CircularIndex(lap.Count);
    //}

    //private void FixedUpdate()
    //{
    //    if (frequency.Update(Time.deltaTime))
    //    {
    //        Set(lap[++index]);
    //    }
    //}

    private void Set(Sample s)
    {
        this.transform.position = s.position;
        this.transform.rotation = s.rotation;
    }
}
