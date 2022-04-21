using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    public class AreaProjector : IProjector
    {
        public (Vector3, float) Project(Transform t, Vector3 start, Vector3 end)
        {
            Vector3 segment = end - start;
            float @base = (segment).magnitude;
            float leftSide = (t.position - start).magnitude;
            float rightSide = (t.position - end).magnitude;

            //Heron's formula
            float semiperimeter = (@base + leftSide + rightSide) / 2.0f;
            float area = semiperimeter * (semiperimeter - @base) * (semiperimeter - leftSide) * (semiperimeter - rightSide);
            area = Mathf.Sqrt(area);
            float height = 2.0f * area / @base;

            float p = Mathf.Sqrt(leftSide * leftSide - height * height);

            float ratio = p / @base;
            Vector3 projectedPosition = start + segment * ratio;

            return (projectedPosition, ratio);

        }
    }
}
