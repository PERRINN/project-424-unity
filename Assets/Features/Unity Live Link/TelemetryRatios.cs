using System;

namespace Perrinn424.LiveLink
{
    public static class TelemetryRatios
    {
        // Preserve signed front / total, even during opposing axle flows; never invent a zero-flow balance.
        public static float FrontPercent(float front, float rear)
        {
            double total = (double)front + rear;
            if (float.IsNaN(front) || float.IsNaN(rear) || float.IsInfinity(front) || float.IsInfinity(rear)
                || Math.Abs(total) <= Math.Max(1e-6, (Math.Abs((double)front) + Math.Abs((double)rear)) * 1e-6)) return float.NaN;
            return (float)(100.0 * front / total);
        }
    }
}
