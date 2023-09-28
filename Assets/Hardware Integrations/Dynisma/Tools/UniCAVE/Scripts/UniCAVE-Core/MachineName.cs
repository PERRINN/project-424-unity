using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UniCAVE
{
    [CreateAssetMenu(fileName = nameof(MachineName), menuName = nameof(UniCAVE) + "/MachineName asset", order = 3000)]
    public class MachineName : ScriptableObject
    {
        [SerializeField]
        string _name;
        /// <summary>
        /// The name of the machine.
        /// </summary>
        public string Name
        {
            get => _name.ToUpper();
            set => _name = value;
        }

        /// <summary>
        /// Returns the machine name stored in an asset if it exists. Otherwise returns the old machine name value.
        /// </summary>
        public static string GetMachineName(string deprecatedMachineName, MachineName machineNameAsset)
        {
            //prioritize the machine name stored in the asset
            if(machineNameAsset) return machineNameAsset.Name.ToUpper();

            //otherwise, return the old machine name string
            if(!string.IsNullOrEmpty(deprecatedMachineName)) return deprecatedMachineName.ToUpper();
            else return string.Empty;
        }

#if UNITY_EDITOR
        const string MACHINE_NAME_WARNING = "String-based machine names are deprecated! Please create a MachineName asset (Create->UniCAVE->MachineName).";

        public static void DrawDeprecatedMachineName(SerializedProperty machineNameString, SerializedProperty machineNameAsset, string machineNameAssetLabel = null)
        {
            //if there is a MachineName asset, the old machine name string will be hidden

            //if there is no MachineName asset and there is an old machine name value, display it with a warning
            if(machineNameAsset.objectReferenceValue == null && !string.IsNullOrEmpty(machineNameString.stringValue))
            {
                EditorGUILayout.HelpBox(MACHINE_NAME_WARNING, MessageType.Warning);

                GUI.enabled = false;
                EditorGUILayout.PropertyField(machineNameString);
                GUI.enabled = true;
            }

            //draw the MachineName asset with an optional custom display name
            GUIContent machineNameAssetGUIContent = new GUIContent(string.IsNullOrEmpty(machineNameAssetLabel) ? machineNameAsset.displayName : machineNameAssetLabel);
            EditorGUILayout.PropertyField(machineNameAsset, machineNameAssetGUIContent);
        }

        [CanEditMultipleObjects]
        [CustomEditor(typeof(MachineName), true)]
        class Editor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                serializedObject.Update();

                EditorGUILayout.LabelField("Machine Name:", EditorStyles.boldLabel);

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_name)));

                serializedObject.ApplyModifiedProperties();

                if(GUILayout.Button("Get Local Machine Name"))
                {
                    string localMachineName = System.Environment.MachineName;

                    Undo.SetCurrentGroupName("Set Machine Name");

                    foreach(MachineName mn in targets)
                    {
                        Undo.RecordObject(mn, "Set Machine Name");

                        mn.Name = localMachineName;
                        EditorUtility.SetDirty(mn);
                    }

                    Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
                }

                if(GUILayout.Button("Set Name From Filename"))
				{
                    Undo.SetCurrentGroupName("Set Machine Name");

                    foreach(MachineName mn in targets)
					{
                        Undo.RecordObject(mn, "Set Machine Name");

                        mn.Name = mn.name;
                        EditorUtility.SetDirty(mn);
                    }

                    Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
                }
            }
        }
#endif
    }
}