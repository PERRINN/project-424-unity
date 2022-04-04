using Perrinn424.AutopilotSystem;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public float offset;
        public string result;

        public void Close()
        {
            string diffTimesStr = String.Join(";", diffTimes.Select(d => d.ToString("F4")));
            result = $"{diffTimesStr:F4};{kp:F4};{kd:F4};{max:F4};{offset:F4}";
            result = result.Replace('.', ',');
        }
    }

    public LapTimer lapTimer;
    public Autopilot autopilot;
    public int lapsPerSample;
    public float timeScale;

    public Sample[] samples;
    public int currentSampleIndex;

    public Sample CurrentSample => samples[currentSampleIndex];

    public override void OnEnableVehicle()
    {
        Time.timeScale = timeScale;
        lapTimer.onLap += OnLapHandler;
        lapTimer.onBeginLap += BeginLapHandler;
        autopilot.ToggleStatus();
    }

    private void BeginLapHandler()
    {
        autopilot.lateralCorrector.kp = CurrentSample.kp;
        autopilot.lateralCorrector.kd = CurrentSample.kd;
        autopilot.lateralCorrector.max = CurrentSample.max;
        autopilot.positionOffset = CurrentSample.offset;
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
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPaused = true;
#endif
            }
        }
    }

    private void Reset()
    {
        lapTimer = FindObjectOfType<LapTimer>();
        autopilot = this.GetComponent<Autopilot>();
        lapsPerSample = 5;
        timeScale = 50;
        Sample s = new Sample
        {
            diffTimes = new List<float>(),
            kp = autopilot.lateralCorrector.kp,
            kd = autopilot.lateralCorrector.kd,
            max = autopilot.lateralCorrector.max,
            offset = autopilot.positionOffset
        };

        samples = new Sample[] { s };
    }
}
