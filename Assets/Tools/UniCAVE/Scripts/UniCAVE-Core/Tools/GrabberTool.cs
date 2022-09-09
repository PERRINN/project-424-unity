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
using System.Linq;
using System.Text;
using UnityEngine;

namespace UniCAVE
{
    /// <summary>
    /// Grabber tool enables the user to select objects and drag them around the scene using the WiiMote's orientation.
    /// </summary>
    public class GrabberTool : MonoBehaviour, ITool
    {
        //Initialize
        public GameObject holder;
        public GameObject wandObject;
        private RaycastHit hit;
        public int rayLength = 200;
        public Vector3 origin, direction, previousOrigin, previousDirection;

        //Initializes all the necessary fields while rendering the scene
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
        /// Handles the button input for the grabberTool
        /// Takes in every button and whether it is currently being pressed or not
        /// While pointing at the object the user can increase or decrease the size of the object
        /// </summary>
        public void ButtonClick(TrackerButton button, Vector3 origin_, Vector3 direction_)
        {
            //Check to see if the raycast has intersected with an object
            if((Physics.Raycast(origin, direction, out hit)))
            {
                //Get the current scale of the object
                Vector3 scale = hit.transform.localScale;
                if(button == TrackerButton.Button2)
                {
                    hit.transform.localScale = new Vector3(scale.x + scale.x / 7, scale.y + scale.y / 7, scale.z + scale.z / 7);
                }
                else if(button == TrackerButton.Button3)
                {
                    hit.transform.localScale = new Vector3(scale.x - scale.x / 7, scale.y - scale.y / 7, scale.z - scale.z / 7);
                }

            }
        }

        /// <summary>
        /// Asyncrounous method that moves whatever object the raycast from the wand has hit.
        /// </summary>
        /// <returns></returns>
        public void ButtonDrag(RaycastHit hit_, Vector3 offset_, Vector3 origin_, Vector3 direction_)
        {
            //Set the objects rotation equal to the wands
            hit_.transform.eulerAngles = wandObject.transform.eulerAngles;

            //Set the direction of the wand.
            direction = wandObject.transform.forward;   //NOTE: hit.point does not update...

            //offset = hit.transform.position - hit.point;
            //Set the transform of the object hit
            Vector3 number = origin_ + (direction * hit_.distance) + offset_;
            hit_.transform.position = origin_ + (direction * hit_.distance) + offset_;
        }


        ///////////Unimplemented Functions ///////////////////

        public void init()
        {
            throw new NotImplementedException();
        }

        public void shutDown()
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Handles analog input - not used
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Analog(double x, double y)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Handles the name of the tool - grab tool.
        /// </summary>
        public string ToolName
        {
            get
            {
                return "Grabber";
            }
        }
    }
}