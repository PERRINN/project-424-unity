using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    public interface INearestNeighbourSearcher
    {
        int Index { get; }
        float Distance { get; }
        Vector3 Position { get; }
        void  Search(Vector3 position);
    }
}
