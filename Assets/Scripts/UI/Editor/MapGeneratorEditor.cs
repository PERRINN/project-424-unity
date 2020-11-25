using System.IO;
using EdyCommonTools;
using EdyCommonTools.EditorTools;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    TextureCanvas m_graph = null;
    const int m_graphWidth = 400;
    const int m_graphHeight = 400;

    private MapGenerator map;

    private void OnEnable()
    {
        map  = (MapGenerator)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();



        Rect graphRect = EditorGUILayout.GetControlRect(false, m_graphHeight);

        GUIDrawGraph(ref m_graph, graphRect, m_graphWidth, m_graphHeight);

        if (GUILayout.Button("Save"))
        {
            //then Save To Disk as PNG
            byte[] bytes = map.mapGeneratorXxx.canvas.texture.EncodeToPNG();

            var relativePath = "/SaveImages/";
            var dirPath = Application.dataPath + relativePath;
            var fileName = "Image" + ".png";
            var assetPath = dirPath + fileName;
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            File.WriteAllBytes(assetPath, bytes);
            string unityPath = "Assets" + relativePath + fileName;
            AssetDatabase.ImportAsset(unityPath, ImportAssetOptions.Default);
            TextureImporter textureImporter = AssetImporter.GetAtPath(unityPath) as TextureImporter;
            textureImporter.textureType = TextureImporterType.Sprite;
        }
    }

    void GUIDrawGraph(ref TextureCanvas graph, Rect position, int graphWidth, int graphHeight)
    {
        if (graph == null || CommonEditorTools.GUIChanged())
        {
            if (graph == null)
            {
                graph = new TextureCanvas(graphWidth, graphHeight);
                graph.alpha = 0.0f;
                graph.color = GColor.black;
                graph.Clear();
                graph.Save();
            }
            else
            {
                graph.Restore();
            }
        }

        //float x = -1500;
        //float y = x;
        //float size = 2500f;
        //graph.rect = new Rect(x, y, size, size);
        Rect r = map.rect;
        graph.rect = r;

        graph.alpha = 1f;
        graph.color = Color.white;
        graph.Line(r.x, r.y, r.x+r.width, r.y+r.height);
        Vector3[] points = map.points;
        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector3 pointA = points[i];
            Vector3 pointB = points[i + 1];
            graph.Line(pointA.x, pointA.z, pointB.x, pointB.z);
        }

        //TextureCanvasEditor.InspectorDraw(graph, position);
        TextureCanvasEditor.InspectorDraw(map.mapGeneratorXxx.canvas, position);
    }
}
