//MIT License
//Copyright 2016-Present 
//Ross Tredinnick
//Kevin Ponto
//Benny Wysong-Grass
//University of Wisconsin - Madison Virtual Environments Group
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
//to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, 
//sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
//INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine;

namespace UniCAVE
{
    public class Make3x2Cubemap
    {
        Mesh mesh;

        public Vector3[] vertices;
        public int[] tri;
        public Vector2[] uv;

        public Make3x2Cubemap()
        {
            vertices = new Vector3[4];
            tri = new int[6];
            uv = new Vector2[4];
        }

        public void MakeFace(Vector3 min, Vector3 axis1, Vector3 axis2, Vector2 uvOffset)
        {
            vertices[0] = min;
            vertices[1] = min + 2f * axis2;
            vertices[2] = min + 2f * axis1;
            vertices[3] = min + 2f * (axis1 + axis2);

            tri[0] = 0;
            tri[1] = 1;
            tri[2] = 2;

            tri[3] = 3;
            tri[4] = 2;
            tri[5] = 1;


            uv[0] = uvOffset;
            uv[2] = uvOffset + new Vector2(0.3333f, 0);
            uv[1] = uvOffset + new Vector2(0, 0.5f);
            uv[3] = uvOffset + new Vector2(0.3333f, 0.5f);
        }



#if UNITY_EDITOR
        [MenuItem("UniCAVE/Make3x2Cubemap")]
        static void MakeCubeMap()
        {

            GameObject cube = GameObject.Find("360Cube");
            if(!cube)
            {
                cube = new GameObject();
                cube.transform.localScale = new Vector3(10, 10, 10);
            }
            cube.name = "360Cube";
            cube.AddComponent<MeshFilter>();
            cube.AddComponent<MeshRenderer>();
            Mesh mesh = cube.GetComponent<MeshFilter>().sharedMesh = new Mesh();


            mesh.name = "360mesh";



            const int numFaces = 6;
            Make3x2Cubemap[] face = new Make3x2Cubemap[numFaces];

            for(int i = 0; i < numFaces; i++)
                face[i] = new Make3x2Cubemap();


            face[0].MakeFace(new Vector3(-1, -1, 1), new Vector3(0, 0, -1), new Vector3(1, 0, 0), new Vector2(0, 0));
            face[1].MakeFace(new Vector3(-1, -1, -1), new Vector3(0, 1, 0), new Vector3(1, 0, 0), new Vector2(0.333f, 0));
            face[2].MakeFace(new Vector3(-1, 1, -1), new Vector3(0, 0, 1), new Vector3(1, 0, 0), new Vector2(0.666f, 0));
            face[3].MakeFace(new Vector3(-1, -1, -1), new Vector3(0, 0, 1), new Vector3(0, 1, 0), new Vector2(0, 0.5f));
            face[4].MakeFace(new Vector3(1, -1, 1), new Vector3(0, 0, -1), new Vector3(0, 1, 0), new Vector2(0.666f, 0.5f));
            face[5].MakeFace(new Vector3(-1, -1, 1), new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector2(0.333f, 0.5f));


            Vector3[] vertices = new Vector3[4 * numFaces];
            Vector2[] uv = new Vector2[4 * numFaces];
            int[] tri = new int[6 * numFaces];

            for(int n = 0; n < numFaces; n++)
            {
                for(int i = 0; i < 4; i++)
                    vertices[i + 4 * n] = face[n].vertices[i];

                for(int i = 0; i < 4; i++)
                    uv[i + 4 * n] = face[n].uv[i];

                for(int i = 0; i < 6; i++)
                    tri[i + 6 * n] = (4 * n) + face[n].tri[i];

            }

            mesh.vertices = vertices;
            mesh.triangles = tri;
            mesh.uv = uv;

            /*
            RenderTexture rt;
            rt = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32);
            rt.name = "360renderTexture";
            rt.Create();

            Debug.Log(rt.name);

            var assetname = "Assets/" + rt.name + ".asset";


            AssetDatabase.CreateAsset(rt, assetname);
            AssetDatabase.ImportAsset(assetname, ImportAssetOptions.ForceUpdate);


            MeshRenderer meshRenderer = cube.GetComponent<MeshRenderer>();

            rt = (RenderTexture)AssetDatabase.LoadAssetAtPath(assetname, typeof(RenderTexture));


            meshRenderer.sharedMaterial.mainTexture = rt;


            //renderTexture.name = "360renderTexture";

          //  cube.AddComponent<Texture>();
    */
        }
#endif
    }
}