using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//https://martin-thoma.com/twiddle/
//https://medium.com/@carola.amu/pid-controller-for-autonomous-vehicle-151d90e49855
public class Twiddle
{
    public float[] parameters;
    public float[] delta;
    public float bestError;

    public float threshold = 0.01f;

    public Func<float[] , float> calculateError;

    public void Optimize()
    {
        while (delta.Sum() > threshold)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                parameters[i] += delta[i];
                float error = calculateError(parameters);

                if (error < bestError) //there was improvement
                {
                    bestError = error;
                    delta[i] *= 1.1f;
                }
                else //there was no improvement
                {
                    parameters[i] -= 2f * delta[i]; // go to the opposite direction
                    error = calculateError(parameters);

                    if (error < bestError) //there was improvement
                    {
                        bestError = error;
                        delta[i] *= 1.05f;
                    }
                    else
                    {
                        parameters[i] += delta[i]; // restore original value
                        delta[i] *= 0.95f;
                    }
                }
            }
        }
    }
}
