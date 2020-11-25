using System.IO;
using EdyCommonTools.EditorTools;
using UnityEngine;
using UnityEditor;
using VehiclePhysics;

namespace Perrinn424.UI.Editor
{
    public class MapImageGeneratorWindow : EditorWindow
    {
        string myString = "Hello World";
        bool groupEnabled;
        bool myBool = true;
        float myFloat = 1.23f;
        private VPReplayAsset replay;
        public MapGeneratorXXX mapGeneratorXxx;


        // Add menu named "My Window" to the Window menu
        [MenuItem("Tools/Vehicle Physics/Map Image Generator")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            MapImageGeneratorWindow window = (MapImageGeneratorWindow)EditorWindow.GetWindow(typeof(MapImageGeneratorWindow));
            window.Show();
        }

        void OnGUI()
        {
            GUILayout.Label("Base Settings", EditorStyles.boldLabel);
            myString = EditorGUILayout.TextField("Text Field", myString);

            groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
            myBool = EditorGUILayout.Toggle("Toggle", myBool);
            myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
            EditorGUILayout.EndToggleGroup();
            replay = EditorGUILayout.ObjectField(replay, typeof(VPReplayAsset), false) as VPReplayAsset;

            var editor = UnityEditor.Editor.CreateEditor(replay);
            editor.OnInspectorGUI();
            mapGeneratorXxx = new MapGeneratorXXX(400, replay, 0.02f);

            Rect graphRect = EditorGUILayout.GetControlRect(false, 400f);
            TextureCanvasEditor.InspectorDraw(mapGeneratorXxx.canvas, graphRect);

            if (GUILayout.Button("Save"))
            {
                //then Save To Disk as PNG
                //byte[] bytes = mapGeneratorXxx.canvas.texture.EncodeToPNG();



                var relativePath = "/SaveImages/";
                //var dirPath = Application.dataPath + relativePath;
                //var fileName = "Image" + ".png";
                //var assetPath = dirPath + fileName;
                //if (!Directory.Exists(dirPath))
                //{
                //    Directory.CreateDirectory(dirPath);
                //}
                //File.WriteAllBytes(assetPath, bytes);
                string unityPath = "Assets" + relativePath;
                //string unityFullPath = unityPath + fileName;
                //AssetDatabase.ImportAsset(unityPath, ImportAssetOptions.Default);
                //TextureImporter textureImporter = AssetImporter.GetAtPath(unityPath) as TextureImporter;
                //textureImporter.textureType = TextureImporterType.Sprite;

                var path = EditorUtility.SaveFilePanelInProject(
                    "Save Map", replay.name + ".png", "png", "xxxx", unityPath);

                if (path.Length != 0)
                {
                    var pngData = mapGeneratorXxx.canvas.texture.EncodeToPNG();
                    if (pngData != null)
                        File.WriteAllBytes(path, pngData);
                    //unityFullPath = TransformAbsolutePathToUnityPath(path);
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.Default);
                    TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                    textureImporter.textureType = TextureImporterType.Sprite;
                }
            }
        }

        //private string TransformAbsolutePathToUnityPath(string absolutepath)
        //{
        //    return  "Assets" + absolutepath.Substring(Application.dataPath.Length);
        //}
    } 
}