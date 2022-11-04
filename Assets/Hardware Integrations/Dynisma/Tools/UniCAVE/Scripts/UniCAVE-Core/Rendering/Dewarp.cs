using System;
using UnityEngine;

namespace UniCAVE
{
    /// <summary>
    /// Dewarp is responsible for generating the dewarp meshes 
    /// used for warp calibration.
    /// 
    /// Author: Christoffer A Træen
    /// </summary>
    public class Dewarp
    {
        /// <summary>
        /// Holds the positions for the Dewarp mesh
        /// </summary>
        [Serializable]
        public class DewarpMeshPosition
        {

            [Header("Mesh calibrated vertices (do not add extra verts):")]
            public Vector3[] verts;

            /// <summary>
            /// Holds generated vertecies: filled if verts are empty
            /// </summary>
            public Vector3[] generatedVerts;

        };

        /// <summary>
        /// The vertices size on the x axis
        /// </summary>
        public int xSize { get; private set; } = 7;

        /// <summary>
        /// The vertices size on the y axis
        /// </summary>
        public int ySize { get; private set; } = 7;

        /// <summary>
        /// The game object name for the dewarp mesh
        /// </summary>
        private readonly string objectName = "Dewarp Mesh For:";

        /// <summary>
        /// Holds the gameobject reference for the dewarp mesh
        /// </summary>
        private GameObject dewarpObject;

        /// <summary>
        /// Holds the reference to the dewarp mesh
        /// </summary>
        private Mesh warpMesh;

        /// <summary>
        /// Holds the reference to the dewarp mesh filter
        /// </summary>
        private MeshFilter warpMeshFilter;

        /// <summary>
        /// Render materioal
        /// </summary>
        private Material renderMaterial;

        private DewarpMeshPosition _calibratedVerticesPositions;

        /// <summary>
        /// Holds the calibrated dewarp positions
        /// </summary>
        public DewarpMeshPosition CalibratedVerticesPositions
        {
            get { return this._calibratedVerticesPositions; }
            private set
            {
                if(value == null)
                {
                    throw new ArgumentException("Vertices positions object cant be null");
                }
                this._calibratedVerticesPositions = value;
            }
        }

        private Material _postProcessMaterial;

        /// <summary>
        /// Holds the Post process material for the mesh.
        /// </summary>
        /// <value></value>
        private Material PostProcessMaterial
        {
            get { return this._postProcessMaterial; }
            set
            {
                if(value == null)
                {
                    throw new ArgumentException("Missing postprocess material");
                }
                this._postProcessMaterial = value;
            }
        }

        private RenderTexture _cameraTexture;

        /// <summary>
        /// Holds the Render texture for the render camera
        /// </summary>
        /// <value></value>
        private RenderTexture CameraTexture
        {
            get { return this._cameraTexture; }
            set
            {
                if(value == null)
                {
                    throw new ArgumentException("Missing camera texture");
                }
                this._cameraTexture = value;
            }
        }

        /// <summary>
        /// Creates a new dewarp mesh game object, and sets the nessecary dependncies for generation of the mesh.
        /// 
        /// /// The dewarp mesh takes a post process material, Dewarp mesh verticies positions and a render texture.
        /// The Post process material can be the UniCave postprocess material which enables edge blending and debugging.
        /// The DewarpMeshPositions is positions of imported verticies from config. So it can generate a mesh based
        /// on your loaded mesh calibration.
        /// The render texture is for the camera displaying this dewarp mesh.
        /// 
        /// </summary>
        /// <param name="name">the name of the gameobject</param>
        /// <param name="postProcessMaterial">post process material</param>
        /// <param name="vertices">positions of verticies from calibartion</param>
        /// <param name="cameraTexture">render texture for the camera</param>
        public Dewarp(string name, Material postProcessMaterial, DewarpMeshPosition verticesPositions, RenderTexture cameraTexture)
        {

            dewarpObject = new GameObject(objectName + name);

            this.PostProcessMaterial = postProcessMaterial;
            this.CalibratedVerticesPositions = verticesPositions;
            this.CameraTexture = cameraTexture;
            this.GenerateMesh();
            this.GenerateAndAssignMaterials(name);
        }

        /// <summary>
        /// Sets the resolution of the warp mesh.
        /// The total resolution will be x*y
        /// </summary>
        /// <param name="x">vertices on the x axis</param>
        /// <param name="y">vertices on the y axis</param>
        public void SetMeshResolution(int x = 2, int y = 2)
        {
            if(x < 2) throw new ArgumentException("X must atleast have 2 vertices");
            if(y < 2) throw new ArgumentException("Y must atleast have 2 vertices");
            this.xSize = x;
            this.ySize = y;
        }

