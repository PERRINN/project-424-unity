using System;
using UnityEngine;
using UnityEngine.UI;

namespace Perrinn424.UI
{
    [ExecuteInEditMode]
    public class TrackMap : MonoBehaviour
    {
        [Serializable]
        public class TrackReference
        {
            [SerializeField]
            private Transform world = default;
            [SerializeField]
            private Image ui = default;

            [SerializeField]
            private Color color = default;

            public TrackReference(Transform world, Image ui, Color color)
            {
                this.world = world;
                this.ui = ui;
                this.color = color;
            }

            public void WorldToCanvas(Matrix4x4 worldToLocalCircuit, Matrix4x4 localCircuitToCanvas)
            {
                Vector3 localCircuitPosition = worldToLocalCircuit.inverse.MultiplyPoint3x4(world.position);
                Vector3 canvasLocalPosition = localCircuitToCanvas * localCircuitPosition;
                ui.rectTransform.localPosition = canvasLocalPosition;
                ui.color = color;
            }
        }

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

        [SerializeField]
        internal TrackReference[] trackReferences = default;

        void Update()
        {
            Matrix4x4 worldToCircuit = CalculateWorldToCircuitMatrix();
            Matrix4x4 localCircuitToCanvas = CalculateCircuitToCanvasMatrix();

            foreach (TrackReference trackReference in trackReferences)
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