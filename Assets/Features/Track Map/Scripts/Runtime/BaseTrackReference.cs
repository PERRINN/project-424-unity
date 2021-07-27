using UnityEngine;
using UnityEngine.UI;

namespace Perrinn424.TrackMapSystem
{
    public abstract class BaseTrackReference
    {
        [SerializeField]
        protected Image ui = default;

        [SerializeField]
        protected Color color = default;

        public virtual void Init() { }
        protected virtual void Precalculate() { }
        protected abstract bool IsValid { get;}
        protected abstract Vector3 Position { get;}
        public void WorldToCanvas(Matrix4x4 worldToLocalCircuit, Matrix4x4 localCircuitToCanvas)
        {
            Precalculate();

            if (IsValid)
            {
                ui.gameObject.SetActive(true);
                Vector3 localCircuitPosition = worldToLocalCircuit.inverse.MultiplyPoint3x4(Position);
                Vector3 canvasLocalPosition = localCircuitToCanvas * localCircuitPosition;
                ui.rectTransform.localPosition = canvasLocalPosition;
                ui.color = color;
            }
            else
            {
                ui.gameObject.SetActive(false);
            }
        }
    } 
}
