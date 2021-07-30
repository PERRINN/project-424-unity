using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

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

        private BaseTrackReference [] trackReferences;
        
        protected override void OnEnable()
        {
            Init();
        }

        private void Init()
        {
            trackReferences =
                traformTrackReferences
                .Cast<BaseTrackReference>()
                .Concat(new[] { telemetryTrackReference })
                .ToArray();

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
    } 
}