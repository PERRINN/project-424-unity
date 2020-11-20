using System.Collections;
using System.Collections.Generic;
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
    }

    void GUIDrawGraph(ref TextureCanvas graph, Rect position, int graphWidth, int graphHeight)
    {
        if (graph == null || CommonEditorTools.GUIChanged())
        {
            if (graph == null)
            {
                graph = new TextureCanvas(graphWidth, graphHeight);
                graph.alpha = 0.75f;
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


        graph.color = Color.white;
        graph.Line(r.x, r.y, r.x+r.width, r.y+r.height);
        Vector3[] points = map.points;
        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector3 pointA = points[i];
            Vector3 pointB = points[i + 1];
            graph.Line(pointA.x, pointA.z, pointB.x, pointB.z);
        }

        TextureCanvasEditor.InspectorDraw(graph, position);
    }
}
