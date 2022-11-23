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
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UniCAVE
{
    /// <summary>
    /// Class handles all UI interactions for the controller including: buttons, sliders, scroll bars etc.
    /// </summary>
    public class ButtonTool : MonoBehaviour, ITool
    {
        //public GameObject canvas;
        public Canvas c;
        RaycastHit hit;
        public GameObject wandObject;
        public GameObject holder;
        public Text txt;
        RaycastHit tester;
        Vector3 origin, direction;

        RaycastHit objectHit;
        RaycastHit emptyHit;

        Dictionary<int, Dropdown> dropDownDictionary = new Dictionary<int, Dropdown>();

        /// <summary>
        /// Selects or highlights UI elements
        /// </summary>
        /// <returns></returns>
        public IEnumerator buttonInput()
        {
            while(true)
            {
                //check to see that we are on the buttonclick tool

                //Raycast into the scene
                Physics.Raycast(origin, direction, out tester);
                if(tester.collider != null)
                {
                    //Check what object is returned
                    if(tester.transform.gameObject.GetComponent<Dropdown>() != null)
                    {
                        //Get the correct compnent and select it
                        Dropdown dropdown = tester.transform.gameObject.GetComponent<Dropdown>();
                        EventSystem.current.SetSelectedGameObject(dropdown.gameObject);

                    }
                    else if(tester.transform.gameObject.GetComponent<Toggle>() != null)
                    {
                        Toggle toggle = tester.transform.gameObject.GetComponent<Toggle>();
                        EventSystem.current.SetSelectedGameObject(toggle.gameObject);

                    }
                    else if(tester.transform.gameObject.GetComponent<Button>() != null)
                    {
                        Button button = tester.transform.gameObject.GetComponent<Button>();
                        EventSystem.current.SetSelectedGameObject(button.gameObject);
                    }

                }
                else
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }

            yield return null;
            }
        }

        /// <summary>
        /// Handles button down event.
        /// </summary>
        /// <param name="buttonNum">The button pressed</param>
        /// <param name="origin">The position of the tracker</param>
        /// <param name="direction">The forward direction of the tracker</param>
        /// <param name="hit">The object hit with raycast.</param>
        public void ButtonDown(TrackerButton buttonNum, Vector3 origin, Vector3 direction, RaycastHit hit)
        {
            if(buttonNum == TrackerButton.Trigger)
                objectHit = hit;
        }

        public void ValueChanged(Dropdown dropdown, int val)
        {
            dropdown.onValueChanged.RemoveAllListeners();
            dropDownDictionary.Remove(dropdown.GetInstanceID());
            dropdown.Hide();
        }

        /// <summary>
        /// Handles the UI button click interactions
        /// </summary>
        /// <param name="buttonNum"></param>
        /// <param name="origin"></param>
        /// <param name="direction"></param>
        public void ButtonClick(TrackerButton buttonNum, Vector3 origin, Vector3 direction)
        {

            Physics.Raycast(origin, direction, out hit);
            Debug.Log(hit.point);

            if(buttonNum == TrackerButton.Trigger)
            {
                //If the object is a dropdown show or hide the menu
                if(hit.collider != null && hit.transform.gameObject.GetComponent<Dropdown>() != null)
                {
                    Dropdown dropdown = hit.transform.gameObject.GetComponent<Dropdown>();
                    Debug.Log(dropdown);

                    if(dropDownDictionary.ContainsKey(dropdown.GetInstanceID()))
                    {
                        dropdown.Hide();
                        dropDownDictionary.Remove(dropdown.GetInstanceID());
                    }
                    else
                    {
                        dropDownDictionary.Add(dropdown.GetInstanceID(), dropdown);
                        dropdown.onValueChanged.AddListener((e) => { ValueChanged(dropdown, e); });
                        dropdown.Show();
                    }



                }

                //If the object is a dropdown menu selectable set that as the new dropdown value and call the method attatched
                else if(hit.collider != null && hit.transform.gameObject.GetComponent<Toggle>() != null)
                {
                    Toggle toggle = hit.transform.gameObject.GetComponent<Toggle>();
                    toggle.isOn = !toggle.isOn;
                }
                //If the object is a button call the onClick method
                else if(hit.collider != null && hit.transform.gameObject.GetComponent<Button>() != null)
                {
                    Button button = hit.transform.gameObject.GetComponent<Button>();
                    if(buttonNum == TrackerButton.Trigger)
                    {
                        button.onClick.Invoke();
                    }

                }
                objectHit = emptyHit;
            }
        }

        /// <summary>
        /// Allows the user to interact with sliders and scrollbars
        /// </summary>
        /// <param name="hit_"></param>
        /// <param name="offset"></param>
        /// <param name="origin_"></param>
        /// <param name="direction_"></param>
        public void ButtonDrag(RaycastHit hit_, Vector3 offset, Vector3 origin_, Vector3 direction_)
        {
            origin = origin_;
            direction = direction_;

            //Check the type of the object to know what to slide
            if(objectHit.transform.gameObject.GetComponent<Slider>() != null)
            {
                Debug.Log("Got slider");
                slide(objectHit.transform.gameObject.GetComponent<Slider>(), origin, direction);
            }
            else if(objectHit.transform.gameObject.GetComponent<Scrollbar>() != null)
            {
                Debug.Log("Got scrollbar");
                slide(objectHit.transform.gameObject.GetComponent<Scrollbar>(), origin, direction);
            }
        }



        // Use this for initialization
        void OnEnable()
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
        /// User can interact with scrollbars
        /// </summary>
        /// <param name="s"></param>
        /// <param name="origin"></param>
        /// <param name="direction"></param>
        public void slide(Scrollbar scrollbar, Vector3 origin, Vector3 direction)
        {
            //Get the canvas component

            Physics.Raycast(origin, direction, out hit);
            //canvas = GameObject.Find("Canvas1");
            c = scrollbar.GetComponentInParent<Canvas>();//canvas.GetComponent<Canvas>();
            if(hit.distance > 0)
            {
                RectTransform rt = scrollbar.GetComponent<RectTransform>();
                Vector3 canvasPt = rt.InverseTransformPoint(hit.point);

                float percent;
                if((scrollbar.direction == Scrollbar.Direction.LeftToRight) || (scrollbar.direction == Scrollbar.Direction.RightToLeft))
                    percent = (canvasPt.x - rt.rect.xMin) / (rt.rect.xMax - rt.rect.xMin);
                else
                    percent = (canvasPt.y - rt.rect.yMin) / (rt.rect.yMax - rt.rect.yMin);

                if(percent > 1.0)
                    percent = 1.0f;
                else if(percent < 0.0)
                    percent = 0;

                if((scrollbar.direction == Scrollbar.Direction.RightToLeft) || (scrollbar.direction == Scrollbar.Direction.TopToBottom))
                    percent = 1 - percent;

                scrollbar.value = percent;
            }

        }
        /// <summary>
        /// Method allows the user to interact with sliders
        /// </summary>
        /// <param name="s"></param>
        /// <param name="origin"></param>
        /// <param name="direction"></param>
        public void slide(Slider slider, Vector3 origin, Vector3 direction)
        {
            Physics.Raycast(origin, direction, out hit);
            c = slider.GetComponentInParent<Canvas>();


            if(hit.distance > 0)
            {
                RectTransform rt = slider.GetComponent<RectTransform>();
                Vector3 canvasPt = rt.InverseTransformPoint(hit.point);

                float percent;
                if((slider.direction == Slider.Direction.LeftToRight) || (slider.direction == Slider.Direction.RightToLeft))
                    percent = (canvasPt.x - rt.rect.xMin) / (rt.rect.xMax - rt.rect.xMin);
                else
                    percent = (canvasPt.y - rt.rect.yMin) / (rt.rect.yMax - rt.rect.yMin);

                if(percent > 1.0)
                    percent = 1.0f;
                else if(percent < 0.0)
                    percent = 0;

                if((slider.direction == Slider.Direction.RightToLeft) || (slider.direction == Slider.Direction.TopToBottom))
                    percent = 1 - percent;

                slider.value = percent * (slider.maxValue - slider.minValue) + slider.minValue;
            }

        }


        //Unimplemented Methods
        void Update()
        {

        }

        public void Analog(double x, double y)
        {
            //throw new NotImplementedException();
        }

        public void init()
        {
            //throw new NotImplementedException();
        }

        public void shutDown()
        {
            c = null;
        }

        public string ToolName
        {
            get
            {
                return "Clicker";
            }
        }
    }
}