using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Perrinn424.TrackMapSystem
{
    [ExecuteInEditMode]
    public class TrackMap : MonoBehaviour
    {

        [SerializeField]
        private Vector3 center = Vector3.zero;
        [SerializeField]
        private Vector3 size = Vector3.one;

        [SerializeField]
        private float rotation = 0;

        [SerializeField]
        private bool invertX = false;
        [SerializeField]
        private bool invertZ = false;

        [SerializeField][FormerlySerializedAs("trackReferences")]
        internal TransformTrackReference[] traformTrackReferences = default;
        
        [SerializeField]
        private TelemetryTrackReference telemetryTrackReference = default;

        private BaseTrackReference [] trackReferences;

        private void OnEnable()
        {
            trackReferences = 
                traformTrackReferences
                .Cast<BaseTrackReference>()
                .Concat(new [] { telemetryTrackReference })
                .ToArray();

            foreach (BaseTrackReference trackReference in trackReferences)
            {
                trackReference.Init();
            }
        }

        void Update()
        {
            Matrix4x4 worldToCircuit = CalculateWorldToCircuitMatrix();
            Matrix4x4 localCircuitToCanvas = CalculateCircuitToCanvasMatrix();

            foreach (BaseTrackReference trackReference in trackReferences)
            {
                trackReference.WorldToCanvas(worldToCircuit, localCircuitToCanvas);
            }
        }

        private Matrix4x4 CalculateWorldToCircuitMatrix()
        {
            return Matrix4x4.Translate(center);
        }

        private Matrix4x4 CalculateCircuitToCanvasMatrix()
        {
            RectTransform rectTransform = (RectTransform)transform;
            float xScale = rectTransform.rect.width / size.x;
            xScale *= invertX ? -1 : 1;
            float zScale = rectTransform.rect.height / size.z;
            zScale *= invertZ ? -1 : 1;
            Vector3 scale = new Vector3(xScale, 0f, zScale);

            Quaternion q = Quaternion.AngleAxis(90f, Vector3.right); //Converting z axis into y axis in the canvas
            q = q * Quaternion.AngleAxis(rotation, Vector3.up);
            Matrix4x4 localCircuitToCanvas = Matrix4x4.TRS(Vector3.zero, q, scale);
            return localCircuitToCanvas;
        }

        private void OnDrawGizmosSelected()
        {
            Color c = Color.green;
            c.a = 0.5f;
            Gizmos.color = c;
            Gizmos.DrawCube(center, size);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(center, 15f);
        }
    } 
}