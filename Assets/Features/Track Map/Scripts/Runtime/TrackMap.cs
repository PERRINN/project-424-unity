using UnityEngine;

namespace Perrinn424.TrackMapSystem
{
    public class TrackMap
    {
        public float scale;
        public float rotation;
        public float position;

        public Matrix4x4 TransformationMatrix { get; private set; }

        public Vector3 ScaleTransformationMatrix { get; private set; }
        public Quaternion RotationTransformationMatrix { get; private set; }
        public Vector3 TranslationTransformationMatrix { get; private set; }

        public TrackMap(float scale, float rotation, float position)
        {
            this.scale = scale;
            this.rotation = rotation;
            this.position = position;
        }


        /// <summary>
        /// Creates a transformation matrix that translates from world position to canvas position
        /// </summary>
        /// <remarks>
        /// First the position is scaled. Then, is rotated from the plane XZ (world plane), to the plane XY, which is the canvas plane
        /// Finally, it is translated in local coordinates (pixels), to match exacly the image
        /// </remarks>
        /// <param name="rectTransform">The rect transfrom containing the track image</param>
        public void CalculateTRS(RectTransform rectTransform)
        {
            Rect rect = rectTransform.rect;
            Vector2 pivot = rectTransform.pivot;

            // Scale takes into account the rectTransform dimensions
            ScaleTransformationMatrix = new Vector3(rect.width, 0, rect.height) * scale;

            // Rotation from XZ plane to XY plane =>Quaternion.AngleAxis(-90f, Vector3.right);
            // Rotation in XY plane => Quaternion.AngleAxis(rotation, Vector3.forward)
            RotationTransformationMatrix = Quaternion.AngleAxis(rotation, Vector3.forward) * Quaternion.AngleAxis(-90f, Vector3.right);

            // Translation in local coordinates. Position is [0-1]. Pivot translation is applied also
            TranslationTransformationMatrix = new Vector3((position - pivot.x) * rect.width, (position - pivot.y) * rect.height);

            TransformationMatrix = Matrix4x4.TRS(TranslationTransformationMatrix, RotationTransformationMatrix, ScaleTransformationMatrix);
        }

        public Vector3 FromWorldPositionToLocalRectTransformPosition(Vector3 worldPosition)
        {
            return TransformationMatrix.MultiplyPoint3x4(worldPosition);
        }
    } 
}
