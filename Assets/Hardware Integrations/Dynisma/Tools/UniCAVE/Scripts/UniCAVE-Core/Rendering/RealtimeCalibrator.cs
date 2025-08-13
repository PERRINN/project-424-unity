using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using VersionCompatibility;

namespace UniCAVE
{
    public class RealtimeCalibrator : NetworkBehaviour
    {
        public struct CalibrationSelection
        {
            public string machineName;
            public PhysicalDisplayCalibration calibration;
        }

        /// <summary>
        /// Calibration types
        /// </summary>
        private enum CalibrationType
        {
            VERTEX,
            POSITION,
            ROTATION
        }

        /// <summary>
        /// Holds the current calibration type.
        /// Default is <c>CalibrationType.VERTEX</c>
        /// </summary>
        private CalibrationType calibrationType = CalibrationType.VERTEX;

        /// <summary>
        /// The currently selected display index
        /// </summary>
        public int selectedIndex = 0;

        /// <summary>
        /// The last selected display index
        /// </summary>
        public int lastSelectedIndex = 0;

        /// <summary>
        /// Holds all displays which can be calibrated
        /// </summary>
        public List<CalibrationSelection> allOptions;

        /// <summary>
        /// The selected vertex index
        /// </summary>
        public int vertexIndex = 0;

        /// <summary>
        /// Reference to info display object.
        /// This object is responsible for displaying the current
        /// calibration type.
        /// </summary>
        [SerializeField]
        private InfoDisplay infoDisplay;

        /// <summary>
        /// The instantiated instance of InfoDisplay for the right eye/cam
        /// </summary>
        private InfoDisplay infoDisplayInstance;

        /// <summary>
        /// Verteces selection size. A size of 0 will select only one
        /// vertex. A size of two wil select all vetices around the selected vertex.
        /// </summary>
        private int gridSelectSize = 0;

        /// <summary>
        /// Folder in which configs are located
        /// </summary>
        public string configsFolder = "configs";

        void OnEnable()
        {
            allOptions = new List<CalibrationSelection>();
            //generate list of options
            List<PhysicalDisplay> displays = gameObject.GetComponent<UCNetwork>().GetAllDisplays();
            foreach(PhysicalDisplay disp in displays)
            {
                PhysicalDisplayCalibration cali = disp.gameObject.GetComponent<PhysicalDisplayCalibration>();
                if(cali != null)
                {
                    CalibrationSelection selection;
                    selection.machineName = (disp.manager == null) ? disp.machineName : disp.manager.machineName;
                    selection.calibration = cali;
                    allOptions.Add(selection);
                }
            }

            Debug.Log("RealtimeCalibration: Found " + allOptions.Count + " calibration objects");
            StartCoroutine(InitiateInfoScreen());
        }

        /// <summary>
        /// Instatiate the info screen with a delay.
        /// So we are sure everything has initialized before
        /// setting the info screen
        /// </summary>
        /// <returns></returns>
        private IEnumerator InitiateInfoScreen()
        {
            yield return new WaitForSeconds(3);
            this.CreateInfoDisplay();
            this.InfoDisplayShift(this.selectedIndex);
            if(isServer) this.RpcInfoDisplayShift(this.selectedIndex);
            yield break;
        }

        /// <summary>
        /// Instantiates the info displays.
        /// And set them to disabled at start
        /// </summary>
        private void CreateInfoDisplay()
        {
            if(this.infoDisplay == null) return;
            this.infoDisplayInstance = Instantiate(infoDisplay);
            this.infoDisplayInstance.gameObject.SetActive(false);
        }

