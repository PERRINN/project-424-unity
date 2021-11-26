
using UnityEngine;
using UnityEditor;
using EdyCommonTools;


namespace Perrinn424.Editor
{
    public class Perrinn424UnderfloorInspector
    {
        // No need to override the inspector for now.
        // Using this for drawing gizmos only.

        [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.Active)]
        static void DrawSceneGizmos(Perrinn424Underfloor src, GizmoType gizmoType)
        {
            foreach (var cp in src.contactPoints)
                DrawContactPoint(cp);
        }

        static void DrawContactPoint(Perrinn424Underfloor.ContactPoint cp)
        {
            if (cp.pointBase == null) return;

            Vector3 up = cp.pointBase.up;
            Vector3 pointBase = cp.pointBase.position;
            Vector3 origin = pointBase + up * cp.detectionLength;
            Vector3 spring = pointBase + up * cp.limitContactDepth;

            Gizmos.color = GColor.green;
            Gizmos.DrawLine(pointBase, spring);
            Gizmos.color = GColor.Alpha(GColor.red, 0.25f);
            Gizmos.DrawLine(origin, spring);

            Gizmos.color = Color.gray;
            DebugUtility.CrossMarkGizmo(pointBase, cp.pointBase.forward, cp.pointBase.right, Vector3.zero, 0.1f);
        }
    }
}

