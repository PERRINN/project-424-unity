using System;
using System.Diagnostics;
public class CustomTimer : IDisposable
{
    private readonly string timerName;
    private readonly int numTests;
    private readonly Stopwatch watch;

    public long Milliseconds { get; private set; }

    // give the timer a name, and a count of the
    // number of tests we're running
    public CustomTimer(string timerName, int numTests)
    {
        this.timerName = timerName; 
        this.numTests = numTests;

        if (this.numTests <= 0)
        {
            this.numTests = 1;
        }

        watch = Stopwatch.StartNew();
    }
    // automatically called when the 'using()' block ends
    public void Dispose()
    {
        watch.Stop();
        Milliseconds = watch.ElapsedMilliseconds;

        string msg = $"{timerName} finished: {Milliseconds:F2} milisecond total, {Milliseconds / numTests:F6} milisecond per-test for {numTests} tests";
        UnityEngine.Debug.Log(msg);
    }
}