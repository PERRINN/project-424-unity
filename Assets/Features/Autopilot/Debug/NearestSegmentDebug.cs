using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VehiclePhysics;

namespace Perrinn424.AutopilotSystem
{
    [ExecuteInEditMode]
    public class NearestSegmentDebug : MonoBehaviour, IReadOnlyList<Vector3>
    {
        public VPReplayAsset asset;
        public NearestSegmentSearcher nearestSearcher;

        public Vector3 this[int index] => asset.recordedData[index].position;

        public int Count => asset.recordedData.Count;

        public float ratio;
        public float distance;
        public float rejectionDistance;
        public float operations;

        public void FixedUpdate()
        {
            if (nearestSearcher == null)
            {
                nearestSearcher = new NearestSegmentSearcher(this);
            }

            nearestSearcher.Search(this.transform);
            ratio = nearestSearcher.Ratio;
            distance = nearestSearcher.Distance;
            rejectionDistance = nearestSearcher.RejectionDistance;
            operations = nearestSearcher.Operations;

            //Vector3 pos = this.transform.position;
            //pos.y = nearestSearcher.ProjectedPosition.y;
            //this.transform.position = pos;

        }

        private void OnDrawGizmos()
        {
            for (int i = 0; i < Count - 1; i++)
            {
                Gizmos.DrawLine(this[i], this[i + 1]);
            }

            Gizmos.DrawWireSphere(nearestSearcher.Start, 0.05f);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(nearestSearcher.End, 0.05f);

            Gizmos.color = Color.green;
            //Gizmos.DrawSphere(nearestSearcher.ProjectedPosition, 0.05f);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(this.transform.position, 0.05f);

            Gizmos.DrawLine(nearestSearcher.ProjectedPosition, this.transform.position);

            Gizmos.DrawWireSphere(this[Count - 1], 5f);

        }

        public IEnumerator<Vector3> GetEnumerator()
        {
            throw new System.NotImplementedException();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    } 
}
