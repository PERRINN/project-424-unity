using System;
using UnityEngine;
using UnityEngine.UI;

namespace Perrinn424.TrackMapSystem
{
    [Serializable]
    public class TransformTrackReference : BaseTrackReference
    {
        [SerializeField]
        private Transform world = default;

        public TransformTrackReference(Transform world, Image ui, Color color)
        {
            this.world = world;
            this.ui = ui;
            this.color = color;
        }

        protected override bool IsValid => true;
        public override Vector3 Position => world.position;

    } 
}
