using System;
using UnityEngine;
using UnityEngine.UI;

namespace Perrinn424.TrackMapSystem
{
    [Serializable]
    public class TransformTrackReference
    {
        [SerializeField]
        private Transform world = default;
        [SerializeField]
        private Image ui = default;

        [SerializeField]
        private Color color = default;

        public TransformTrackReference(Transform world, Image ui, Color color)
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
}
