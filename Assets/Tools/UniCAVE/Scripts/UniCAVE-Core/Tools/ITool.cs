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
 * Interface that provides the methods for the various tools which the wand will use
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniCAVE
{
    /// <summary>
    /// The basic tool interface used by all tools
    /// </summary>
    public interface ITool
    {
        /// <summary>
        /// Gets the name of the tool
        /// </summary>
        string ToolName { get; }

        /// <summary>
        /// Starts the tool.
        /// </summary>
        void init();

        /// <summary>
        /// Stops the tool
        /// </summary>
        void shutDown();


        void ButtonDown(TrackerButton buttonNum, Vector3 origin, Vector3 direction, RaycastHit hit);

        /// <summary>
        /// Handles the button click event. Not this happens on button up.
        /// </summary>
        /// <param name="buttonNum">The button enum to pressed.</param>
        /// <param name="origin">The position of the tracker</param>
        /// <param name="direction">The forward direction of the tracker</param>
        void ButtonClick(TrackerButton buttonNum, Vector3 origin, Vector3 direction);

        /// <summary>
        /// Handles the analog input from tracker.
        /// </summary>
        /// <param name="x">The x coord of the tracker. From [-1,.1].</param>
        /// <param name="y">The y coord of the tracker. From [-1,.1].</param>
        void Analog(double x, double y);

        /// <summary>
        /// Handles a drag event if a button is held down.
        /// </summary>
        /// <param name="hit">The object hit.</param>
        /// <param name="offset">The offset of the hit from the object center.</param>
        /// <param name="origin">The tracker position</param>
        /// <param name="direction">The forward direction of the tracker.</param>
        void ButtonDrag(RaycastHit hit, Vector3 offset, Vector3 origin, Vector3 direction);
    }
}