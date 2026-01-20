using UnityEngine;

namespace Perrinn424.TrackMapSystem
{
    [CreateAssetMenu(fileName = "TrackMapData", menuName = "Perrinn 424/TrackMapData", order = 490)]
    public class TrackMapData : ScriptableObject
    {
        public Sprite map;
        public float scale;
        public float rotation;
        public Vector2 position = new Vector2(0.5f, 0.5f);

        public TrackMap CreateTrackMap()
        {
            return new TrackMap(scale, rotation, position);
        }
    }
}
