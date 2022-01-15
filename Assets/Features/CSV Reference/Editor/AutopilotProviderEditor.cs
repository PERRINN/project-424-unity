using UnityEditor;
namespace Perrinn424
{
    [CustomEditor(typeof(AutopilotProvider))]
    public class AutopilotProviderEditor : UnityEditor.Editor
    {
        UnityEditor.Editor replayAssetEditor;
        private AutopilotProvider autopilotProvider;
        void OnEnable()
        {
            autopilotProvider = (AutopilotProvider)(target);
            replayAssetEditor = CreateEditor(autopilotProvider.GetReplayAsset());

        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            DrawDefaultInspector();

            if (EditorGUI.EndChangeCheck())
            {
                UnityEditor.Editor  tmpEditor = CreateEditor(autopilotProvider.GetReplayAsset());
                if (replayAssetEditor != null)
                {
                    DestroyImmediate(replayAssetEditor);
                }

                replayAssetEditor = tmpEditor;
            }

            if (replayAssetEditor != null)
            {
                replayAssetEditor.OnInspectorGUI();
            }
        }
    } 
}
