using UnityEditor;
using UnityEngine;

namespace Perrinn424.LiveLink
{
    [CustomEditor(typeof(UnityLiveLink))]
    public sealed class UnityLiveLinkEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var bridge = (UnityLiveLink)target;
            EditorGUILayout.LabelField("Connection", bridge.Status);
            if (Application.isPlaying && !string.IsNullOrEmpty(bridge.pairingToken))
            {
                EditorGUILayout.LabelField("Session pairing token");
                EditorGUILayout.SelectableLabel(bridge.pairingToken, GUILayout.Height(20));
                if (GUILayout.Button("Copy pairing token")) EditorGUIUtility.systemCopyBuffer = bridge.pairingToken;
            }
            if (!string.IsNullOrEmpty(bridge.RecordingPath)) EditorGUILayout.SelectableLabel("Recording: " + bridge.RecordingPath, GUILayout.Height(40));
            if (!string.IsNullOrEmpty(bridge.NetworkError)) EditorGUILayout.HelpBox(bridge.NetworkError, MessageType.Warning);
            EditorGUILayout.HelpBox("Open client-web /prototypes/unity-live-link, connect and paste this session's token. Both front springs are changed together. Values are restored when this component stops.", MessageType.Info);
            if (Application.isPlaying) Repaint();
        }

        [MenuItem("PERRINN/Enable Unity Live Link in Play Mode")]
        static void EnableInPlayMode()
        {
            if (!Application.isPlaying) { Debug.LogWarning("Start Play Mode first, then enable Unity Live Link."); return; }
            var selected = Selection.activeGameObject == null ? null : Selection.activeGameObject.GetComponentInParent<Perrinn424CarController>();
            var cars = Object.FindObjectsOfType<Perrinn424CarController>();
            var car = selected != null ? selected : cars.Length == 1 ? cars[0] : null;
            if (car == null) { Debug.LogWarning("Select the intended car: Unity Live Link requires one Perrinn424CarController."); return; }
            var bridge = car.GetComponent<UnityLiveLink>();
            if (bridge == null) bridge = car.gameObject.AddComponent<UnityLiveLink>();
            bridge.enabled = true;
            Selection.activeGameObject = car.gameObject;
        }
    }
}
