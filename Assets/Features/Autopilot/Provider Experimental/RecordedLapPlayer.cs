using Perrinn424.AutopilotSystem;
using Perrinn424.Utilities;
using UnityEngine;

public class RecordedLapPlayer : MonoBehaviour
{
    public enum Type
    {
        Discrete,
        Continous
    }

    public RecordedLap lap;

    private float playingTime;
    private float dt;
    private CircularIndex circularIndex;

    private bool isPlaying;
    public Type reproductionType;
    public float reproductionSpeed = 1f;

    public float PlayingTime => playingTime;
    public float TotalTime => lap.Count * dt;
    private void Awake()
    {
        dt = 1f / lap.frequency;
        circularIndex = new CircularIndex(lap.Count);
    }

    public void SetPlayingTime(float time)
    {
        playingTime = time;

        (int frameIndex, float interpolationKey) = GetKeyFrame(playingTime);
        circularIndex.Assign(frameIndex);

        Sample s;
        if (reproductionType == Type.Discrete)
        {
            s = lap[circularIndex];
        }
        else
        {
            s = Sample.Lerp(lap[circularIndex], lap[circularIndex + 1], interpolationKey);
        }

        Set(s);
    }

    private void FixedUpdate()
    {
        if (isPlaying)
        {
            SetPlayingTime(playingTime + Time.deltaTime * reproductionSpeed);
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