        /// <summary>
        /// Moves a vertex by index in a direction (-1, +1 or 0) by a factor delta.
        /// A direction can be Vector2(-1,0); move the display in a negative X direction.
        /// The factor delta sets the "speed/distance" of the movement.
        /// </summary>
        /// <param name="direction">the direction to move in</param>
        /// <param name="delta">movement factor</param>
        /// <param name="selectedIndex">the display to move vertex on</param>
        /// <param name="vertexIndex">the vertex to move</param>
        private void LocalShift(Vector2 direction, float delta, int selectedIndex, int vertexIndex)
        {
            PhysicalDisplayCalibration lastCalibration = allOptions[lastSelectedIndex].calibration;
            lastCalibration.HideVisualMarker();
            PhysicalDisplayCalibration calibration = allOptions[selectedIndex].calibration;
            calibration.SetVisualMarkerVertextPoint(vertexIndex);

            Debug.Log("RealtimeCalibration: LocalShift called " + delta + ", " + selectedIndex + ", " + vertexIndex);

            MeshFilter lastWarpedFilter = null;
            foreach(Dewarp dewarp in calibration.GetDisplayWarpsValues())
            {
                MeshFilter meshFilter = dewarp.GetDewarpMeshFilter();
                lastWarpedFilter = meshFilter;
                Vector3[] verts = meshFilter.sharedMesh.vertices;
                // verts[vertexIndex] = new Vector3(verts[vertexIndex].x + direction.x * delta, verts[vertexIndex].y + direction.y * delta, verts[vertexIndex].z);

                Dictionary<int, float> vertsToShift = this.getIndexesSurrounding(dewarp.xSize, vertexIndex);
                foreach(var ind in vertsToShift)
                {
                    verts[ind.Key] = new Vector3(verts[ind.Key].x + (direction.x * delta * ind.Value), verts[ind.Key].y + (direction.y * delta * ind.Value), verts[ind.Key].z);
                }

                meshFilter.sharedMesh.vertices = verts;
                meshFilter.sharedMesh.UploadMeshData(false);
                meshFilter.mesh.RecalculateBounds();
                meshFilter.mesh.RecalculateTangents();

            }

            calibration.UpdateMeshPositions(lastWarpedFilter?.sharedMesh.vertices);
        }

        /// <summary>
        /// Move the position of a display in a given direction (-1, +1 or 0) by a factor delta.
        /// A direction can be Vector3(-1,0,0) ; move the display in a negative X direction.
        /// The factor delta sets the "speed/distance" of the movement.
        /// </summary>
        /// <param name="direction">the direction to move in</param>
        /// <param name="delta">movement factor</param>
        /// <param name="selectedIndex">display index of the display to move</param>
        private void LocalPositionShift(Vector3 direction, float delta, int selectedIndex)
        {
            PhysicalDisplayCalibration lastCalibration = allOptions[lastSelectedIndex].calibration;
            lastCalibration.HideVisualMarker();
            PhysicalDisplayCalibration calibration = allOptions[selectedIndex].calibration;
            calibration.SetVisualMarkerVertextPoint(vertexIndex);
            calibration.MoveDisplay(new Vector3(direction.x * delta, direction.y * delta, direction.z * delta));
        }

        /// <summary>
        /// Rotates a display in a around a given axis (-1, +1 or 0) by a factor delta.
        /// A roation can be Vector3(-1,0,0) ; roate the display around X axis in negative direction.
        /// The factor delta sets the "speed" of the rotation.
        /// </summary>
        /// <param name="direction">the axis to rotate around</param>
        /// <param name="delta">rotation factor</param>
        /// <param name="selectedIndex">display index of the display to rotate</param>
        private void LocalRotationShift(Vector3 direction, float delta, int selectedIndex)
        {
            PhysicalDisplayCalibration lastCalibration = allOptions[lastSelectedIndex].calibration;
            lastCalibration.HideVisualMarker();
            PhysicalDisplayCalibration calibration = allOptions[selectedIndex].calibration;
            calibration.SetVisualMarkerVertextPoint(vertexIndex);
            calibration.RotateDisplay(new Vector3(direction.x * delta, direction.y * delta, direction.z * delta));
        }

        /// <summary>
        /// Adjust the grid selection size of vertex movement.
        /// Uses bool to tell if it is a increase or decrease.
        /// </summary>
        /// <param name="increase">true to increase, false to decrease</param>
        private void LocalAdjustGridSelectSize(bool increase)
        {
            if(increase)
            {
                this.gridSelectSize++;
            }
            else
            {
                this.gridSelectSize = (this.gridSelectSize--) <= 0 ? 0 : this.gridSelectSize--;
            }
        }

