using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineHand : MonoBehaviour
{
    private WaitForNewError waitForNewError;
    private TwiddleEnumerator twiddleEnumerator;
    public float[] parameters;
    public float[] delta;
    private IEnumerator Start()
    {
        parameters = new float[] { 0, 0, 0 };
        delta = new float[] { 1, 1, 1 };
        waitForNewError = new WaitForNewError();
        twiddleEnumerator = new TwiddleEnumerator();
        twiddleEnumerator.parameters = parameters;
        twiddleEnumerator.delta = delta;
        twiddleEnumerator.threshold = 0.0001f;
        twiddleEnumerator.bestError = Mathf.Infinity;
        twiddleEnumerator.waitForNewError = waitForNewError;
        twiddleEnumerator.maxIterations = 5;
        yield return StartCoroutine(twiddleEnumerator.Optimize());
        print("end");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            float[] roots = new float[] { 3, -2, -4 };
            float[] parameters = twiddleEnumerator.parameters;
            float error  = Mathf.Pow(parameters[0] - roots[0], 2) + Mathf.Pow(parameters[1] - roots[1], 2) + Mathf.Pow(parameters[2] - roots[2], 2);
            waitForNewError.SetError(error);
        }
    }
}
