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
//using UnityEditor;
using UnityEngine;

namespace UniCAVE
{
    /// <summary>
    /// The tool for scaling an object - currently not used.
    /// </summary>
    public class ScalerTool : MonoBehaviour, ITool
    {
        GameObject wandObject;
        GameObject holder;
        Vector3 origin;

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
            //throw new NotImplementedException();
        }

        public void ButtonClick(TrackerButton button, Vector3 origin, Vector3 direction)
        {
            //throw new NotImplementedException();
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

        /// <summary>
        /// Handles the scaling for the tool
        /// </summary>
        /// <param name="hit"></param>
        /// <param name="offset"></param>
        /// <param name="position"></param>
        /// <param name="direction"></param>
        public void ButtonDrag(RaycastHit hit, Vector3 offset, Vector3 position, Vector3 direction)
        {
            if(origin == null)
            {
                origin = hit.point;
            }
            Vector3 objectScale = hit.transform.localScale;
            Vector3 a = position - origin;
            float magnitude = Vector3.Magnitude(a);
            float dotProduct = Vector3.Dot(direction, a);
            float scale = magnitude * dotProduct;

            hit.transform.localScale = new Vector3(objectScale.x + scale, objectScale.y + scale, objectScale.z + scale);
        }

        /// <summary>
        /// The tool name - scale tool.
        /// </summary>
        public string ToolName
        {
            get
            {
                return "Scaler";
            }
        }
    }
}