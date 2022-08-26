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


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
#endif

namespace UniCAVE
{
    //Used to put multiple displays in one instance of the program
    //Start in full screen if you use this
    public class PhysicalDisplayManager : MonoBehaviour
    {


        [UnityEngine.Serialization.FormerlySerializedAs("machineName")]
        public string oldMachineName;
        public MachineName machineNameAsset;
        public string machineName => MachineName.GetMachineName(oldMachineName, machineNameAsset);
        public bool fullscreen = true;
        public int displayNumber = 0;
        public Vector2Int displayResolution;
        public List<PhysicalDisplay> displays = new List<PhysicalDisplay>();

        /// <summary>
        /// Whether or not this display should be active, in this case if the machine name matches actual machine name
        /// </summary>
        /// <returns>Should be active or not</returns>
        public bool ShouldBeActive()
        {
            return machineName == Util.GetMachineName();
        }

        /// <summary>
        /// Deactivate if needed
        /// </summary>
        void Start()
        {
            if(!ShouldBeActive())
            {
                Debug.Log("Deactivating Display Manager: " + gameObject.name);
                gameObject.SetActive(false);
                return;
            }
            Debug.Log("Display Manager Active: " + gameObject.name);
        }

        /// <summary>
        /// Whether all initialization operations have completed
        /// </summary>
        bool _initialized = false;

