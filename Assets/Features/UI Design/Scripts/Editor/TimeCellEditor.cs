using UnityEditor;

namespace Perrinn424.UI.Editor
{
    [CustomEditor(typeof(TimeCell))]
    public class TimeCellEditor : UnityEditor.Editor
    {

        private TimeCell timeCell;
        private float seconds;

        private void OnEnable()
        {
            timeCell = (TimeCell) target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PrefixLabel("Example");
            seconds = EditorGUILayout.FloatField("Seconds", seconds);
            EditorGUILayout.LabelField("Result",timeCell.timeFormatter.ToString(seconds));
            
            EditorGUILayout.EndVertical();
        }
    }
}