        /// <summary>
        /// Shifts the info window to the display on the given index
        /// </summary>
        /// <param name="selectedIndex">index of the display to set display on</param>
        private void InfoDisplayShift(int selectedIndex)
        {
            PhysicalDisplayCalibration currentDisplay = this.allOptions[selectedIndex].calibration;
            if(currentDisplay == null || this.infoDisplayInstance == null) return;

            if(currentDisplay.GetDisplayWarpsValues().Count() > 0)
            {
                this.SetInfoDisplay(infoDisplayInstance.gameObject, currentDisplay.GetDisplayWarpsValues().First().GetDewarpGameObject().transform);
                this.infoDisplayInstance.SetText(this.calibrationType.ToString());
            }
        }

        /// <summary>
        /// Activates the info display, sets it parent to provided transform,
        /// and resets its local position so it is centered to the parent.
        /// </summary>
        /// <param name="infoDisplay"></param>
        /// <param name="parent"></param>
        private void SetInfoDisplay(GameObject infoDisplay, Transform parent)
        {
            infoDisplay.gameObject.SetActive(true);
            infoDisplay.transform.SetParent(parent);
            infoDisplay.transform.localPosition = new Vector2(0, 0);
        }

        /// <summary>
        /// Creates a dictionary of indexes and move factors which holds the
        /// indexs surrounding the provided index, the selection is a square around the index,
        /// and each square selection will get a "move factor". The move factor tells how much the verex at index
        /// will be allowed to move compared to the main vertex(provided index). The selections move factor will degrade by
        /// selection size. This makes it possible to create a "smudge effect"
        ///
        /// The size is the X axis size.
        /// </summary>
        /// <param name="size">the x axis size</param>
        /// <param name="index">the index number</param>
        /// <returns>dictionary of indexeswith the index and a move factor</returns>
        public Dictionary<int, float> getIndexesSurrounding(int size, int index)
        {

            int sizeX = size;

            int indexSizeX = size + 1;

            // The row the selected index is on
            int indexRow = (index / (sizeX + 1)) < 1.0f ? 0 : (int)Mathf.Floor(index / (sizeX + 1));

            int startRow = (indexRow - this.gridSelectSize) >= 0 ? (indexRow - this.gridSelectSize) : 0;
            int endRow = (indexRow + this.gridSelectSize) <= sizeX ? (indexRow + this.gridSelectSize) : sizeX;

            // Vertecies to move
            Dictionary<int, float> vertsToShift = new Dictionary<int, float>();

            float moveFactor = (float)this.gridSelectSize / (float)((this.gridSelectSize * 2));
            float currentMoveFactor = moveFactor;

            for(int row = startRow; row <= endRow; row++)
            {
                int rowDiff = indexRow - row;

                int startIndexForRow = 0;
                int stopIndexForRow = 0;
                int midIndexForRow = 0;

                int minSize = row * indexSizeX;
                int maxSize = minSize + sizeX;

                startIndexForRow = index - (indexSizeX * (rowDiff)) - this.gridSelectSize;
                startIndexForRow = (startIndexForRow < minSize) ? minSize : startIndexForRow;

                midIndexForRow = index - (indexSizeX * (rowDiff));

                stopIndexForRow = index - (indexSizeX * (rowDiff)) + this.gridSelectSize;
                stopIndexForRow = (stopIndexForRow > maxSize) ? maxSize : stopIndexForRow;

                int factor = 2;
                for(int i = midIndexForRow + 1; i <= stopIndexForRow; i++)
                {
                    vertsToShift.Add(i, moveFactor / factor);
                    factor++;
                }
                factor = 2;
                for(int i = midIndexForRow - 1; i >= startIndexForRow; i--)
                {
                    vertsToShift.Add(i, moveFactor / factor);
                    factor++;
                }

                if(midIndexForRow == index)
                {
                    vertsToShift.Add(midIndexForRow, 1);
                }
                else
                {
                    vertsToShift.Add(midIndexForRow, moveFactor);
                }

            }
            return vertsToShift;

        }

