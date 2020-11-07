using UnityEngine;
using UnityEngine.UI;

namespace Perrinn424.UI
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
        private Transform worldReferenceParent = default;

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
