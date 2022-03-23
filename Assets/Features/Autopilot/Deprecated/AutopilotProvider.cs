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
    } 
}
