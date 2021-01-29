using System;
using System.IO;
using EdyCommonTools.EditorTools;
using UnityEngine;
using UnityEditor;
using VehiclePhysics;

namespace Perrinn424.TrackMapSystem.Editor
{
    public class ReplayImageGeneratorWindow : EditorWindow
    {
        private VPReplayAsset replay;
        private ReplayTexture replayTexture;

        private float rate = 0.02f;
        private int resolution = 400;
        private UnityEditor.Editor replayEditor;
        private Vector2 scroll;

        private bool invertX;
        private bool invertZ;
        private bool doublePixel;

        [MenuItem("Tools/Vehicle Physics/Replay Image Generator")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            ReplayImageGeneratorWindow window = (ReplayImageGeneratorWindow)GetWindow(typeof(ReplayImageGeneratorWindow));
            window.Show();
        }

        void OnGUI()
        {
            replay = EditorGUILayout.ObjectField(replay, typeof(VPReplayAsset), false) as VPReplayAsset;

            if (replay == null)
                return;

            DrawReplayData();

            DrawSettings();

            DrawRefresh();

            DrawInfo();

            DrawSave();

            DrawReplayImage();
        }

        private void DrawReplayData()
        {
            GUI.enabled = false;
            replayEditor = UnityEditor.Editor.CreateEditor(replay);
            replayEditor.OnInspectorGUI();
            GUI.enabled = true;
        }

        private void DrawSettings()
        {
            EditorGUILayout.BeginHorizontal();
            rate = EditorGUILayout.FloatField("Rate", rate);
            EditorGUILayout.LabelField($"{rate:F2} s", $"{1f / rate:F2} Hz");
            EditorGUILayout.EndHorizontal();
            invertX = EditorGUILayout.Toggle("Invert X axis", invertX);
            invertZ = EditorGUILayout.Toggle("Invert Z axis", invertZ);
            doublePixel = EditorGUILayout.Toggle("Double Pixel", doublePixel);

            if (rate < replay.timeStep)
            {
                EditorGUILayout.HelpBox("Image rate sampling should be less than replay rate sampling", MessageType.Warning);
            }
            resolution = EditorGUILayout.IntField("Resolution", resolution);
        }

        private void DrawRefresh()
        {
            if (GUILayout.Button("Refresh") || replayTexture == null)
            {
                Vector3 scale = Vector3.one;
                scale.x = invertX ? -1f : 1f;
                scale.z = invertZ ? -1f : 1f;
                replayTexture = new ReplayTexture(resolution, replay, rate, scale, doublePixel);
            }
        }

        private void DrawInfo()
        {
            if (replayTexture != null)
            {
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("Info");
                EditorGUILayout.LabelField("Frames", replayTexture.SamplingCount.ToString());
                EditorGUILayout.RectField("Coordinates", replayTexture.WorldCoordinates);
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawReplayImage()
        {
            if (replayTexture != null)
            {
                scroll = EditorGUILayout.BeginScrollView(scroll);
                Rect graphRect = EditorGUILayout.GetControlRect(false, replayTexture.Resolution);
                TextureCanvasEditor.InspectorDraw(replayTexture.Canvas, graphRect);
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawSave()
        {
            if (GUILayout.Button("Save as Image"))
            {
                var imagePath = EditorUtility.SaveFilePanelInProject("Save Replay Image", replay.name + ".png", "png", "");

                if (imagePath.Length != 0)
                {
                    var pngData = replayTexture.Texture.EncodeToPNG();
                    if (pngData != null)
                    {
                        SaveAndImport(imagePath, pngData);
                        SelectInProject(imagePath);
                    }
                }
            }
        }

        private static void SaveAndImport(string imagePath, byte[] pngData)
        {
            File.WriteAllBytes(imagePath, pngData);
            AssetDatabase.ImportAsset(imagePath);
            TextureImporter textureImporter = AssetImporter.GetAtPath(imagePath) as TextureImporter;
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.SaveAndReimport();
        }

        private static void SelectInProject(string imagePath)
        {
            var obj = AssetDatabase.LoadAssetAtPath(imagePath, typeof(Texture2D));
            Selection.activeObject = obj;
            EditorUtility.FocusProjectWindow();
        }
    } 
}