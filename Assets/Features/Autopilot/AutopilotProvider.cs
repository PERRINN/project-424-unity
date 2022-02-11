using UnityEngine;
using VehiclePhysics;

namespace Perrinn424.AutopilotSystem
{
    public class AutopilotProvider : MonoBehaviour
    {
        public VPReplayAsset replayAsset;

        public float TimeStep => replayAsset.timeStep; 

        public int Count => replayAsset.recordedData.Count;

        public VPReplay.Frame this[int index]
        {
            get => replayAsset.recordedData[index];
        }

        public float CalculateDuration()
        {
            return Count * TimeStep;
        }


        private void OnDrawGizmos()
        {
            VPReplayAsset asset = replayAsset;

            float seconds = 20f;
            int pointCount = (int)(seconds / asset.timeStep);

            for (int i = 0; i < asset.recordedData.Count - 1; i++)
            {
                Vector3 origin = asset.recordedData[i].position;
                Vector3 destination = asset.recordedData[i + 1].position;
                Gizmos.DrawLine(origin, destination);
            }
        }
    } 
}
