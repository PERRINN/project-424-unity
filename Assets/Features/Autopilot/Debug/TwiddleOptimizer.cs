using Perrinn424.AutopilotSystem;
using Perrinn424.TelemetryLapSystem;
using System.Collections;
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

    public override void OnEnableVehicle()
    {
        vehicle.GetComponentInChildren<VPTelemetry>().gameObject.SetActive(false);
        vehicle.GetComponentInChildren<TelemetryLapWriter>().gameObject.SetActive(false);
        Time.timeScale = timeScale;
        lapTimer.onLap += OnLapHandler;
    }

    private IEnumerator Start()
    {
        yield return null;
        autopilot.ToggleStatus();
        waitForNewError = new WaitForNewError();
        twiddleEnumerator = new TwiddleEnumerator();
        twiddleEnumerator.parameters = parameters;
        twiddleEnumerator.delta = delta;
        twiddleEnumerator.threshold = 0.000001f;
        twiddleEnumerator.bestError = Mathf.Infinity;
        twiddleEnumerator.waitForNewError = waitForNewError;
        twiddleEnumerator.maxIterations = maxIter;
        yield return StartCoroutine(twiddleEnumerator.Optimize());

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPaused = true;
#endif
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
        local.z = twiddleEnumerator.parameters[0];
        autopilot.lateralCorrector.localApplicationPosition = local;
    }
}
