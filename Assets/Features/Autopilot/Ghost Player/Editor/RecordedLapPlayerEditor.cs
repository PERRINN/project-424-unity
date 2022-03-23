using Perrinn424.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RecordedLapPlayer))]
public class RecordedLapPlayerEditor : Editor
{
    private RecordedLapPlayer player;
    SerializedProperty lap;
    SerializedProperty reproductionSpeed;
    SerializedProperty reproductionType;
    private TimeFormatter timeFormatter;

    private void OnEnable()
    {
        player = (RecordedLapPlayer)target;
        lap = serializedObject.FindProperty("lap");
        reproductionSpeed = serializedObject.FindProperty("reproductionSpeed");
        reproductionType = serializedObject.FindProperty("reproductionType");
        timeFormatter = new TimeFormatter(TimeFormatter.Mode.MinutesAndSeconds, @"m\:ss\.fff", @"m\:ss\.fff");

    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(lap);
        EditorGUILayout.PropertyField(reproductionType);

        EditorGUILayout.Slider(reproductionSpeed, 0f, 3f, $"{reproductionSpeed.floatValue}x");

        string duration = $"{timeFormatter.ToString(player.PlayingTime)}/{timeFormatter.ToString(player.TotalTime)}";
        EditorGUILayout.LabelField(duration);
        
        EditorGUI.BeginChangeCheck();
        float time = EditorGUILayout.Slider(player.PlayingTime, 0f, player.TotalTime);
        if (EditorGUI.EndChangeCheck())
        {
            player.Stop();
            player.SetPlayingTime(time);
        }



        if (GUILayout.Button("Play"))
        {
            player.Play();
        }
        if (GUILayout.Button("Pause"))
        {
            player.Pause();
        }
        if (GUILayout.Button("Stop"))
        {
            player.Stop();
        }
        if (GUILayout.Button("Restart"))
        {
            player.Restart();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
