using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    public interface INearestNeighbourSearcher
    {
        (int, float) Search(Vector3 position);

    }
}
