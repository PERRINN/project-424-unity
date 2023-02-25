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


/*
 * Class handles the movement of the wiimote around the worldspace
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniCAVE
{
    /// <summary>
    /// Handles the movement within the scene using two analog controls.
    /// </summary>
    public class DriveTool : ITool
    {

        public float movementSpeed;
        public float rotationSpeed;
        public double deadZone;
        public bool restrictVerticalMovement = true;
        public GameObject holder;
        public GameObject wand;
        private bool negateAnalogX = false;
        private bool negateAnalogY = true;

        private bool rotateVertical = false;
        private bool rotateHorizontal = true;

        /// <summary>
        /// DriveTool Constructor. Object handles all the analog
        /// </summary>
        /// <param name="deadZone_"></param>
        /// <param name="rotationSpeed_"></param>
        /// <param name="movementSpeed_"></param>
        public DriveTool(GameObject holder_, GameObject wandObject_, double deadZone_, float rotationSpeed_, float movementSpeed_, bool negateAnalogX_, bool negateAnalogY_)
        {
            holder = holder_;
            wand = wandObject_;
            movementSpeed = movementSpeed_;
            rotationSpeed = rotationSpeed_;
            deadZone = deadZone_;
            negateAnalogX = negateAnalogX_;
            negateAnalogY = negateAnalogY_;
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
        /// Function handles all the analog data and movement of the IQstation
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Analog(double x, double y)
        {
            //Create a vector fromt the X and Y data
            Vector2 analogData = new Vector2((float)(((negateAnalogX) ? -1 : 1) * x), (float)(((negateAnalogY) ? -1 : 1) * y));

            //Set the movement speed of the wandObject to the magnitude of the vector
            //**RT - 11/8/2017 commenting this out as it just ends up overriding the editor setting (and in our case was way too fast)
            //movementSpeed = analogData.magnitude; /// 2f;

            //Check if the Y value is bad input
            if(y >= deadZone || y <= -deadZone) // || movementSpeed <= -deadZone)
            {
                //Check if the IQ station is initialized
                if(holder != null)
                {
                    //Check to see whether you are rotating or translating the object
                    if(rotateVertical)
                        Rotate(analogData, true);
                    else
                        Translate(analogData, true);
                }
            }

            //Check if the X value is bad input
            if(x >= deadZone || x <= -deadZone) // || analogHorizontal <= -deadZone)
            {
                //Check if the IQ station is initialized
                if(holder != null)
                {
                    //Check to see whether you are rotating or translating the object
                    if(rotateHorizontal)
                        Rotate(analogData, false);
                    else
                        Translate(analogData, false);
                }
            }
        }

        /// <summary>
        /// Switches between rotate and translate for horizontal movement
        /// </summary>
        public void setHorizontal()
        {
            rotateHorizontal = !rotateHorizontal;
        }

        /// <summary>
        /// Switches between rotate and translate for vertical movement
        /// </summary>
        public void setVertical()
        {
            rotateVertical = !rotateVertical;
        }

        /// <summary>
        /// Handles button click - does nothing
        /// </summary>
        /// <param name="button"></param>
        public void ButtonClick(TrackerButton button)
        {
            throw new NotImplementedException();
        }

        public void init()
        {
            throw new NotImplementedException();
        }

        public void shutDown()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Function moves the object around the world space
        /// </summary>
        /// <param name="analogData"></param>
        /// <param name="vertical"></param>
        private void Translate(Vector2 analogData, bool vertical)
        {
            Vector3 modTrans;
            Vector3 movement;
            float value;
            float sign;

            //If Vertical is true get the y value from the vector and set other values
            if(vertical)
            {
                movement = Vector3.forward;
                value = analogData.y;
                sign = Mathf.Sign(analogData.y);
            }
            else
            {
                movement = Vector3.left;
                value = analogData.x;
                sign = Mathf.Sign(analogData.x);
            }

            //Calculate the vector the object will be moving on using the rotation of the wandObject
            if(wand != null)
            {
                modTrans = wand.transform.localRotation * movement * movementSpeed;
            }
            else
            {
                modTrans = Vector3.forward * movementSpeed;
            }
            //If you are moving in the horizontal direction set the y to 0
            if(!vertical)
            {
                modTrans.y = 0;
            }

            //If the sign is postive move it accordingly
            if(sign > 0)
            {   //moving forward or right
                holder.transform.Translate(modTrans * .5f);

            }
            else
            {   //moving back or left
                holder.transform.Translate(-modTrans * .5f);

            }
        }

        /// <summary>
        /// Function rotates the object about the world space
        /// </summary>
        /// <param name="analogData"></param>
        /// <param name="Vertical"></param>
        private void Rotate(Vector2 analogData, bool Vertical)
        {
            if(Vertical)
            {
                float sign = Mathf.Sign(analogData.y);
                if(sign > 0)
                {   //rotating up
                    holder.transform.Rotate(new Vector3(rotationSpeed * movementSpeed, 0, 0));
                }
                else
                {   //rotating down
                    holder.transform.Rotate(new Vector3(-rotationSpeed * movementSpeed, 0, 0));
                }
            }
            else
            {
                float sign = Mathf.Sign(analogData.x);
                if(sign > 0)
                {   //rotating right
                    holder.transform.Rotate(new Vector3(0, rotationSpeed * movementSpeed, 0));
                }
                else
                {   //rotating left
                    holder.transform.Rotate(new Vector3(0, -rotationSpeed * movementSpeed, 0));
                }
            }
        }

        /// <summary>
        /// Takes in the button input which tells the object which way to rotate
        /// </summary>
        /// <param name="button"></param>
        /// <param name="origin"></param>
        /// <param name="direction"></param>
        /// <param name="isPressed"></param>
        public void ButtonClick(TrackerButton button, Vector3 origin, Vector3 direction)
        {

            if(button == TrackerButton.Button3)
                setHorizontal();
            else if(button == TrackerButton.Button4)
                setVertical();
        }

        /// <summary>
        /// Handles button drag - not used.
        /// </summary>
        /// <param name="hit"></param>
        /// <param name="offset"></param>
        /// <param name="origin"></param>
        /// <param name="direction"></param>
        public void ButtonDrag(RaycastHit hit, Vector3 offset, Vector3 origin, Vector3 direction)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The name of the tool - Drive tool.
        /// </summary>
        public string ToolName
        {
            get
            {
                return "Drive";
            }
        }
    }
}