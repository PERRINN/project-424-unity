using System;
using Perrinn424.LiveLink;
class TelemetryRatiosHarness
{
    static void Expect(float front, float rear, float expected)
    {
        float actual = TelemetryRatios.FrontPercent(front, rear);
        if (float.IsNaN(expected) ? !float.IsNaN(actual) : Math.Abs(actual - expected) > 0.001f) throw new Exception("Incorrect front / total ratio");
    }
    static void Main()
    {
        Expect(200, 300, 40); Expect(-200, -300, 40); Expect(100, -50, 200);
        Expect(1000000, -999999.9f, float.NaN); Expect(0, 100, 0); Expect(100, 0, 100); Expect(0, 0, float.NaN); Expect(100, -100, float.NaN);
        Expect(float.NaN, 100, float.NaN); Expect(float.PositiveInfinity, 100, float.NaN);
        Console.WriteLine("PASS: front / total percentages, regeneration, opposing axle flows, zero/near-zero totals and invalid sources");
    }
}
