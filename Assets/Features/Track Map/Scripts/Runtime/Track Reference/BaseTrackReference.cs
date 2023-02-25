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
        public abstract Vector3 Position { get;}

        public void SetLocalPosition(Vector3 localPosition)
        {
            Precalculate();

            if (IsValid)
            {
                ui.gameObject.SetActive(true);
                ui.rectTransform.localPosition = localPosition;
                ui.color = color;
            }
            else
            {
                ui.gameObject.SetActive(false);
            }
        }
    } 
}
