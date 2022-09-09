//MIT License
//Copyright 2016-Present
//James H. Money
//Luke Kingsley
//Idaho National Laboratory
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
//to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute,
//sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
//INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniCAVE
{
    /// <summary>
    /// Handles the Warp tool for the user to move quickly in the scene.
    /// </summary>
    public class WarpTool : MonoBehaviour, ITool
    {
        public GameObject holder;
        public GameObject wandObject;
        private RaycastHit hit;
        public int rayLength;
        public Stack<Vector3> previousWarps;

        /// <summary>
        /// Basically a constructor
        /// </summary>
        private void Start()
        {
            //Get all necessary game objects
            if(wandObject == null)
            {
                Debug.LogError("Need to set wand object!");
            }

            if(holder == null)
            {
                Debug.LogError("Need to set top level UniCAVE object!");
            }

            //StartCoroutine("Raycaster");
            rayLength = 100;
            previousWarps = new Stack<Vector3>();
        }



        /// <summary>
        /// Teleports the IQstation the length of the raycast or to an object
        /// </summary>
        public void Warp(Vector3 origin, Vector3 direction)
        {
            //Add the location to the stack of warps
            previousWarps.Push(holder.transform.position);

            //returns true if an object is hit and fills the raycasthit struct with data
            if(Physics.Raycast(origin, direction * rayLength, out hit))
            {
                //Transport the IQstation to the object
                holder.transform.position = Vector3.Lerp(holder.transform.position, hit.point, .75f);
            }
            else
            {
                //Transport the IQstation to the end of the rayCast
                holder.transform.position = Vector3.Lerp(holder.transform.position, holder.transform.position + direction * rayLength, .75f);
            }
        }

        /// <summary>
        /// Keeps a record of all teleportations that the user does and reverts them back if desired
        /// </summary>
        public void undoWarp()
        {
            //If the stack isnt empty pop off the top object
            if(previousWarps.Count != 0)
            {
                holder.transform.position = previousWarps.Pop();
            }
        }

        /// <summary>
        /// Handles the button input for the warp tool
        /// </summary>
        public void ButtonClick(TrackerButton button, Vector3 origin_, Vector3 direction_)
        {

            if(button == TrackerButton.Trigger)
            {
                Warp(origin_, direction_);
            }
            if(button == TrackerButton.Button1)
            {
                undoWarp();
            }
        }


        ///////////Unimplemented Functions ///////////////////
        public void Analog(double x, double y)
        {
            throw new NotImplementedException();
        }

        public IEnumerator ButtonDrag()
        {
            throw new NotImplementedException();
        }

        public void init()
        {
        }

        public void shutDown()
        {
        }

        public void ButtonDrag(RaycastHit hit, Vector3 offset)
        {
            //Do nothing
        }

        /// <summary>
        /// Handles button down event - does nothing right now.
        /// </summary>
        /// <param name="buttonNum">The button pressed</param>
        /// <param name="origin">The position of the tracker</param>
        /// <param name="direction">The forward direction of the tracker</param>
        /// <param name="hit">The object hit with raycast.</param>
        public void ButtonDown(TrackerButton buttonNum, Vector3 origin, Vector3 direction, RaycastHit hit)
        {

        }

        public void ButtonDrag(RaycastHit hit, Vector3 offset, Vector3 origin, Vector3 direction)
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the tool name - warp tool.
        /// </summary>
        public string ToolName
        {
            get
            {
                return "Warp";
            }
        }
    }
}