        /// <summary>
        /// Waits for all displays to be initialized, then repositions camera viewports and/or sets up post processing
        /// </summary>
        void Update()
        {
            if(!_initialized)
            {
                bool displaysInitialized = true;
                for(int i = 0; i < displays.Count; i++)
                {
                    if(!displays[i].gameObject.activeSelf) continue;

                    if(displays[i].enabled && !displays[i].Initialized())
                    {
                        displaysInitialized = false;
                        break;
                    }
                    PhysicalDisplayCalibration cali = displays[i].gameObject.GetComponent<PhysicalDisplayCalibration>();
                    if(cali != null && !cali.Initialized())
                    {
                        displaysInitialized = false;
                        break;
                    }
                }
                //wait until every other display is initialized

                if(displaysInitialized)
                {
                    for(int i = 0; i < displays.Count; i++)
                    {
                        PhysicalDisplay display = displays[i];

                        PhysicalDisplayCalibration cali = display.gameObject.GetComponent<PhysicalDisplayCalibration>();
                        if(cali == null)
                        {
                            if(!display.useRenderTextures)
                            {
                                if(display.dualPipe)
                                {
                                    Vector2Int windowSpaceOffset = display.dualInstance ? new Vector2Int(0, 0) : new Vector2Int(display.windowBounds.x, display.windowBounds.y);
                                    display.leftCam.pixelRect = new Rect(
                                        windowSpaceOffset.x + display.leftViewport.x,
                                        windowSpaceOffset.y + display.leftViewport.y,
                                        display.leftViewport.width,
                                        display.leftViewport.height);
                                    display.rightCam.pixelRect = new Rect(
                                        windowSpaceOffset.x + display.rightViewport.x,
                                        windowSpaceOffset.y + display.rightViewport.y,
                                        display.rightViewport.width,
                                        display.rightViewport.height);
                                }
                                else
                                {
                                    //if stereo blit is enabled, only update the viewport of the center cam (in the future perhaps consolidate this logic with useRenderTextures)
                                    if(display.centerCam != null && display.centerCam.GetComponent<StereoBlit>() != null)
                                    {
                                        display.centerCam.pixelRect = new Rect(display.windowBounds.x, display.windowBounds.y, display.windowBounds.width, display.windowBounds.height);
                                    }
                                    else
                                    {
                                        foreach(Camera cam in display.GetAllCameras())
                                        {
                                            Debug.Log("Manager [" + name + "] set Camera [" + cam.name + "] viewport to <"
                                                + display.windowBounds.x + ", " + display.windowBounds.y + ", " + display.windowBounds.width + ", " + display.windowBounds.height + ">");
                                            cam.pixelRect = new Rect(display.windowBounds.x, display.windowBounds.y, display.windowBounds.width, display.windowBounds.height);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            //special case for PhysicalDisplayCalibration
                            //Debug.Log("Display:");
                            foreach(Camera cam in cali.postCams)
                            {
                                Rect r = new Rect(display.windowBounds.x, display.windowBounds.y, display.windowBounds.width, display.windowBounds.height);
                                //Debug.Log("Set cam " + cam.name + " to " + r);
                                cam.pixelRect = r;
                            }
                        }
                    }

                    _initialized = true;
                }
            }
        }

        /// <summary>
        /// Search entire child tree for PhysicalDisplays and assign their manager to be this
        /// </summary>
        [ContextMenu("Assign This Manager to Children")]
        void AssignManagerChildren()
        {
            AssignManagerChildren_h(null);
        }

        /// <summary>
        /// helper function to assign this manager to all its children
        /// </summary>
        /// <param name="it">initial object to recursively iterate through</param>
        void AssignManagerChildren_h(GameObject it = null)
        {
            if(it == null) it = gameObject;

            for(int i = 0; i < it.transform.childCount; i++)
            {
                GameObject child = it.transform.GetChild(i).gameObject;
                PhysicalDisplay disp = child.GetComponent<PhysicalDisplay>();
                if(disp != null)
                {
                    if(disp.manager != null)
                    {
                        disp.manager.displays.Remove(disp);
                    }
                    RemoveDisplay(disp);
                    disp.manager = this;
                    AddDisplay(disp);
                }
                AssignManagerChildren_h(child);
            }
        }

        public bool AddDisplay(PhysicalDisplay physicalDisplay)
        {
            if(!displays.Contains(physicalDisplay))
            {
                displays.Add(physicalDisplay);
                return true;
            }
            else return false;
        }

        public bool RemoveDisplay(PhysicalDisplay physicalDisplay)
        {
            return displays.Remove(physicalDisplay);
        }

#if UNITY_EDITOR
        public static void SetManager(PhysicalDisplay physicalDisplay, PhysicalDisplayManager oldManager, PhysicalDisplayManager newManager, string undoName = "Set Manager")
        {
            //the SetDirty calls might not be necessary...
            Undo.SetCurrentGroupName(undoName);

            //remove from old manager
            if(oldManager)
            {
                Undo.RecordObject(oldManager, "Remove Display");

                oldManager.RemoveDisplay(physicalDisplay);
                EditorUtility.SetDirty(oldManager);
            }

            //set manager on display
            Undo.RecordObject(physicalDisplay, "Set New Manager");

            physicalDisplay.manager = newManager;
            EditorUtility.SetDirty(physicalDisplay);

            //add to new manager
            if(newManager)
            {
                Undo.RecordObject(newManager, "Add Display");

                newManager.AddDisplay(physicalDisplay);
                EditorUtility.SetDirty(newManager);
            }

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
        }

        [CustomEditor(typeof(PhysicalDisplayManager))]
        public class Editor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                serializedObject.Update();

                //draw machine name
                SerializedProperty oldMachineName = serializedObject.FindProperty(nameof(PhysicalDisplayManager.oldMachineName));
                SerializedProperty machineNameAsset = serializedObject.FindProperty(nameof(PhysicalDisplayManager.machineNameAsset));
                MachineName.DrawDeprecatedMachineName(oldMachineName, machineNameAsset, "Machine Name");

                //draw display settings
                SerializedProperty fullscreen = serializedObject.FindProperty(nameof(PhysicalDisplayManager.fullscreen));
                EditorGUILayout.PropertyField(fullscreen);

                if(fullscreen.boolValue)
                {
                    SerializedProperty displayNumber = serializedObject.FindProperty(nameof(PhysicalDisplayManager.displayNumber));
                    EditorGUILayout.PropertyField(displayNumber);
                }

                SerializedProperty displayResolution = serializedObject.FindProperty(nameof(PhysicalDisplayManager.displayResolution));
                EditorGUILayout.PropertyField(displayResolution);

                serializedObject.ApplyModifiedProperties();

                //draw buttons
                PhysicalDisplayManager physicalDisplayManager = target as PhysicalDisplayManager;

                if(GUILayout.Button("Assign Children To This Manager"))
                {
                    foreach(Transform child in physicalDisplayManager.transform)
                    {
                        PhysicalDisplay childPD = child.GetComponent<PhysicalDisplay>();

                        if(childPD) SetManager(childPD, childPD.manager, physicalDisplayManager, undoName: "Assign Children to Manager");
                    }
                }

                //draw displays
                GUILayout.Label("Associated Displays:");

                for(int i = 0; i < physicalDisplayManager.displays.Count; i++)
                {
                    PhysicalDisplay pd = physicalDisplayManager.displays[i];

                    pd = EditorGUILayout.ObjectField(pd, typeof(PhysicalDisplay), true) as PhysicalDisplay;

                    //remove any displays that are null
                    if(!pd)
                    {
                        Undo.SetCurrentGroupName("Remove Display");

                        PhysicalDisplay physicalDisplay = physicalDisplayManager.displays[i];
                        Undo.RecordObject(physicalDisplay, string.Empty);
                        if(physicalDisplay.manager == physicalDisplayManager)
                        {
                            physicalDisplay.manager = null;
                            EditorUtility.SetDirty(physicalDisplay);
                        }

                        Undo.RecordObject(physicalDisplayManager, string.Empty);
                        physicalDisplayManager.displays.RemoveAt(i);
                        EditorUtility.SetDirty(physicalDisplayManager);

                        Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

                        i--;
                    }
                }

                //draw field to add displays
                PhysicalDisplay addedPD = EditorGUILayout.ObjectField("Add Display", null, typeof(PhysicalDisplay), true) as PhysicalDisplay;

                if(addedPD) SetManager(addedPD, addedPD.manager, physicalDisplayManager, undoName: "Add Display");
            }
        }
#endif
    }
}