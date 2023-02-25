using System.Collections;
using System.Linq;
using UnityEngine;

public class WaitForNewError : CustomYieldInstruction
{

    public float error;
    public bool newError = false;

    public void SetError(float e)
    {
        this.error = e;
        newError = true;
    }

    public override bool keepWaiting
    {
        get
        {
            if (newError)
            {
                newError = false;
                return false;
            }

            return true;
        }
    }
}


public class TwiddleEnumerator
{
    public float[] parameters;
    public float[] bestParameters;
    public float[] delta;
    public float error;
    public float bestError;

    public float threshold = 0.0001f;
    public float maxIterations = Mathf.Infinity;

    public WaitForNewError waitForNewError;

    public int count = 0;


    public IEnumerator Optimize()
    {
        while (delta.Sum() > threshold && count < maxIterations)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                parameters[i] += delta[i];
                //wait new error
                yield return waitForNewError;
                error = waitForNewError.error;

                if (error < bestError) //there was improvement
                {
                    Debug.Log("Best");
                    Debug.Log(this.ToString());
                    bestError = error;
                    bestParameters = parameters.ToArray();
                    delta[i] *= 1.1f;
                }
                else //there was no improvement
                {
                    parameters[i] -= 2f * delta[i]; // go to the opposite direction
                    yield return waitForNewError;
                    error = waitForNewError.error;

                    if (error < bestError) //there was improvement
                    {
                        Debug.Log("Best");
                        Debug.Log(this.ToString());
                        bestError = error;
                        bestParameters = parameters.ToArray();
                        delta[i] *= 1.05f;
                    }
                    else
                    {
                        parameters[i] += delta[i]; // restore original value
                        delta[i] *= 0.95f;
                    }
                }
            }

            Debug.Log(this.ToString());
            count++;
        }
    }

    public override string ToString()
    {
        string param = string.Join(",", parameters.Select(p => p.ToString("F2")));
        return $"Error: {error}. Best Error: {bestError}. Iter: {count}. Params: {param}";
    }
}
