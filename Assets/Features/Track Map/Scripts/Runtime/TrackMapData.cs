using UnityEngine;

namespace Perrinn424.TrackMapSystem
{
    [CreateAssetMenu(fileName = "TrackMapData", menuName = "Perrin 424/TrackMapData")]
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
