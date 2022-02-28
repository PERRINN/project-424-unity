using Perrinn424.AutopilotSystem;
using Perrinn424.Utilities;
using UnityEngine;

public class RecordedLapPlayer : MonoBehaviour
{
    public RecordedLap lap;

    private float playingTime;
    private float dt;
    private CircularIndex circularIndex;

    private bool isPlaying;
    
    private void Awake()
    {
        dt = 1f / lap.frequency;
        circularIndex = new CircularIndex(lap.Count);
    }

    private void SetPlayingTime(float time)
    {
        playingTime = time;

        (int frameIndex, float interpolationKey) = GetKeyFrame(playingTime);
        circularIndex.Assign(frameIndex);
        Sample s = Sample.Lerp(lap[circularIndex], lap[circularIndex + 1], interpolationKey);
        Set(s);
    }

    private void FixedUpdate()
    {
        if (isPlaying)
        {
            SetPlayingTime(playingTime + Time.deltaTime);
        }
    }

    private (int, float) GetKeyFrame(float time)
    {
        float key = time / dt;
        key = key % lap.Count;
        int frameIndex = Mathf.FloorToInt(key);
        float percentage = key - frameIndex;
        return (frameIndex, percentage);
    }

    private void Set(Sample s)
    {
        this.transform.position = s.position;
        this.transform.rotation = s.rotation;
    }

    public void Play()
    {
        isPlaying = true;
    }

    public void Pause()
    {
        isPlaying = false;
    }

    public void Stop()
    {
        Pause();
        SetPlayingTime(0f);
    }

    public void Restart()
    {
        Stop();
        Play();
    }
}
