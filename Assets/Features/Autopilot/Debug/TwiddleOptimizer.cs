using Perrinn424.AutopilotSystem;
using System.Collections;
using UnityEditor;
using UnityEngine;
using VehiclePhysics;
using VehiclePhysics.Timing;

public class TwiddleOptimizer : VehicleBehaviour
{
    private WaitForNewError waitForNewError;
    private TwiddleEnumerator twiddleEnumerator;

    public LapTimer lapTimer;
    public Autopilot autopilot;
    public float timeScale;
    public float maxIter = 1;
    public float[] parameters;
    public float[] bestParameters;
    public float[] delta;

    public bool firstLap = true;

    private IEnumerator Start()
    {
        waitForNewError = new WaitForNewError();
        twiddleEnumerator = new TwiddleEnumerator();
        twiddleEnumerator.parameters = parameters;
        twiddleEnumerator.delta = delta;
        twiddleEnumerator.threshold = 0.000001f;
        twiddleEnumerator.bestError = Mathf.Infinity;
        twiddleEnumerator.waitForNewError = waitForNewError;
        twiddleEnumerator.maxIterations = maxIter;
        yield return StartCoroutine(twiddleEnumerator.Optimize());
        EditorApplication.isPaused = true;

    }

    public override void OnEnableVehicle()
    {
        Time.timeScale = timeScale;
        lapTimer.onLap += OnLapHandler;
        autopilot.ToggleStatus();
    }

    private void OnLapHandler(float lapTime, bool validBool, float[] sectors, bool[] validSectors)
    {
        if (firstLap == true) //ignore first lap
        {
            firstLap = false;
            return;
        }

        float error = lapTime - autopilot.CalculateDuration();
        error = Mathf.Abs(error);
        waitForNewError.SetError(error);
        //print(twiddleEnumerator.ToString());

    }

    private void Update()
    {
        //autopilot.lateralCorrector.kp = twiddleEnumerator.parameters[0];
        //autopilot.lateralCorrector.kd = twiddleEnumerator.parameters[1];
        Vector3 local = autopilot.lateralCorrector.localApplicationPosition;
        local.y = twiddleEnumerator.parameters[0];
        autopilot.lateralCorrector.localApplicationPosition = local;
    }
}