        /// <summary>
        /// Assign all point positions based on positions of the four corner points;
        /// positions are linearly interpolated
        /// </summary>
        public void LocalInterpolateFromCorners(int selectedIndex)
        {
            PhysicalDisplayCalibration lastCalibration = allOptions[lastSelectedIndex].calibration;
            lastCalibration.HideVisualMarker();
            PhysicalDisplayCalibration calibration = allOptions[selectedIndex].calibration;
            calibration.SetVisualMarkerVertextPoint(vertexIndex);

            Debug.Log("RealtimeCalibration: InterpolateFromCorners called, selectedIndex " + selectedIndex);

            MeshFilter lastWarpedFilter = null;
            foreach(Dewarp dewarp in calibration.GetDisplayWarpsValues())
            {
                MeshFilter meshFilter = dewarp.GetDewarpMeshFilter();
                lastWarpedFilter = meshFilter;
                Vector3[] verts = meshFilter.sharedMesh.vertices;
                // verts[vertexIndex] = new Vector3(verts[vertexIndex].x + direction.x * delta, verts[vertexIndex].y + direction.y * delta, verts[vertexIndex].z);

                Vector3 corner_0 = verts[0];
                Vector3 corner_1 = verts[7];
                Vector3 corner_2 = verts[63];
                Vector3 corner_3 = verts[56];
                for(int x = 0; x < 8; x++)
                {
                    float xfac = x / 7.0f;
                    Vector3 xmix_0 = corner_0 * (1.0f - xfac) + corner_1 * xfac;
                    Vector3 xmix_1 = corner_3 * (1.0f - xfac) + corner_2 * xfac;
                    for(int y = 0; y < 8; y++)
                    {
                        float yfac = y / 7.0f;
                        verts[y * 8 + x] = xmix_0 * (1.0f - yfac) + xmix_1 * yfac;
                    }
                }

                meshFilter.sharedMesh.vertices = verts;
                meshFilter.sharedMesh.UploadMeshData(false);
                meshFilter.mesh.RecalculateBounds();
                meshFilter.mesh.RecalculateTangents();

            }

            calibration.UpdateMeshPositions(lastWarpedFilter?.sharedMesh.vertices);
        }

        /// <summary>
        /// Invoked from server, calls LocalInterpolateFromCorners
        /// </summary>
        [ClientRpc]
        public void RpcInterpolateFromCorners(int selectedIndex)
        {
            LocalInterpolateFromCorners(selectedIndex);
        }

        /// <summary>
        /// Triggers local and Rpc vertex shift methods
        /// </summary>
        public void InterpolateFromCorners()
        {
            Debug.Log("server gives selectedIndex " + selectedIndex);
            LocalInterpolateFromCorners(selectedIndex);
            if(isServer) RpcInterpolateFromCorners(selectedIndex);
        }

        /// <summary>
        /// Cycles to the next calibration type, if it reaches the end
        /// start over.
        /// </summary>
        /// <returns>type of calibration :(pos, rot, vertex)</returns>
        private CalibrationType CycleNextCalibrationType()
        {
            this.calibrationType = (from CalibrationType val in Enum.GetValues(typeof(CalibrationType)) where val > this.calibrationType orderby val select val).DefaultIfEmpty().First();
            if(isServer) this.RpcSetCalibrationType(this.calibrationType);
            this.infoDisplayInstance.SetText(this.calibrationType.ToString());
            return this.calibrationType;
        }

        /// <summary>
        /// Triggers adjustment of grid selection size of vertex movement.
        /// Uses bool to tell if it is a increase or decrease.
        /// </summary>
        /// <param name="increase">true to increase, false to decrease</param>
        private void AdjustGridSelectSize(bool increase)
        {
            this.LocalAdjustGridSelectSize(increase);
            if(isServer) this.RpcAdjustGridSelectSize(increase);
        }

        /// <summary>
        /// Triggers local and Rpc vertex shift methods
        /// </summary>
        /// <param name="direction">the direction to move</param>
        /// <param name="delta">the speed/steps of movement</param>
        private void VertexShift(Vector2 direction, float delta)
        {
            LocalShift(direction, delta, selectedIndex, vertexIndex);
            if(isServer) RpcShift(direction, delta, selectedIndex, vertexIndex);
        }

        /// <summary>
        /// Triggers local and Rpc display positions methods
        /// </summary>
        /// <param name="direction">the direction to move</param>
        /// <param name="delta">the speed/steps of movement</param>
        private void PositionShift(Vector3 direction, float delta)
        {
            this.LocalPositionShift(direction, delta, selectedIndex);
            if(isServer) this.RpcMovePosition(direction, delta, selectedIndex);
        }

