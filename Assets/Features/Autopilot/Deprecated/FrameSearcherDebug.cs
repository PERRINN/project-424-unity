using UnityEngine;
using VehiclePhysics;


namespace Perrinn424.AutopilotSystem
{
    [ExecuteInEditMode]
    public class FrameSearcherDebug : MonoBehaviour
    {
        public VPReplayAsset asset => provider.replayAsset;
        public Vector3[] positions;
        public int index;
        public int nextIndex;

        public Transform a;
        public Transform b;

        IFrameSearcher frameSearcher;

        public AutopilotProvider provider;

        // Update is called once per frame
        void Update()
        {
            if (frameSearcher == null) frameSearcher = new HeuristicFrameSearcher(provider.replayAsset.recordedData, 5, -10, 100);


            frameSearcher.Search(this.transform);
            index = frameSearcher.ClosestFrame1;
            nextIndex = frameSearcher.ClosestFrame2;
            this.transform.position = new Vector3(transform.position.x, provider.replayAsset.recordedData[index].position.y, transform.position.z);

            a.transform.position = provider.replayAsset.recordedData[index].position;
            b.transform.position = provider.replayAsset.recordedData[nextIndex].position;
        }


        private void OnDrawGizmos()
        {
            DrawPath();
            DrawIndex();
        }

        private void DrawPath()
        {
            for (int i = 0; i < asset.recordedData.Count - 1; i++)
            {
                Vector3 current = asset.recordedData[i].position;
                Vector3 next = asset.recordedData[i + 1].position;
                Gizmos.DrawLine(current, next);
            }
        }

        private void DrawIndex()
        {
            Gizmos.color = Color.white;

            for (int i = -2; i < 4; i++)
            {
                Gizmos.DrawSphere(asset.recordedData[i + index].position, 0.1f);
            }

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(asset.recordedData[index].position, 0.1f);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(asset.recordedData[nextIndex].position, 0.1f);
        }
    } 
}
