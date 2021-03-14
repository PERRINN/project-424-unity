using System;
using UnityEngine;
using UnityEngine.UI;
using VehiclePhysics;

namespace Perrinn424.TrackMapSystem
{
    public class TrackMapCalibrationHelper : MonoBehaviour
    {
        [SerializeField]
        private Color color = default;

        [SerializeField]
        private TrackMap trackMap = default;
        [SerializeField]
        private Image reference = default;

        [SerializeField]
        private VPReplayAsset replay = default;

        [SerializeField]
        private float timeStepGetPositions = 0.1f;

        [ContextMenu("Create References")]
        private void CreateReferences()
        {
            Clean();

            replay.GetPositions(timeStepGetPositions, out var positions, out _);
            trackMap.trackReferences = new TrackMap.TrackReference[positions.Length];

            for (int i = 0; i < positions.Length; i++)
            {
                Vector3 position = positions[i];
                Transform child = new GameObject("Reference").transform;
                child.parent = this.transform;
                child.position = position;
                GameObject newReference = Instantiate(reference.gameObject, trackMap.transform);
                newReference.name = child.name;
                TrackMap.TrackReference trackReference = new TrackMap.TrackReference(child, newReference.GetComponent<Image>(), color);
                trackMap.trackReferences[i] = trackReference;
            }

            //int referencesCount = worldReferenceParent.childCount;
        }


        private void Clean()
        {
            RemoveMapChilds();
            RemoveTransformChilds();
        }

        private void RemoveMapChilds()
        {
            foreach (Transform child in trackMap.transform)
            {
                if (child == reference.transform)
                    continue;
                try
                {
                    GameObject.DestroyImmediate(child.gameObject);
                }
                catch (Exception) { }
            }

            trackMap.trackReferences = new TrackMap.TrackReference[0];
        }

        private void RemoveTransformChilds()
        {
            foreach (Transform child in transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
    } 
}
