//MIT License
//Copyright 2016-Present
//Ross Tredinnick
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace UniCAVE
{
    [RequireComponent(typeof(PhysicalDisplay))]
    public class PhysicalDisplayCalibration : MonoBehaviour
    {
        private static string getConfigFolder()
        {
            RealtimeCalibrator[] found = UnityEngine.Object.FindObjectsOfType<RealtimeCalibrator>();
            if(found.Length == 1)
            {
                return found[0].configsFolder;
            }
            return "configs/";
        }

        /// <summary>
        /// Holds the numer of the post processing layer used
        /// for rendering the post processing (dewarp rendering)
        /// </summary>
        [SerializeField, Layer]
        [Tooltip("The layer where post processing should be rendered")]
        public int postProcessLayer = 10;

        /// <summary>
        /// Offset for placing render textures in the world.
        /// </summary>
        /// <returns>vector 3 position</returns>
        public static Vector3 globalPostOffset = new Vector3(0.0f, 0.0f, 0.0f);

        /// <summary>
        /// The display ratio for the display
        /// this calibration is attached to.
        /// </summary>
        /// <value>display ratio</value>
        public float displayRatio
        {
            get
            {
                PhysicalDisplay pd = GetComponent<PhysicalDisplay>();
                return pd.windowBounds.width / (float)pd.windowBounds.height;
            }
        }

        /// <summary>
        /// Post process material to use for the calibration
        /// </summary>
        [Tooltip("This should be an unlit textured material")]
        public Material postProcessMaterial;

        /// <summary>
        /// Proportions of screenspace to blend.
        /// </summary>
        [Tooltip("Proportion of screenspace to blend")]
        public float rightBlend, topBlend, leftBlend, bottomBlend;

        /// <summary>
        /// The resolution the camera will render at before warp correction is applied.
        /// </summary>
        /// <returns></returns>
        [Tooltip("The resolution the camera will render at before warp correction")]
        public Vector2Int resolution = new Vector2Int(1280, 720);

        public GameObject camChild;

        private Material leftRenderMat, rightRenderMat;

        public List<Camera> postCams = new List<Camera>();

        /// EXTENSIONS HERE



        /// <summary>
        /// Used to set which head/eye camera we're
        /// holding on
        /// </summary>
        public enum HeadCamera
        {
            LEFT,
            RIGHT,
            CENTER
        }

        /// <summary>
        /// Holds the instance for the visual marker, that shows which vertex is beeing modified
        /// </summary>
        private LineRenderer visualMarkerInstance;

        /// <summary>
        /// Positions of the dewarp mesh vertices
        /// </summary>
        [SerializeField]
        private Dewarp.DewarpMeshPosition dewarpMeshPositions;

        /// <summary>
        /// Holds all the display <c>Dewarp</c> instances
        /// </summary>
        /// <typeparam name="HeadCamera">The camera that the warp is respoible for</typeparam>
        /// <typeparam name="Dewarp">The dewarp object</typeparam>
        /// <returns></returns>
        private Dictionary<HeadCamera, Dewarp> displayCalibrations = new Dictionary<HeadCamera, Dewarp>();

        /// <summary>
        /// Holds the display for this calibration
        /// </summary>
        private PhysicalDisplay display;

        /// <summary>
        /// Loads the calibration files from file.
        /// The folder loaded from is stored in the RealtimeCalibrator class,
        /// or by default is in a folder called configs relative to pwd
        /// </summary>
        [ContextMenu("Load calibrations from file")]
        public void LoadCalibrations()
        {
            string path = getConfigFolder();
            string fullPath = Path.Combine(path, "WARP - " + Util.ObjectFullName(gameObject) + ".conf");

            // LOADS THE WARP VERTICES CALIBRATIONS
            if(File.Exists(fullPath))
            {
                Debug.Log("Loading warp calibration \"" + fullPath + "\"");
                string content = File.ReadAllText(fullPath);
                string[] lines = content.Split('\n');
                List<Vector3> vecs = new List<Vector3>();
                foreach(string str in lines)
                {
                    string[] parts = str.Split('|');
                    if(parts.Length > 1)
                    {
                        vecs.Add(new Vector3(float.Parse(parts[0].Replace(',', '.')), float.Parse(parts[1].Replace(',', '.')), float.Parse(parts[2].Replace(',', '.'))));
                        //Debug.Log(new Vector2(float.Parse(parts[0]), float.Parse(parts[1])));
                    }
                }
                this.dewarpMeshPositions.verts = new Vector3[vecs.Count];
                for(int i = 0; i < vecs.Count; i++)
                {
                    this.dewarpMeshPositions.verts[i] = vecs[i];
                }
            }
            else
            {
                Debug.LogWarning("Warp calibration file could not be found");
            }

            // LOADS THE DISPLAYS POSITION CALIBRATIONS
            //fullPath = Path.Combine(path, "POS - " + Util.ObjectFullName(this.gameObject) + ".conf");
            //Debug.Log("Loading position calibration \"" + fullPath + "\"");
            //if (File.Exists(fullPath))
            //{
            //    string content = File.ReadAllText(fullPath);
            //    string[] parts = content.Split('|');
            //    Debug.Log(parts);
            //    if (parts.Length > 1)
            //    {
            //        this.transform.localPosition = new Vector3(float.Parse(parts[0].Replace(',', '.')), float.Parse(parts[1].Replace(',', '.')), float.Parse(parts[2].Replace(',', '.')));
            //    }

            //}
            //else
            //{
            //    Debug.LogWarning("Position calibration file could not be found");
            //}

            // LOADS THE DISPLAYS ROTATION CALIBRATIONS
            //fullPath = Path.Combine(path, "ROT - " + Util.ObjectFullName(this.gameObject) + ".conf");
            //Debug.Log("Loading rotation calibration \"" + fullPath + "\"");
            //if (File.Exists(fullPath))
            //{
            //    string content = File.ReadAllText(fullPath);
            //    string[] parts = content.Split('|');
            //    if (parts.Length > 1)
            //    {
            //        this.transform.localRotation = Quaternion.Euler(float.Parse(parts[0].Replace(',', '.')), float.Parse(parts[1].Replace(',', '.')), float.Parse(parts[2].Replace(',', '.')));
            //    }
            //}
            //else
            //{
            //    Debug.LogWarning("Rotation calibration file could not be found");
            //}

#if UNITY_EDITOR
            if(!EditorApplication.isPlaying) EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
#endif
        }

        /// <summary>
        /// Saves the Warp, rotation and position calibrations to file.
        /// The folder loaded from is stored in the RealtimeCalibrator class,
        /// or by default is in a folder called configs relative to pwd
        /// </summary>
        [ContextMenu("Save calibrations to file")]
        public void SaveWarpFile()
        {
            string path = getConfigFolder();
            Directory.CreateDirectory(path); // returns a DirectoryInfo object
            StringBuilder strBuilder = new StringBuilder();
            foreach(var vert in this.dewarpMeshPositions.verts)
            {
                strBuilder.Append(vert.x + "|" + vert.y + "|" + vert.z + "\n");
            }

            File.WriteAllText(Path.Combine(path, "WARP - " + Util.ObjectFullName(this.gameObject) + ".conf"), strBuilder.ToString());

            File.WriteAllText(Path.Combine(path, "POS - " + Util.ObjectFullName(this.gameObject) + ".conf"),
                this.transform.localPosition.x + "|" + this.transform.localPosition.y + "|" + this.transform.localPosition.z);

            File.WriteAllText(Path.Combine(path, "ROTGLOBAL - " + Util.ObjectFullName(this.gameObject) + ".conf"),
                transform.rotation.x + "|" + transform.rotation.y + "|" + transform.rotation.z);

            File.WriteAllText(Path.Combine(path, "ROT - " + Util.ObjectFullName(this.gameObject) + ".conf"),
                transform.localEulerAngles.x + "|" + transform.localEulerAngles.y + "|" + transform.localEulerAngles.z);
        }

        void SetupPostProcessing()
        {
            PhysicalDisplay display = GetComponent<PhysicalDisplay>();
            GameObject staticParent = new GameObject("Post Holder For: " + gameObject.name);

            // Apply multiplier factor on all vertices
            int numVertices = this.dewarpMeshPositions.verts.Length;
            Vector3[] verts = this.dewarpMeshPositions.verts;
            for(int i = 0; i < numVertices; i++)
            {
                verts[i] *= GetMultiplierFactor();
            }

            bool stereo = true;

            if(this.GetDisplay().is3D)
            {
                if(this.GetDisplay().leftCam != null)
                {
                    this.displayCalibrations.Add(HeadCamera.LEFT, new Dewarp(gameObject.name, this.postProcessMaterial, dewarpMeshPositions, this.GetDisplay().leftTex));
                    this.RemovePostProcessingFromHeadCamera(this.GetDisplay().leftCam);
                }
                else
                {
                    stereo = false;
                }

                if(this.GetDisplay().rightCam != null)
                {
                    this.displayCalibrations.Add(HeadCamera.RIGHT, new Dewarp(gameObject.name, this.postProcessMaterial, dewarpMeshPositions, this.GetDisplay().rightTex));
                    this.RemovePostProcessingFromHeadCamera(this.GetDisplay().rightCam);
                }
                else
                {
                    stereo = false;
                }
            }
            else
            {
                stereo = false;
                this.displayCalibrations.Add(HeadCamera.CENTER, new Dewarp(gameObject.name, this.postProcessMaterial, dewarpMeshPositions, this.GetDisplay().centerTex));
                this.RemovePostProcessingFromHeadCamera(this.GetDisplay().centerCam);
            }

            this.SetDewarpPositions(stereo);

            foreach(var displayCalibration in this.displayCalibrations)
            {
                GameObject obj = displayCalibration.Value.GetDewarpGameObject();
                obj.layer = this.postProcessLayer;
                obj.transform.parent = staticParent.transform;
            }

            {
                camChild = new GameObject("Calibration Cam (Left)");
                camChild.transform.parent = staticParent.transform;
                Camera postCam = camChild.AddComponent<Camera>();
                postCam.transform.localPosition = globalPostOffset + new Vector3(stereo ? -displayRatio * 2.0f : 0.0f, 0.0f, -1.0f);
                postCam.stereoTargetEye = StereoTargetEyeMask.Left;
                this.SetPostCameraProperties(postCam);
                postCams.Add(postCam);
            }
            {
                GameObject calibrationCamera = new GameObject("Calibration Cam (Right)");
                calibrationCamera.transform.parent = staticParent.transform;
                Camera postCam = calibrationCamera.AddComponent<Camera>();
                postCam.transform.localPosition = globalPostOffset + new Vector3(stereo ? displayRatio * 2.0f : 0.0f, 0.0f, -1.0f);
                postCam.stereoTargetEye = StereoTargetEyeMask.Right;
                this.SetPostCameraProperties(postCam);
                postCams.Add(postCam);
            }
            globalPostOffset = globalPostOffset + new Vector3(10, 10, 10);
        }

        /// <summary>
        /// Sets the position of the dewarp calibration
        /// game objects in the world.
        /// And create the visual marker for the calibration at the position.
        /// </summary>
        /// <param name="stereo">true if current display is stereo</param>
        private void SetDewarpPositions(bool stereo)
        {

            if(this.displayCalibrations.ContainsKey(HeadCamera.LEFT))
            {
                Transform trans = this.displayCalibrations[HeadCamera.LEFT].GetDewarpGameObject().transform;
                trans.localPosition = globalPostOffset + new Vector3(stereo ? -displayRatio * 2.0f : 0.0f, 0.0f, 0.0f);
                this.CreateVisualMarker(trans);
            }

            if(this.displayCalibrations.ContainsKey(HeadCamera.RIGHT))
            {
                Transform trans = this.displayCalibrations[HeadCamera.RIGHT].GetDewarpGameObject().transform;
                trans.localPosition = globalPostOffset + new Vector3(stereo ? displayRatio * 2.0f : 0.0f, 0.0f, 0.0f);
                this.CreateVisualMarker(trans);
            }

            if(this.displayCalibrations.ContainsKey(HeadCamera.CENTER))
            {
                Transform trans = this.displayCalibrations[HeadCamera.CENTER].GetDewarpGameObject().transform;
                trans.localPosition = globalPostOffset + new Vector3(0.0f, 0.0f, 0.0f);
                this.CreateVisualMarker(trans);
            }

        }

        /// <summary>
        /// Removes post processing layer from head/eye camera, so we dont
        /// see the post processing/dewarp meshes in the head/eye camera when we render to
        /// the post processing mesh. And set StereoTargetEyeMask to none.
        /// </summary>
        /// <param name="camera">the head camera</param>
        private void RemovePostProcessingFromHeadCamera(Camera camera)
        {
            camera.cullingMask &= ~(1 << this.postProcessLayer); //remove post processing layer of the worldspace camera
            Vector3 oldPos = camera.transform.localPosition;
            camera.stereoTargetEye = StereoTargetEyeMask.None;
            camera.transform.localPosition = oldPos;
        }

        /// <summary>
        /// Set post process camera attributes
        /// </summary>
        /// <param name="postCam">the camera to set attributes on</param>
        private void SetPostCameraProperties(Camera postCam)
        {
            postCam.nearClipPlane = 0.1f;
            postCam.farClipPlane = 10.0f;
            postCam.fieldOfView = 90.0f;
            postCam.stereoSeparation = 0.0f;
            postCam.stereoConvergence = 1000.0f; //probably doesn't matter but far away makes the most sense
            postCam.cullingMask = 1 << this.postProcessLayer; //post processing layer
            postCam.backgroundColor = Color.black;
            postCam.clearFlags = CameraClearFlags.SolidColor;
            postCam.depth = 1;
            postCam.allowHDR = false;
            postCam.allowMSAA = false;
            postCam.renderingPath = RenderingPath.Forward;
        }

        /// <summary>
        /// Returs the display that this calibration
        /// handles.
        /// </summary>
        /// <returns>the display of this calibration</returns>
        public PhysicalDisplay GetDisplay()
        {
            return this.display;
        }

        /// <summary>
        /// Calculates the vertext muliplier factor based on the display ratio
        /// for the display
        /// </summary>
        /// <returns>Mulitplier factor for vertex position</returns>
        private Vector2 GetMultiplierFactor()
        {
            return new Vector2(this.displayRatio, 1.0f);
        }

        /// <summary>
        /// Creates the visual marker when calibrating
        /// </summary>
        /// <param name="parent">the parent of the marker</param>
        private void CreateVisualMarker(Transform parent)
        {
            GameObject render = new GameObject("Visual renderer");
            render.layer = this.postProcessLayer;
            render.transform.parent = parent;

            this.visualMarkerInstance = render.AddComponent<LineRenderer>();
            this.visualMarkerInstance.useWorldSpace = false;

            Vector3[] pos = { parent.transform.position + new Vector3(0, 0, -.1f), parent.transform.localPosition };
            this.visualMarkerInstance.SetPositions(pos);
            this.visualMarkerInstance.startWidth = 0.008f;
        }

        /// <summary>
        /// Updats the dewarp/edgeblend mesh edge vertecies positions
        /// after a mesh has been modified.
        /// </summary>
        public void UpdateMeshPositions(Vector3[] vertex)
        {
            if(vertex != null)
            {
                int i = 0;
                foreach(var localVertex in vertex)
                {
                    this.dewarpMeshPositions.verts[i] = new Vector3(localVertex.x / this.displayRatio, localVertex.y, localVertex.z);
                    i++;
                }
                this.SaveWarpFile();
            }
        }

        /// <summary>
        /// Returns the <c>Dewarp</c> display calibrations object(s) for
        /// the display.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Dewarp> GetDisplayWarpsValues()
        {
            return this.displayCalibrations.Values;
        }

        #region CALIBRATIONS

        /// <summary>
        /// Move the display by given amount.
        /// </summary>
        /// <param name="pos">position to add to current position</param>
        public void MoveDisplay(Vector3 pos)
        {
            this.transform.position += pos;
        }

        /// <summary>
        /// Rotates the display by given amount.
        /// </summary>
        /// <param name="rot">rotation to add to current rotation</param>
        public void RotateDisplay(Vector3 rot)
        {
            this.transform.Rotate(rot);
        }

        /// <summary>
        /// Se
        /// </summary>
        /// <param name="vertexIndex"></param>
        public void SetVisualMarkerVertextPoint(int vertexIndex)
        {
            if(vertexIndex < this.dewarpMeshPositions.verts.Length)
                this.SetVisualMarker(this.dewarpMeshPositions.verts[vertexIndex]);
        }

        /// <summary>
        /// Move the visual marker to the the given vector position.
        /// Adds the visual multiplier factor based on display ratio (to match vertices positions).
        /// </summary>
        /// <param name="pos">the vertex position</param>
        private void SetVisualMarker(Vector2 pos)
        {
            if(this.visualMarkerInstance == null) return;
            this.ShowVisualMarker();
            this.visualMarkerInstance.SetPosition(1, this.visualMarkerInstance.transform.parent.localToWorldMatrix.MultiplyPoint3x4(pos * GetMultiplierFactor()));
        }

        /// <summary>
        /// Hids the visual marker
        /// </summary>
        public void HideVisualMarker()
        {
            this.visualMarkerInstance?.gameObject.SetActive(false);
        }

        /// <summary>
        /// Display the visual marker
        /// </summary>
        public void ShowVisualMarker()
        {
            this.visualMarkerInstance?.gameObject.SetActive(true);
        }

        #endregion

        void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            if(EditorApplication.isPlaying) return;
#endif

            PhysicalDisplay disp = GetComponent<PhysicalDisplay>();

            if(leftBlend != 0)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(disp.ScreenspaceToWorld(new Vector2(-1.0f - leftBlend, 1.0f)), disp.ScreenspaceToWorld(new Vector2(-1.0f - leftBlend, -1.0f)));
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(disp.ScreenspaceToWorld(new Vector2(-1.0f + leftBlend, 1.0f)), disp.ScreenspaceToWorld(new Vector2(-1.0f + leftBlend, -1.0f)));
            }
            if(topBlend != 0)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(disp.ScreenspaceToWorld(new Vector2(-1.0f, 1.0f + topBlend)), disp.ScreenspaceToWorld(new Vector2(1.0f, 1.0f + topBlend)));
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(disp.ScreenspaceToWorld(new Vector2(-1.0f, 1.0f - topBlend)), disp.ScreenspaceToWorld(new Vector2(1.0f, 1.0f - topBlend)));
            }
            if(rightBlend != 0)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(disp.ScreenspaceToWorld(new Vector2(1.0f + rightBlend, 1.0f)), disp.ScreenspaceToWorld(new Vector2(1.0f + rightBlend, -1.0f)));
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(disp.ScreenspaceToWorld(new Vector2(1.0f - rightBlend, 1.0f)), disp.ScreenspaceToWorld(new Vector2(1.0f - rightBlend, -1.0f)));
            }
            if(bottomBlend != 0)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(disp.ScreenspaceToWorld(new Vector2(-1.0f, -1.0f - bottomBlend)), disp.ScreenspaceToWorld(new Vector2(1.0f, -1.0f - bottomBlend)));
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(disp.ScreenspaceToWorld(new Vector2(-1.0f, -1.0f + bottomBlend)), disp.ScreenspaceToWorld(new Vector2(1.0f, -1.0f + bottomBlend)));
            }
        }

        public bool Initialized()
        {
            return initialized;
        }

        void Start()
        {
            PhysicalDisplay disp = this.display = gameObject.GetComponent<PhysicalDisplay>();
            disp.transform.localPosition = disp.transform.localPosition +
                disp.transform.right.normalized * (rightBlend - leftBlend) * disp.halfWidth() * 0.5f +
                disp.transform.up.normalized * (topBlend - bottomBlend) * disp.halfHeight() * 0.5f;

            Vector2 shift = new Vector2((leftBlend + rightBlend) * disp.halfWidth(), (bottomBlend + topBlend) * disp.halfHeight());
            disp.width += shift.x;
            disp.height += shift.y;
        }

        private bool initialized = false;
        void Update()
        {
            if(!initialized)
            {
                if(GetComponent<PhysicalDisplay>().Initialized())
                {
                    LoadCalibrations();
                    SetupPostProcessing();
                    initialized = true;
                }
            }
            else
            {
                PhysicalDisplay display = GetComponent<PhysicalDisplay>();
            }
        }
    }
}