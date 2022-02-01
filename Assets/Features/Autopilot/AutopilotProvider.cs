using UnityEngine;
using VehiclePhysics;

namespace Perrinn424.AutopilotSystem
{
    public class AutopilotProvider : MonoBehaviour
    {
        public int selected;
        public VPReplayAsset[] replayAssets;

        public int[] frames = new int[0];
        public VPReplayAsset GetReplayAsset()
        {
            return replayAssets[selected];
        }

        public VPReplayAsset GetReplayAsset(int index)
        {
            return replayAssets[index];
        }

        public void SetSelected(int index)
        {
            selected = index;
        }

        public float TimeStep => GetReplayAsset().timeStep; 


        public int Count => GetReplayAsset().recordedData.Count;

        public VPReplay.Frame this[int index]
        {
            get => GetReplayAsset().recordedData[index];
        }


        private void OnDrawGizmos()
        {
            VPReplayAsset asset = GetReplayAsset();

            float seconds = 20f;
            int pointCount = (int)(seconds / asset.timeStep);

            //for (int i = pointCount; i < asset.recordedData.Count - 1; i++)
            for (int i = 0; i < asset.recordedData.Count - 1; i++)
            {
                Vector3 origin = asset.recordedData[i].position;
                Vector3 destination = asset.recordedData[i + 1].position;
                Gizmos.DrawLine(origin, destination);
                //Gizmos.DrawSphere(origin, 0.1f);
                //Gizmos.DrawSphere(destination, 0.1f);

            }

            for (int i = 0; i < frames.Length; i++)
            {
                Vector3 pos = this[frames[i]].position;
                Gizmos.DrawSphere(pos, 0.5f);
            }
        }

        public void DebugFrames(int[] frames)
        {
            this.frames = frames;

            //for (int i = 0; i < frames.Length; i++)
            //{
            //    string pos = Vector3ToString(this[frames[i]].position);
            //    print($"{frames[i]} pos: {pos}");
            //}
        }

        private string Vector3ToString(Vector3 v)
        {
            return $"({v.x:F5}, {v.y:F5}, {v.z:F5})";
        }
    } 
}
