//MIT License
//Copyright 2016-Present 
//Ross Tredinnick
//Brady Boettcher
//Living Environments Laboratory
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
//to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, 
//sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
//INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using System.Threading;
using UnityEngine.Networking;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UniCAVE
{
    /// <summary>
    /// Updated VRPNInput module with tool management.
    /// </summary>\
    [ExecuteInEditMode]
    public class VRPNInput : NetworkBehaviour
    {

        [SerializeField]
        private string trackerAddress = "WiiMote0@localhost";

        [SerializeField]
        private int channelHorizontal = 2;
        [SerializeField]
        private int channelVertical = 1;
        [SerializeField]
        private bool trackButton = true;
        [SerializeField]
        private bool trackAnalog = true;
        public float rotationSpeed = 0.05f;



        public TrackerButtonList trackerButtonList;
        public GameObject TopLevelUniCAVE;
        public GameObject wandObject = null;

        public bool debugOutput = false;
        public float movementSpeed = 0.01f;
        public double deadZone = 0.05;
        public Text tool;
        public GameObject canvas;
        public bool rotationMovement = false;
        public GameObject panel;
        public int maxButtons = 20;
        public float rayLength = 200;
        public bool negativeAnalogX = false;
        public bool negativeAnalogY = true;


        private RaycastHit hit;
        private Vector3 offset;
        private ToolManager toolManager;
        private int lastButtonPressed = -1;
        private Dictionary<TrackerButton, bool> buttonState = new Dictionary<TrackerButton, bool>();

        private const int MAX_LOOPS_BUTTON_CHECK = 20;
        private const int SLEEP_TIMEOUT = 100;




        public bool TrackButton
        {
            get { return trackButton; }
            set
            {
                trackButton = value;
                StopCoroutine("Button");
                if(trackButton && Application.isPlaying)
                {
                    StartCoroutine("Button");
                }
            }
        }

        public bool TrackAnalog
        {
            get { return trackAnalog; }
            set
            {
                trackAnalog = value;
                StopCoroutine("Analog");
                if(trackAnalog && Application.isPlaying)
                {
                    StartCoroutine("Analog");
                }
            }
        }


        /// <summary>
        /// Handles the startup
        /// </summary>
        private void Start()
        {
            if(Application.isPlaying)
            {
                if(trackerButtonList == null)
                    trackerButtonList = this.GetComponent<TrackerButtonList>();

                if(trackerButtonList == null)
                {
                    this.gameObject.AddComponent(typeof(TrackerButtonList));
                    trackerButtonList = this.GetComponent<TrackerButtonList>();
                }
                //Add a toolManager to the wandObject to shuffle between tools
                toolManager = new ToolManager(wandObject, this.gameObject, TopLevelUniCAVE, deadZone, rotationSpeed, movementSpeed, tool, negativeAnalogX, negativeAnalogY);
                //add state of each button
                foreach(TrackerButton btn in Enum.GetValues(typeof(TrackerButton)))
                {
                    buttonState.Add(btn, false);
                }

                //this gets rid of this object from non-head nodes...we only want this running on the machine that connects to VRPN...
                //this assumes a distributed type setup, where one machine connects to the tracking system and distributes information
                //to other machines...
                //some setups may try to connect each machine to vrpn...
                //in that case, we wouldn't want to destroy this object..
                //if (Util.GetMachineName() != master.masterMachineName) {
                //    Debug.Log("Removing tracker settings from " + Util.GetMachineName());
                //    Destroy(this);
                //    return;
                //}

                if(!isServer)
                {
                    Destroy(this);
                    return;
                }
#if !UNITY_EDITOR
            //Start the coroutines
            if (trackButton)
                StartCoroutine("Button");

            if (trackAnalog)
                StartCoroutine("Analog");
#endif
            }

        }


        /// <summary>
        /// Asynchronous method taking in button input and sending it to the current selected tool
        /// </summary>
        /// <returns></returns>
        private IEnumerator Button()
        {
            int maxButtons = trackerButtonList.getMaxButtons();

            while(true)
            {
                Vector3 origin = wandObject.transform.position;
                Vector3 direction = wandObject.transform.forward;

                //i tracks the number of the current button
                for(int i = 0; i < maxButtons; ++i)
                {
                    //Current value of the button
                    bool curValue = VRPN.vrpnButton(trackerAddress, i);
                    TrackerButton currentButton = trackerButtonList.MapButton(i);

                    //If the previous state is true and the current value is false it is a button click
                    if(buttonState.ContainsKey(currentButton) && buttonState[currentButton] && !curValue)
                    {
                        //Fire the event
                        toolManager.handleButtonClick(currentButton, origin, direction);

                        //hasStarted = false;
                        hit = new RaycastHit(); //temp;

                    }
                    //If the current and previous are true then it is a buttondrag event
                    else if(buttonState.ContainsKey(currentButton) && buttonState[currentButton] && curValue && (currentButton == TrackerButton.Trigger))
                    {
                        if(hit.distance > 0)
                        {
                            //Fire the event
                            toolManager.handleButtonDrag(currentButton, hit, offset, origin, direction);
                        }
                    }
                    //If the previous is false and the current is true 
                    else if(!(buttonState.ContainsKey(currentButton) && buttonState[currentButton]) && curValue && (currentButton == TrackerButton.Trigger))
                    {
                        Physics.Raycast(origin, direction * rayLength, out hit);
                        if(hit.distance > 0)
                            offset = hit.transform.position - hit.point;

                        toolManager.handleButtonDown(currentButton, hit, origin, direction);
                    }

                    //update the previous value
                    buttonState[currentButton] = curValue;
                }
                yield return null;
            }
        }

        /// <summary>
        /// Asyncronous method that handles all the input from the analog stick
        /// </summary>
        /// <returns></returns>
        private IEnumerator Analog()
        {
            while(true)
            {
                //Get the X and Y values from the joystick
                double analogVertical = VRPN.vrpnAnalog(trackerAddress, channelVertical);
                double analogHorizontal = VRPN.vrpnAnalog(trackerAddress, channelHorizontal);

                //Translate the holder
                toolManager.handleAnalog(analogHorizontal, analogVertical);
                yield return null;
            }
        }

        /// <summary>
        /// Gets the latest pushed button on tracker from vrpn. Stops after a preset amount of time (2 seconds currently).
        /// </summary>
        /// <returns>The number of the pushed button (0...n-1) or -1 if no button is pushed on tracker.</returns>
        public int GetPushedButton()
        {
            lastButtonPressed = -1;
            for(int ii = 0; ii < MAX_LOOPS_BUTTON_CHECK; ii++)
            {
                for(int jj = 0; jj < maxButtons; jj++)
                {
                    bool btnValue = VRPN.vrpnButton(trackerAddress, jj, ii);
                    if(btnValue)
                        lastButtonPressed = jj;

                }
                if(lastButtonPressed > -1)
                {
                    return lastButtonPressed;
                }
                Thread.Sleep(SLEEP_TIMEOUT);
            }
            return -1;

        }

    }
}