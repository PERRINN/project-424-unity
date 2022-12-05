using Perrinn424.Utilities;
using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RecordedLapPlayer))]
public class RecordedLapPlayerEditor : Editor
{
    private RecordedLapPlayer player;
    SerializedProperty lap;
    SerializedProperty reproductionSpeed;
    SerializedProperty reproductionType;
    SerializedProperty startTime;
    SerializedProperty playOnStart;
    SerializedProperty showGui;
    SerializedProperty guiPosition;
    private TimeFormatter timeFormatter;

    private void OnEnable()
    {
        player = (RecordedLapPlayer)target;
        lap = serializedObject.FindProperty("lap");
        reproductionSpeed = serializedObject.FindProperty("reproductionSpeed");
        reproductionType = serializedObject.FindProperty("reproductionType");
        startTime = serializedObject.FindProperty("startTime");
        playOnStart = serializedObject.FindProperty("playOnStart");
        showGui = serializedObject.FindProperty("showGui");
        guiPosition = serializedObject.FindProperty("guiPosition");
        timeFormatter = new TimeFormatter(TimeFormatter.Mode.MinutesAndSeconds, @"m\:ss\.fff", @"m\:ss\.fff");

    }

    public override void OnInspectorGUI()
    {
        DrawControls();
        DrawInGameGUIOptions();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawControls()
    {
        EditorGUILayout.PropertyField(lap);
        EditorGUILayout.PropertyField(reproductionType);

        EditorGUILayout.Slider(reproductionSpeed, 0f, 3f, $"{reproductionSpeed.floatValue}x");
        EditorGUILayout.PropertyField(playOnStart);

        string duration = $"{timeFormatter.ToString(player.playingTime)}/{timeFormatter.ToString(player.TotalTime)}";
        EditorGUILayout.LabelField(duration);

        EditorGUI.BeginChangeCheck();
        float time = EditorGUILayout.Slider(player.playingTime, 0f, player.TotalTime);
        if (EditorGUI.EndChangeCheck())
        {
            player.Stop();
            player.SetPlayingTime(time);
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("<<<"))
        {
            player.SetPlayingTime(time - 2f);
        }
        if (GUILayout.Button("<<"))
        {
            player.SetPlayingTime(time - 0.5f);
        }
        if (GUILayout.Button("<"))
        {
            player.SetPlayingTime(time - 0.1f);
        }
        if (GUILayout.Button(">"))
        {
            player.SetPlayingTime(time + 0.1f);
        }
        if (GUILayout.Button(">>"))
        {
            player.SetPlayingTime(time + 0.5f);
        }
        if (GUILayout.Button(">>>"))
        {
            player.SetPlayingTime(time + 2f);
        }
        GUILayout.EndHorizontal();


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
    }

    private void DrawInGameGUIOptions()
    {
        EditorGUILayout.PropertyField(showGui, new GUIContent("Show Controls InGame"));
        if (showGui.boolValue)
        {
            EditorGUILayout.PropertyField(guiPosition);
        }
    }
}
