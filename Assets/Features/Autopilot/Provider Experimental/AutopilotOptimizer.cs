using Perrinn424.AutopilotSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VehiclePhysics;
using VehiclePhysics.Timing;

public class AutopilotOptimizer : VehicleBehaviour
{
    [Serializable]
    public class Sample
    {
        public List<float> diffTimes = new List<float>();
        public float kp;
        public float kd;
        public float max;
        public string result;

        public void Close()
        {
            string diffTimesStr = String.Join(";", diffTimes.Select(d => d.ToString("F4")));
            result = String.Join(";", diffTimesStr, kp.ToString("F4"), kd.ToString("F4"), max.ToString("F4"));
            result = result.Replace('.', ',');
        }
    }

    public LapTimer lapTimer;
    public AutopilotExperimental autopilot;
    public int lapsPerSample;
    public float timeScale;

    private List<float> diff = new List<float>();

    public Sample[] samples;
    public int currentSampleIndex;

    public Sample CurrentSample => samples[currentSampleIndex];

    //private void Awake()
    //{
    //    CurrentSample.diffTimes.Add(0.1f);
    //    CurrentSample.diffTimes.Add(0.2f);
    //    CurrentSample.Close();
    //}
    public override void OnEnableVehicle()
    {
        Time.timeScale = timeScale;
        lapTimer.onLap += OnLapHandler;
        lapTimer.onBeginLap += BeginLapHandler;
    }

    private void BeginLapHandler()
    {
        autopilot.lateralCorrector.kp = CurrentSample.kp;
        autopilot.lateralCorrector.kd = CurrentSample.kd;
        autopilot.lateralCorrector.max = CurrentSample.max;
    }

    public override void OnDisableVehicle()
    {
        lapTimer.onLap -= OnLapHandler;
        lapTimer.onBeginLap -= BeginLapHandler;

    }

    private void OnLapHandler(float lapTime, bool validBool, float[] sectors, bool[] validSectors)
    {
        CurrentSample.diffTimes.Add(lapTime - autopilot.CalculateDuration());
        if (CurrentSample.diffTimes.Count == lapsPerSample)
        {
            CurrentSample.Close();
            currentSampleIndex++;

            if (currentSampleIndex >= samples.Length)
            {
                EditorApplication.isPaused = true;
            }
        }

        //diff.Add(lapTime - autopilot.CalculateDuration());

        //if (diff.Count == 5)
        //{
        //    foreach (float d in diff)
        //    {
        //        print(d);
        //    }

        //    EditorApplication.isPlaying = false;
        //}
    }

}
