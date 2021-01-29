using EdyCommonTools;
using UnityEngine;
using VehiclePhysics;

namespace Perrinn424.TrackMapSystem
{
    public class ReplayTexture
    {
        public int Resolution { get; }

        public Rect WorldCoordinates { get; private set; }
        private readonly Vector3[] positions;
        public int SamplingCount => positions.Length;

        public TextureCanvas Canvas { get; private set; }
        public Texture2D Texture => Canvas.texture;

        public ReplayTexture(int resolution, VPReplayAsset replay, float timeStep, Vector3 scale, bool doublePixel)
        {
            this.Resolution = resolution;

            replay.GetPositions(timeStep, out positions, out Rect r);

            RescalePositions(scale);

            WorldCoordinates = r;

            ResizeLocalCoordinates();
            CreateCanvas(doublePixel);
        }

        private void RescalePositions(Vector3 scale)
        {
            Matrix4x4 m = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);

            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = m.MultiplyPoint3x4(positions[i]);
            }
        }

        private void ResizeLocalCoordinates()
        {
            //making it squared
            float origin = Mathf.Min(WorldCoordinates.x, WorldCoordinates.y);
            float size = Mathf.Max(WorldCoordinates.width, WorldCoordinates.height);

            //giving some space at the margins
            float augmentedSize = size * 0.1f;
            size += augmentedSize;
            origin -= augmentedSize * 0.5f;
            WorldCoordinates = new Rect(origin, origin, size, size);
        }

        private void CreateCanvas(bool doublePixel)
        {
            Canvas = new TextureCanvas(Resolution, Resolution)
            {
                alpha = 0.0f,
                color = GColor.black,
                doublePixel = doublePixel
            };

            Canvas.Clear();
            Canvas.Save();

            Canvas.rect = WorldCoordinates;

            Canvas.alpha = 1f;
            Canvas.color = Color.white;
            
            //canvas.Line(r.x, r.y, r.x + r.width, r.y + r.height);
            for (int i = 0; i < positions.Length - 1; i++)
            {
                Vector3 pointA = positions[i];
                Vector3 pointB = positions[i + 1];
                Canvas.Line(pointA.x, pointA.z, pointB.x, pointB.z);
            }
        }
    } 
}