        /// <summary>
        /// Generates the dewarp mesh and assign the .
        /// </summary>
        private void GenerateMesh()
        {
            this.warpMeshFilter = dewarpObject.AddComponent<MeshFilter>();
            this.warpMeshFilter.mesh = Generate();
        }

        /// <summary>
        /// Creates the materials and textures for the dewarp mesh
        /// </summary>
        private void GenerateAndAssignMaterials(string nameForMaterial)
        {

            this.renderMaterial = new Material(this._postProcessMaterial);
            this.renderMaterial.name = $"Material for {nameForMaterial}";

            this.renderMaterial.mainTexture = this._cameraTexture;
            this.dewarpObject.AddComponent<MeshRenderer>().material = this.renderMaterial;
        }

        /// <summary>
        /// Sets the layer of where the gameobject should render.
        /// </summary>
        /// <param name="layer">the layer to render on</param>
        public void SetPostprocessLayer(int layer)
        {
            if(!this.dewarpObject) throw new NullReferenceException("Cant set layer, game object is null");
            this.dewarpObject.layer = layer;
        }

        /// <summary>
        /// Generates the dewarp mesh.
        /// The mesh has a total of <c>xSize*ySize</c> vertices.
        /// Normals, tangtents, UVs and triangles are automaticaly generated.
        /// 
        /// Vertices starts from bottom left.
        /// </summary>
        /// <returns>returns the generated mesh</returns>
        private Mesh Generate()
        {
            this.warpMesh = new Mesh();
            this.warpMesh.name = "Warp mesh";

            this._calibratedVerticesPositions.generatedVerts = new Vector3[(this.xSize + 1) * (this.ySize + 1)];
            Vector2[] uv = new Vector2[this._calibratedVerticesPositions.generatedVerts.Length];
            Vector4[] tangents = new Vector4[this._calibratedVerticesPositions.generatedVerts.Length];
            Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);

            decimal xx = xSize;
            decimal yy = ySize;

            decimal ymodifier = (2 / yy);
            decimal lastY = -1;

            decimal xmodifier = (2 / xx);
            decimal lastX = -1;

            // Set verices positions and generate uvs and tangents
            for(int i = 0, y = 0; y <= ySize; y++)
            {
                for(int x = 0; x <= xSize; x++, i++)
                {
                    if(this._calibratedVerticesPositions.verts.Length == this._calibratedVerticesPositions.generatedVerts.Length)
                    {
                        this._calibratedVerticesPositions.generatedVerts[i] = new Vector3((float)this._calibratedVerticesPositions.verts[i].x, (float)this._calibratedVerticesPositions.verts[i].y);
                    }
                    else
                    {
                        this._calibratedVerticesPositions.generatedVerts[i] = new Vector3((float)lastX, (float)lastY);
                    }
                    uv[i] = new Vector2((float)x / xSize, (float)y / ySize);
                    tangents[i] = tangent;
                    lastX += (decimal)xmodifier;

                }
                lastY += (decimal)ymodifier;
                lastX = -1;
            }

            // Generates mesh triangles
            int[] triangles = new int[xSize * ySize * 6];
            for(int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++)
            {
                for(int x = 0; x < xSize; x++, ti += 6, vi++)
                {
                    triangles[ti] = vi;
                    triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                    triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
                    triangles[ti + 5] = vi + xSize + 2;
                }
            }
            this.warpMesh.vertices = this._calibratedVerticesPositions.generatedVerts;
            this.warpMesh.triangles = triangles;
            this.warpMesh.uv = uv;
            this.warpMesh.tangents = tangents;
            this.warpMesh.RecalculateNormals();
            this.warpMesh.RecalculateTangents();
            if(this._calibratedVerticesPositions.verts.Length == 0)
            {
                this._calibratedVerticesPositions.verts = this._calibratedVerticesPositions.generatedVerts;
            }

            return this.warpMesh;

        }

        /// <summary>
        /// Returns the GameObject for the dewarp mesh
        /// </summary>
        /// <returns>GameObject for the dewarp mesh</returns>
        public GameObject GetDewarpGameObject()
        {
            return this.dewarpObject;
        }

        /// <summary>
        /// Returns the Dewarp mesh
        /// </summary>
        /// <returns>reference of the warp mesh</returns>
        public Mesh GetDewarpMesh()
        {
            return this.warpMesh;
        }

        /// <summary>
        /// Returns the Dewarp mesh filter
        /// </summary>
        /// <returns>reference of the warp mesh filter </returns>
        public MeshFilter GetDewarpMeshFilter()
        {
            return this.warpMeshFilter;
        }

    }
}