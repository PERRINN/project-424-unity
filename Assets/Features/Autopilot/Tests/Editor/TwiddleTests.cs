using NUnit.Framework;
using System;
using UnityEngine;

public class TwiddleTests
{
    [Test]
    public void TwiddleTest()
    {
        float[] roots = new float[] { 3, -2, -4 };
        Func<float[], float> errorFunc =
            (parameters) => Mathf.Pow(parameters[0] - roots[0], 2) + Mathf.Pow(parameters[1] - roots[1], 2) + Mathf.Pow(parameters[2] - roots[2], 2);

        Twiddle twiddle = new Twiddle();
        twiddle.parameters = new float[] { 0, 0, 0 };
        twiddle.delta = new float[] { 1, 1, 1 };
        twiddle.threshold = 0.0001f;
        twiddle.calculateError = errorFunc;
        twiddle.bestError = Mathf.Infinity;
        twiddle.Optimize();

        Debug.Log(twiddle.bestError);
        Assert.That(twiddle.parameters[0], Is.EqualTo(roots[0]).Within(1e-2));
        Assert.That(twiddle.parameters[1], Is.EqualTo(roots[1]).Within(1e-2));
        Assert.That(twiddle.parameters[2], Is.EqualTo(roots[2]).Within(1e-2));
    }
}
