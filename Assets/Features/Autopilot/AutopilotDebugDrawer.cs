using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    public class AutopilotDebugDrawer
    {
        private Vector3 targetPosition;
        private Vector3 lateralCorrectionPosition;
        private Vector3 lateralCorrectorForce;

        public void Set(Vector3 targetPosition, Vector3 lateralCorrectionPosition, Vector3 lateralCorrectorForce)
        {
            this.targetPosition = targetPosition;
            this.lateralCorrectionPosition = lateralCorrectionPosition;
            this.lateralCorrectorForce = lateralCorrectorForce;
        }

        public void Draw()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(targetPosition, 0.05f * 10);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(lateralCorrectionPosition, lateralCorrectorForce);
        }
    } 
}
