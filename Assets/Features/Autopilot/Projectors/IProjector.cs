using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    public interface IProjector
    {
        (Vector3, float) Project(Transform t, Vector3 start, Vector3 end);
    } 
}
