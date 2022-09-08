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
 * This class represents the toolManager object and keeps track of the different tool interfaces and shuffles between them as desired.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UniCAVE
{
    /// <summary>
    /// Class that manages the current tool. It runs the drive tool all the time and you can cycle through the tools with buttons.
    /// </summary>
    public class ToolManager
    {
        //Initialize member variables
        public List<ITool> list;
        public GameObject wandObject;
        public GameObject wandControls;
        public GameObject TopLevelUniCAVE;
        public int toolNumber = 0;
        public Text tool;
        public GameObject canvas;
        private DriveTool driveTool;
        public bool negativeAnalogX;
        public bool negativeAnalogY;

        // private string TOOL_NAME_PREFIX = "Tool: ";

        /// <summary>
        /// Creates the toolManager object which holds a list of all ITOOL interfaces
        /// </summary>
        /// <param name="wandObject_"></param>
        /// <param name="holder_"></param>
        public ToolManager(GameObject wandObject_, GameObject wandControls_, GameObject TopLevelUniCAVE_, double deadZone, float rotationSpeed, float movementSpeed, Text tool_, bool negateAnalogX_, bool negateAnalogY_)
        {
            wandObject = wandObject_;
            tool = tool_;
            wandControls = wandControls_;
            TopLevelUniCAVE = TopLevelUniCAVE_;
            negativeAnalogX = negateAnalogX_;
            negativeAnalogY = negateAnalogY_;

            list = new List<ITool>();

            //fill the list with all the tool interfaces
            list.Add(wandControls.GetComponent<WarpTool>());
            list.Add(wandControls.GetComponent<GrabberTool>());
            list.Add(wandControls.GetComponent<ButtonTool>());
            list.Add(wandControls.GetComponent<RotatorTool>());

            driveTool = new DriveTool(TopLevelUniCAVE, wandObject, deadZone, rotationSpeed, movementSpeed, negativeAnalogX, negativeAnalogY);

            toolNumber = 0;
            updateToolName(tool);
        }

        /// <summary>
        /// Increments the tool number
        /// </summary>
        public void NextTool()
        {
            list[toolNumber].shutDown();
            if(toolNumber + 1 >= list.Count)
            {
                toolNumber = 0;
            }
            else
            {
                toolNumber++;
            }
        }

        /// <summary>
        /// Decreases the tool number
        /// </summary>
        public void PreviousTool()
        {
            list[toolNumber].shutDown();
            if(toolNumber - 1 < 0)
            {
                toolNumber = list.Count - 1;
            }
            else
            {
                toolNumber--;
            }

        }

        /// <summary>
        /// Handles a button click event. It checks for previous/next click or passes it on to the selected tool. Also calls driver tool for button events.
        /// </summary>
        /// <param name="button">The button clicked</param>
        /// <param name="origin">The tracker position</param>
        /// <param name="direction">The forward direction of the tracker.</param>
        /// <returns></returns>
        public bool handleButtonClick(TrackerButton button, Vector3 origin, Vector3 direction)
        {
            if(button == TrackerButton.NextTool)
            {
                NextTool();
                updateToolName(tool);
                return true;
            }
            else if(button == TrackerButton.PreviousTool)
            {
                PreviousTool();
                updateToolName(tool);
                return true;
            }
            else
            {
                try
                {
                    list[toolNumber].ButtonClick(button, origin, direction);
                }
                catch(Exception e)
                {
                    Debug.LogError(e);
                }
                driveTool.ButtonClick(button, origin, direction);
                return true;
            }
        }


        /// <summary>
        /// Handles a drag event
        /// </summary>
        /// <param name="button">The button dragged</param>
        /// <param name="hit">The object hit</param>
        /// <param name="offset">The offset of the original hit from center of object.</param>
        /// <param name="origin">The tracker position</param>
        /// <param name="direction">The tracker forward direction.</param>
        /// <returns></returns>
        public bool handleButtonDrag(TrackerButton button, RaycastHit hit, Vector3 offset, Vector3 origin, Vector3 direction)
        {
            try
            {
                list[toolNumber].ButtonDrag(hit, offset, origin, direction);
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }
            return true;
        }

        /// <summary>
        /// Handles a button down event
        /// </summary>
        /// <param name="button">The button dragged</param>
        /// <param name="hit">The object hit</param>
        /// <param name="origin">The tracker position</param>
        /// <param name="direction">The tracker forward direction.</param>
        /// <returns></returns>
        public bool handleButtonDown(TrackerButton button, RaycastHit hit, Vector3 origin, Vector3 direction)
        {
            try
            {
                list[toolNumber].ButtonDown(button, origin, direction, hit);
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }
            return true;
        }

        /// <summary>
        /// Handles the analog changes.
        /// </summary>
        /// <param name="horizontal">The horizontal analog from [-1,1].</param>
        /// <param name="vertical">The vertical analog from [-1,1].</param>
        /// <returns></returns>
        public bool handleAnalog(double horizontal, double vertical)
        {
            try
            {
                driveTool.Analog(horizontal, vertical);
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }
            return true;
        }

        /// <summary>
        /// Updates the display of the tool name on screen.
        /// </summary>
        /// <param name="text">The Text object to update with tool name.</param>
        public void updateToolName(Text text)
        {
            //text.text = TOOL_NAME_PREFIX + list[toolNumber].ToolName;
        }


    }
}