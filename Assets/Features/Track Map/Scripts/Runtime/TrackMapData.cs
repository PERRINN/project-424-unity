using UnityEngine;

namespace Perrinn424.TrackMapSystem
{
    [CreateAssetMenu(fileName = "TrackMapData", menuName = "Perrinn 424/TrackMapData", order = 490)]
    public class TrackMapData : ScriptableObject
    {
        public Sprite map;
        public float scale;
        public float rotation;
        [Range(0, 1f)]
        public float position;

        public TrackMap CreateTrackMap()
        {
            return new TrackMap(scale, rotation, position);
        }
    }
}