        /// <summary>
        /// Triggers local and Rpc display rotation methods
        /// </summary>
        /// <param name="direction">the direction of rotation</param>
        /// <param name="delta">the speed/steps of the rotation</param>
        private void RotationShift(Vector3 direction, float delta)
        {
            this.LocalRotationShift(direction, delta, selectedIndex);
            if(isServer) this.RpcRotate(direction, delta, selectedIndex);
        }

        /// <summary>
        /// Triggers local and Rpc info display shift methods
        /// </summary>
        private void DisplayShift()
        {
            InfoDisplayShift(selectedIndex);
            if(isServer) RpcInfoDisplayShift(selectedIndex);
        }

        /// <summary>
        /// Triggers Rpc set last display index method and set it on local
        /// <param name="index">the index to set as last index</param>
        private void SetLastIndex(int index)
        {
            this.lastSelectedIndex = index;
            if(isServer) this.RpcSetLastIndex(index);
        }

        #region RPCCALLS

        /// <summary>
        /// Client RPC methods which triggers the local vertex shift movement method.
        /// </summary>
        /// <param name="direction">direction to move in</param>
        /// <param name="delta">movement factor</param>
        /// <param name="selectedIndex">display index</param>
        /// <param name="vertexIndex">vertex index</param>
        [ClientRpc]
        void RpcShift(Vector2 direction, float delta, int selectedIndex, int vertexIndex)
        {
            LocalShift(direction, delta, selectedIndex, vertexIndex);
        }

        /// <summary>
        /// Client RPC methods which triggers the local display movement method.
        /// </summary>
        /// <param name="direction">direction to move in</param>
        /// <param name="delta">movement factor</param>
        /// <param name="selectedIndex">display index</param>
        [ClientRpc]
        void RpcMovePosition(Vector2 direction, float delta, int selectedIndex)
        {
            LocalPositionShift(direction, delta, selectedIndex);
        }
        /// <summary>
        /// Client RPC methods which triggers the local display rotate method.
        /// </summary>
        /// <param name="direction">axis to rotate around and direction</param>
        /// <param name="delta">rotate speed factor</param>
        /// <param name="selectedIndex">display index</param>
        [ClientRpc]
        void RpcRotate(Vector2 direction, float delta, int selectedIndex)
        {
            LocalRotationShift(direction, delta, selectedIndex);
        }

        /// <summary>
        /// Client RPC method which sets the calibration type on each client
        /// </summary>
        /// <param name="calibrationType">calibration type to set</param>
        [ClientRpc]
        void RpcSetCalibrationType(CalibrationType calibrationType)
        {
            this.calibrationType = calibrationType;
            this.infoDisplayInstance.SetText(calibrationType.ToString());
        }

        /// <summary>
        /// Client RPC method which calls <c>InfoDisplayShift</c> and activates
        /// the info display on provided display index
        /// </summary>
        /// <param name="selectedIndex">the selected display index</param>
        [ClientRpc]
        void RpcInfoDisplayShift(int selectedIndex)
        {
            this.InfoDisplayShift(selectedIndex);
        }

        /// <summary>
        /// Client RPC method which calls <c>LocalAdjustGridSelectSize</c> on clients to
        /// adjustment of grid selection size of vertex movement.
        /// Uses bool to tell if it is a increase or decrease.
        /// </summary>
        /// <param name="increase">true to increase, false to decrease</param>
        [ClientRpc]
        void RpcAdjustGridSelectSize(bool increase)
        {
            LocalAdjustGridSelectSize(increase);
        }

        /// <summary>
        /// Client RPC method which sets the last selected display index
        /// </summary>
        /// <param name="index">the index of the last selected display</param>
        [ClientRpc]
        private void RpcSetLastIndex(int index)
        {
            this.lastSelectedIndex = index;
        }

#if UNITY_EDITOR
        /// <summary>
        /// For each physical display calibration, load the calibration
        /// </summary>
        [ContextMenu("Load All Calibrations")]
        private void LoadAllCalibrations()
        {
            var list = UnityEngine.Object.FindObjectsOfType<PhysicalDisplayCalibration>();
            foreach(PhysicalDisplayCalibration cali in list)
            {
                cali.LoadCalibrations();
            }
        }
#endif

