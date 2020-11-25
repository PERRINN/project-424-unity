using EdyCommonTools;
using UnityEngine;
using VehiclePhysics;

namespace Perrinn424.UI
{
    public class ReplayTexture
    {
        public int Resolution { get; }
        private Rect localCoordinates;
        public TextureCanvas canvas;

        private Vector3[] positions;
        public int SamplingCount => positions.Length;
        public ReplayTexture(int resolution, VPReplayAsset replay, float timeStep)
        {
            this.Resolution = resolution;

            replay.GetPositions(timeStep, out positions, out localCoordinates);

            ResizeLocalCoordinates();
            CreateCanvas();
        }

        private void ResizeLocalCoordinates()
        {
            //making it squared
            float origin = Mathf.Min(localCoordinates.x, localCoordinates.y);
            float size = Mathf.Max(localCoordinates.width, localCoordinates.height);

            //giving some space at the margins
            float augmentedSize = size * 0.1f;
            size += augmentedSize;
            origin -= augmentedSize * 0.5f;
            localCoordinates = new Rect(origin, origin, size, size);
        }

        private void CreateCanvas()
        {
            canvas = new TextureCanvas(Resolution, Resolution)
            {
                alpha = 0.0f,
                color = GColor.black
            };

            canvas.Clear();
            canvas.Save();

            //Rect r = map.rect;
            canvas.rect = localCoordinates;

            canvas.alpha = 1f;
            canvas.color = Color.white;
            
            //canvas.Line(r.x, r.y, r.x + r.width, r.y + r.height);
            for (int i = 0; i < positions.Length - 1; i++)
            {
                Vector3 pointA = positions[i];
                Vector3 pointB = positions[i + 1];
                canvas.Line(pointA.x, pointA.z, pointB.x, pointB.z);
            }
        }
    } 
}
