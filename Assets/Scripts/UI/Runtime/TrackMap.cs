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

            public void WorldToCanvas(Matrix4x4 worldToLocalCircuit, Matrix4x4 localCircuitToCanvas)
            {
                Vector3 localCircuitPosition = worldToLocalCircuit.inverse.MultiplyPoint3x4(world.position);
                Vector3 canvasLocalPosition = localCircuitToCanvas * localCircuitPosition;
                ui.rectTransform.localPosition = canvasLocalPosition;
                ui.color = color;
            }
        }

        //[Header("World references")]
        //[SerializeField]
        //private Transform vehicle = default;
        //[SerializeField]
        //private Transform startLine = default;

        //[Header("UI references")]
        //[SerializeField]
        //private RectTransform vehicleReference = default;
        //[SerializeField]
        //private RectTransform startLineReference = default;


        [SerializeField]
        private TrackReference[] trackReferences = default;

        public Vector3 center = Vector3.zero;
        public Vector3 size = Vector3.one;

        //public Vector2 flip = Vector2.zero;
        public Vector2 scaleM = Vector2.one;
        // Update is called once per frame

        public Vector3 localPosition;
        void Update()
        {
            //if(IsNullAnyReference())
            //    return;


            //Matrix4x4 worldToCircuit = Matrix4x4.Translate(center);
            //localPosition = worldToCircuit.inverse.MultiplyPoint3x4(startLine.position);


            //RectTransform rectTransform = (RectTransform)transform;

            //float xScale = rectTransform.rect.width / size.x;
            //xScale *= scaleM.x;
            //float zScale = rectTransform.rect.height / size.z;
            //zScale *= scaleM.y;
            //Vector3 scale = new Vector3(xScale, 0f, zScale);

            //Quaternion q = Quaternion.AngleAxis(90f, Vector3.right);
            //Matrix4x4 worldToCanvas = Matrix4x4.TRS(Vector3.zero, q, scale);
            //Vector3 canvasLocalPosition = worldToCanvas * localPosition;
            //startLineReference.localPosition = canvasLocalPosition;

            Matrix4x4 worldToCircuit = Matrix4x4.Translate(center);

            RectTransform rectTransform = (RectTransform)transform;
            float xScale = rectTransform.rect.width / size.x;
            xScale *= scaleM.x;
            float zScale = rectTransform.rect.height / size.z;
            zScale *= scaleM.y;
            Vector3 scale = new Vector3(xScale, 0f, zScale);

            Quaternion q = Quaternion.AngleAxis(90f, Vector3.right);
            Matrix4x4 localCircuitToCanvas = Matrix4x4.TRS(Vector3.zero, q, scale);


            foreach (TrackReference trackReference in trackReferences)
            {
                trackReference.WorldToCanvas(worldToCircuit, localCircuitToCanvas);
            }
            //WorldToCanvas(startLine, startLineReference, worldToCircuit, localCircuitToCanvas);
            //WorldToCanvas(vehicle, vehicleReference, worldToCircuit, localCircuitToCanvas);
        }

        private void WorldToCanvas(Transform worldObject, RectTransform uiObject, Matrix4x4 worldToLocalCircuit, Matrix4x4 localCircuitToCanvas)
        {
            Vector3 localCircuitPosition = worldToLocalCircuit.inverse.MultiplyPoint3x4(worldObject.position);
            Vector3 canvasLocalPosition = localCircuitToCanvas * localCircuitPosition;
            uiObject.localPosition = canvasLocalPosition;
        }

        //private bool IsNullAnyReference()
        //{
        //    return vehicle == null || startLine == null || vehicleReference == null || startLineReference == null;
        //}

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
