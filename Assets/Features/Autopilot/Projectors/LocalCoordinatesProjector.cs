using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    public class LocalCoordinatesProjector : IProjector
    {
        public (Vector3, float) Project(Transform t, Vector3 start, Vector3 end)
        {
            Vector3 endLocal = t.transform.InverseTransformPoint(end);
            Vector3 startLocal = t.transform.InverseTransformPoint(start);
            float ratio = InverseLerpUncampled(startLocal.z, endLocal.z, 0f);

            Vector3 projectedPosition = Vector3.LerpUnclamped(start, end, ratio);

            return (projectedPosition, ratio);
        }

        private static float InverseLerpUncampled(float a, float b, float value)
        {
            if (a != b)
                return (value - a) / (b - a);
            else
                return 0.0f;
        }
    }
}