        #endregion

        void Update()
        {
            if(UnityInput.GetKeyDown(UnityKey.Space))
            {
                this.CycleNextCalibrationType();
            }

            if(UnityInput.GetKeyDown(UnityKey.NumpadPlus))
            {
                this.AdjustGridSelectSize(true);
            }

            if(UnityInput.GetKeyDown(UnityKey.NumpadMinus))
            {
                this.AdjustGridSelectSize(false);
            }

            if(UnityInput.GetKeyDown(UnityKey.Home))
            {
                this.InterpolateFromCorners();
            }

            Vector3 direction = Vector3.zero;
            bool anyPressed = false;
            bool noOptions = allOptions.Count == 0;

            if(UnityInput.shiftKeyPressed)
            {
                if(UnityInput.GetKeyDown(UnityKey.Enter))
                {
                    int index = this.selectedIndex - 1;
                    if(index < 0)
                    {
                        this.selectedIndex = allOptions.Count - 1;
                    }
                    else
                    {
                        this.selectedIndex = Mathf.Abs((selectedIndex - 1) % allOptions.Count);
                    }
                    if(!noOptions)
                    {
                        DisplayShift();
                        VertexShift(direction, 1f);
                    }
                }

            }
            else if(UnityInput.GetKeyDown(UnityKey.Enter))
            {
                this.selectedIndex = (selectedIndex + 1) % allOptions.Count;
                if(!noOptions)
                {
                    DisplayShift();
                    VertexShift(direction, 1f);
                }
            }

            if(UnityInput.GetKeyDown(UnityKey.A))
            {

                int lastIndex = this.vertexIndex - 1;
                this.vertexIndex = (lastIndex < 0) ? 63 : lastIndex;
                Debug.Log(vertexIndex);
                if(!noOptions)
                {
                    Debug.Log(vertexIndex);
                    this.VertexShift(direction, 1f);
                }

            }

            if(UnityInput.GetKeyDown(UnityKey.D))
            {
                this.vertexIndex = (vertexIndex + 1) % 64;
                if(!noOptions)
                {
                    Debug.Log(vertexIndex);
                    this.VertexShift(direction, 1f);
                }
            }

            if(UnityInput.GetKeyDown(UnityKey.W))
            {
                this.vertexIndex = (vertexIndex + 8) % 64;
                if(!noOptions)
                {
                    Debug.Log(vertexIndex);
                    this.VertexShift(direction, 1f);
                }
            }

            if(UnityInput.GetKeyDown(UnityKey.S))
            {
                this.vertexIndex = (vertexIndex + (64 - 8)) % 64;
                if(!noOptions)
                {
                    Debug.Log(vertexIndex);
                    this.VertexShift(direction, 1f);
                }
            }

            if(noOptions) { return; }

            if(UnityInput.GetKey(UnityKey.RightArrow))
            {
                direction.x = 1;
                anyPressed = true;
            }
            else if(UnityInput.GetKey(UnityKey.UpArrow))
            {
                direction.y = 1;
                anyPressed = true;
            }
            else if(UnityInput.GetKey(UnityKey.LeftArrow))
            {
                direction.x = -1;
                anyPressed = true;
            }
            else if(UnityInput.GetKey(UnityKey.DownArrow))
            {
                direction.y = -1;
                anyPressed = true;
            }
            else if(UnityInput.GetKey(UnityKey.Numpad8))
            {
                direction.z = -1;
                anyPressed = true;
            }
            else if(UnityInput.GetKey(UnityKey.Numpad2))
            {
                direction.z = 1;
                anyPressed = true;
            }

            if(anyPressed)
            {
                Debug.Log("RealtimeCalibration: isServer = " + isServer);
                if(isServer)
                {
                    switch(this.calibrationType)
                    {
                        case CalibrationType.POSITION:
                            this.PositionShift(direction, 0.0015f);
                            break;
                        case CalibrationType.ROTATION:
                            this.RotationShift(direction, 0.10f);
                            break;
                        case CalibrationType.VERTEX:
                            this.VertexShift(direction, 0.0015f);
                            break;
                    }

                }
            }
            this.SetLastIndex(selectedIndex);
        }
    }
}