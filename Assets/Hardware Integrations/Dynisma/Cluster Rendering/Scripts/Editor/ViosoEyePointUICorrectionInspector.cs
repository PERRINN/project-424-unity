
using UnityEngine;
using UnityEditor;


namespace Perrinn424.Editor
{

[CustomEditor(typeof(ViosoEyePointUICorrection))]
public class ViosoEyePointUICorrectionInspector : UnityEditor.Editor
	{
	public override void OnInspectorGUI()
		{
		DrawDefaultInspector();

		if (targets.Length == 1 && ((ViosoEyePointUICorrection)target).debugInfo)
			{
			GUI.enabled = false;
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("VIOSO Eye Point", EditorStyles.boldLabel);
			EditorGUILayout.Vector3Field("Position", VIOSOCamera.eyePointPos);
			EditorGUILayout.Vector3Field("Rotation", VIOSOCamera.eyePointRot);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("VIOSO Matrix", EditorStyles.boldLabel);
			DisplayMatrix(VIOSOCamera.lastMatrix);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("VIOSO Frustum", EditorStyles.boldLabel);
			EditorGUILayout.Vector2Field("Left, Top", new Vector2(VIOSOCamera.lastFrustum.left, VIOSOCamera.lastFrustum.top));
			EditorGUILayout.Vector2Field("Right, Bottom", new Vector2(VIOSOCamera.lastFrustum.right, VIOSOCamera.lastFrustum.bottom));
			EditorGUILayout.Vector2Field("Near, Far", new Vector2(VIOSOCamera.lastFrustum.zNear, VIOSOCamera.lastFrustum.zFar));
			EditorGUILayout.LabelField("Matrix:");
			DisplayMatrix(Matrix4x4.Frustum(VIOSOCamera.lastFrustum));
			GUI.enabled = true;
			}
		}


	void DisplayMatrix (Matrix4x4 m)
		{
		EditorGUILayout.Vector4Field("", m.GetRow(0));
		EditorGUILayout.Vector4Field("", m.GetRow(1));
		EditorGUILayout.Vector4Field("", m.GetRow(2));
		EditorGUILayout.Vector4Field("", m.GetRow(3));
		EditorGUILayout.Vector3Field("Position", m.GetColumn(3));
		if (m.ValidTRS())
			EditorGUILayout.Vector3Field("Rotation", m.rotation.eulerAngles);
		else
			EditorGUILayout.LabelField("Rotation", "NO VALID TRS");
		}
	}

}
