using UnityEngine;
using UnityEngine.UI;

namespace Perrinn424.UI
{
    public class TrackMapCalibrationHelper : MonoBehaviour
    {
        [SerializeField]
        private Color color;

        [SerializeField]
        private TrackMap trackMap;
        [SerializeField]
        private Image reference;
        [SerializeField]
        private Transform worldReferenceParent;

        [ContextMenu("Create References")]
        private void CreateReferences()
        {

            foreach (Transform child in trackMap.transform)
            {
                if(child == reference.transform)
                    continue;

                GameObject.DestroyImmediate(child.gameObject);
            }


            int referencesCount = worldReferenceParent.childCount;

            trackMap.trackReferences = new TrackMap.TrackReference[referencesCount];

            for (int i = 0; i < referencesCount; i++)
            {
                Transform child = worldReferenceParent.GetChild(i);
                GameObject newReference = Instantiate(reference.gameObject, trackMap.transform);
                newReference.name = child.name;
                TrackMap.TrackReference trackReference = new TrackMap.TrackReference(child, newReference.GetComponent<Image>(), color);
                trackMap.trackReferences[i] = trackReference;
            }
        }
    } 
}
