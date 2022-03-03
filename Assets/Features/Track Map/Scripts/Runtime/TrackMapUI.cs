using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Perrinn424.TrackMapSystem
{
    public class TrackMapUI : UIBehaviour
    {
        [SerializeField]
        private TrackMapData data;
        private TrackMap trackMap;

        [SerializeField][FormerlySerializedAs("trackReferences")]
        internal TransformTrackReference[] traformTrackReferences = default;
        
        [SerializeField]
        private TelemetryTrackReference telemetryTrackReference = default;

        private List<BaseTrackReference> trackReferences;

        [SerializeField]
        private Image defaultIcon = default;


        protected override void OnEnable()
        {
            Init();
            defaultIcon.gameObject.SetActive(false);
        }

        private void Init()
        {
            trackReferences =
                traformTrackReferences
                .Cast<BaseTrackReference>()
                .Concat(new[] { telemetryTrackReference })
                .ToList();

            foreach (BaseTrackReference trackReference in trackReferences)
            {
                trackReference.Init();
            }

            trackMap = data.CreateTrackMap();
            
            CalculateMatrices();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            if (trackMap == null)
            {
                return;
            }

            CalculateMatrices();
        }

        private void CalculateMatrices()
        {
            trackMap.CalculateTRS((RectTransform)transform);
        }

        void Update()
        {
            foreach (BaseTrackReference trackReference in trackReferences)
            {
                Vector3 localPosition = trackMap.FromWorldPositionToLocalRectTransformPosition(trackReference.Position);
                trackReference.SetLocalPosition(localPosition);
            }
        }

        public void AddTrackReference(Transform newTrackReference, Color c)
        {
            Image newIcon = Object.Instantiate(defaultIcon, defaultIcon.transform.parent);
            newIcon.gameObject.SetActive(true);
            TransformTrackReference newReference = new TransformTrackReference(newTrackReference, newIcon, c);
            newReference.Init();
            trackReferences.Add(newReference);
        }
    } 
}