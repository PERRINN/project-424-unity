using Perrinn424.TrackMapSystem;
using Perrinn424.Utilities;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using VehiclePhysics;

namespace Perrinn424.TrackMapSystem
{
    public class TrackMapCalibrator : MonoBehaviour
    {
        public Image mapImage;
        public Image referenceImage;
        public VPReplayAsset replay;

        private CircularIterator<Vector3> iterator;

        public Vector3[] positions;
        public TrackPosition[] trackPositions;

        public TrackMap trackMap;
        public TrackMapData trackMapData;

        public struct TrackPosition
        {
            public Vector3 position;
            public Image image;
        }


        private void Awake()
        {
            replay.GetPositions(1.0f, out var positions, out _);
            iterator = new CircularIterator<Vector3>(positions);


            trackPositions = new TrackPosition[positions.Length];

            for (int i = 0; i < positions.Length; i++)
            {
                trackPositions[i] = new TrackPosition() { position = positions[i], image = GameObject.Instantiate(referenceImage, referenceImage.transform.parent) };
            }

            referenceImage.color = Color.red;
            referenceImage.transform.SetSiblingIndex(referenceImage.transform.childCount);

            StartCoroutine(NextPosition());

            mapImage.sprite = trackMapData.map;

            trackMap = trackMapData.CreateTrackMap();
        }

        private IEnumerator NextPosition()
        {
            var wait = new WaitForSeconds(0.05f);
            while (true)
            {
                iterator.MoveNext();
                yield return wait;
            }
        }

        public void Update()
        {
            trackMap.CalculateTRS((RectTransform)transform);
            trackMap.position = trackMapData.position;
            trackMap.rotation = trackMapData.rotation;
            trackMap.scale = trackMapData.scale;

            Vector3 pos = iterator.Current;
            Vector3 localPos = trackMap.FromWorldPositionToLocalRectTransformPosition(pos);
            referenceImage.transform.localPosition = localPos;

            for (int i = 0; i < trackPositions.Length; i++)
            {
                localPos = trackMap.FromWorldPositionToLocalRectTransformPosition(trackPositions[i].position);
                trackPositions[i].image.transform.localPosition = localPos;
            }
        }
    } 
}
