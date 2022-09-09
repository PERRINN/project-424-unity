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
    /// handles the rotation of an object.
    /// </summary>
    public class RotatorTool : MonoBehaviour, ITool
    {
        public GameObject wandObject;
        public GameObject holder;
        RaycastHit refHit;


        /// <summary>
        /// Rotates the object when the user has selected it
        /// </summary>
        /// <param name="hit"></param>
        /// <param name="offset"></param>
        /// <param name="origin"></param>
        /// <param name="direction"></param>
        public void ButtonDrag(RaycastHit hit, Vector3 offset, Vector3 origin, Vector3 direction)
        {
            if(refHit.collider == null)
            {
                refHit = hit;
            }

            hit.transform.rotation = wandObject.transform.rotation;

        }

        // Use this for initialization
        void Start()
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
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void init()
        {
            throw new NotImplementedException();
        }

        public void shutDown()
        {
            refHit = new RaycastHit();
        }

        public void Analog(double x, double y)
        {
            throw new NotImplementedException();
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
        public void ButtonClick(TrackerButton button, Vector3 origin, Vector3 direction)
        {

        }

        /// <summary>
        /// Name of the tool - Rotate tool.
        /// </summary>
        public string ToolName
        {
            get
            {
                return "Rotator";
            }
        }
    }
}