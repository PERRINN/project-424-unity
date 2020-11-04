using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VehiclePhysics.Timing;
using System;
using System.Linq;
using JetBrains.Annotations;

// Attach script to Perrinn424 game object
namespace Project424
{
    public class LapSummaryTable : MonoBehaviour
    {
        // Variables for font and text box position

        public Vector2 position = new Vector2(8, 535);
        public Font font;
        public int fontSize = 10;
        public Color fontColor = Color.white;
        public bool showGui = true;

        // Reference to LapTimer component

        private LapTimer m_lapTimer;

        // Variable to count number the laps

        private int m_lapCount = 1;

        // List to store all laps

        private List<Lap> m_laps = new List<Lap>();

        // Variables for text, text style and textbox height/width

        string m_text = "";
        GUIStyle m_textStyle = new GUIStyle();
        float m_boxWidth;
        float m_boxHeight;

        // Variables to store best lap values

        float m_bestLap;
        float m_bestLapSector1;
        float m_bestLapSector2;
        float m_bestLapSector3;

        void OnEnable()
        {
            // Assign font variables
            m_textStyle.font = font;
            m_textStyle.fontSize = fontSize;
            m_textStyle.normal.textColor = fontColor;

            // Subscribe getLapTime method to onLap

            m_lapTimer = FindObjectOfType<LapTimer>();
            if (m_lapTimer != null) m_lapTimer.onLap += GetLapTime;

            ///// TEST LAPS /////
            //m_laps.Add(new Lap(1, 11.11f, 9.11f, 66.11f, 93.41231f, true));
            //m_laps.Add(new Lap(2, 26.11f, 1f, 12f, 84.41231f, true));
            //m_laps.Add(new Lap(3, 31.11f, 20f, 999f, 95.41231f, true));
            //m_laps.Add(new Lap(4, 99.11f, 38f, 50f, 86.41231f, true));
            //m_laps.Add(new Lap(5, 20.11f, 89f, 42f, 97.41231f, true));
            //m_laps.Add(new Lap(6, 41.11f, 399f, 34f, 88.41231f, false));
            //m_laps.Add(new Lap(7, 249.11f, 56f, 36f, 99.41231f, false));
            //m_laps.Add(new Lap(8, 55f, 99f, 78f, 90.41231f, false));
            //m_laps.Add(new Lap(9, 22f, 15f, 12f, 91.41231f, false));
            //m_laps.Add(new Lap(101, 40f, 76f, 32f, 92.41231f, false));

            m_lapCount = m_laps.Count();

            UpdateLapTable(m_laps);
        }

        void OnDisable()
        {
            // Unsubscribe getLapTime method from onLap

            if (m_lapTimer != null) m_lapTimer.onLap -= GetLapTime;
        }

        // Method to retrieve lapTime value and add it to m_laps list

        public void GetLapTime(float lapTime, bool validBool, float[] sectors, bool[] validSectors)
        {

            m_laps.Add(new Lap(m_lapCount, sectors[0], sectors[1], sectors[2], lapTime, validBool));
            m_lapCount++;

            if (m_bestLap == 0.0f && validBool == true || lapTime < m_bestLap && validBool == true)
            {
                // Reassign best lap to new value
                m_bestLapSector1 = sectors[0];
                m_bestLapSector2 = sectors[1];
                m_bestLapSector3 = sectors[2];
                m_bestLap = lapTime;
            }

            UpdateLapTable(m_laps);
        }

        // Method to update the last 10 values in the lap table with a given list of laps

        void UpdateLapTable(List<Lap> lapList)
        {
            // List of last ten laps
            List<Lap> lastTenLaps = new List<Lap>();
            foreach (Lap lap in lapList.Skip(Math.Max(0, lapList.Count) - 10))
            {
                lastTenLaps.Add(lap);
            }

            // Index to store position of foreach loop when iterating through list

            int prevIndex = 0;


            // String to store table's text values

            m_text = " Lap Number |  Sector 1 |  Sector 2 |  Sector 3 | Total Time \n";

            // Prints the last ten laps in the m_laps dictionary

            foreach (Lap lap in lastTenLaps)
            {
                m_text += ("   Lap " + lap.LapNum).PadRight(11, ' ');

                // If its the first lap
                if (prevIndex == 0)
                {
                    m_text +=
                        " | G " + FormatSectorTime(lap.Sector1) +
                        " | G " + FormatSectorTime(lap.Sector2) +
                        " | G " + FormatSectorTime(lap.Sector3) +
                        " | G" + FormatLapTime(lap.TotalLapTime) +
                        "\n";
                }

                // Else check current sectors against prev sectors
                else
                {
                    // Sector 1: If < then print as green, else print as red
                    if (lap.Sector1 < lastTenLaps[prevIndex - 1].Sector1)
                        m_text += " | G " + FormatSectorTime(lap.Sector1);
                    else
                        m_text += " | R " + FormatSectorTime(lap.Sector1);

                    // Sector 2
                    if (lap.Sector2 < lastTenLaps[prevIndex - 1].Sector2)
                        m_text += " | G " + FormatSectorTime(lap.Sector2);
                    else
                        m_text += " | R " + FormatSectorTime(lap.Sector2);

                    // Sector 3
                    if (lap.Sector3 < lastTenLaps[prevIndex - 1].Sector3)
                        m_text += " | G " + FormatSectorTime(lap.Sector3);
                    else
                        m_text += " | R " + FormatSectorTime(lap.Sector3);

                    // LapTime
                    if (lap.TotalLapTime < lastTenLaps[prevIndex - 1].TotalLapTime)
                        m_text += " | G" + FormatLapTime(lap.TotalLapTime);
                    else
                        m_text += " | R" + FormatLapTime(lap.TotalLapTime);

                    // Adds stars if the lap is invalid
                    if (lap.IsValid == true)
                        m_text += "\n";
                    else if (lap.IsValid == false)
                        m_text += "**\n";
                }
                prevIndex += 1;
            }

            // Prints the best lap as the last table entry

            m_text += "\n  Best Lap  |   " +
                FormatSectorTime(m_bestLapSector1) + " |   " +
                FormatSectorTime(m_bestLapSector2) + " |   " +
                FormatSectorTime(m_bestLapSector3) + " |" +
                FormatLapTime(m_bestLap) + "\n";
        }

        // Method to draw the table

        void OnGUI()
        {
            // Compute box size

            Vector2 contentSize = m_textStyle.CalcSize(new GUIContent(m_text));
            float margin = m_textStyle.lineHeight * 1.2f;
            float headerHeight = GUI.skin.box.lineHeight;

            m_boxWidth = contentSize.x + margin;
            m_boxHeight = contentSize.y + headerHeight + margin / 2;

            // Compute box position

            float xPos = position.x < 0 ? Screen.width + position.x - m_boxWidth : position.x;
            float yPos = position.y < 0 ? Screen.height + position.y - m_boxHeight : position.y;

            // Draw telemetry box

            GUI.Box(new Rect(xPos, yPos, m_boxWidth, m_boxHeight), "Lap Time Summary");
            GUI.Label(new Rect(xPos + margin / 2, yPos + margin / 2 + headerHeight, Screen.width, Screen.height), m_text, m_textStyle);

        }

        // Method to reformat float as a Lap Time (0:00:000)

        string FormatLapTime(float t)
        {
            int seconds = Mathf.FloorToInt(t);

            int m = seconds / 60;
            int s = seconds % 60;
            int mm = Mathf.RoundToInt(t * 1000.0f) % 1000;

            return string.Format("{0,3}:{1,2:00}:{2,3:000}", m, s, mm);
        }

        // Method to reformat float as Sector Time ((0)00:000)

        string FormatSectorTime(float t)
        {
            int seconds = Mathf.FloorToInt(t);

            int s = seconds;
            int mm = Mathf.RoundToInt(t * 1000.0f) % 1000;

            // We only want to show 3 numbers to the left of ':' if the value is >100
            if (s >= 100)
                return string.Format("{0,3:000}:{1,3:000}", s, mm);
            else
                return string.Format("{0,3:00}:{1,3:000}", s, mm);
        }
    }

    // Lap class stores all laps as objects that can be put into lists

    public class Lap
    {
        public int LapNum { get; set; }
        public float Sector1 { get; set; }
        public float Sector2 { get; set; }
        public float Sector3 { get; set; }
        public float TotalLapTime { get; set; }
        public bool IsValid { get; set; }

        public Lap(int lapNum, float sector1, float sector2, float sector3, float totalLapTime, bool isValid)
        {
            LapNum = lapNum;
            Sector1 = sector1;
            Sector2 = sector2;
            Sector3 = sector3;
            TotalLapTime = totalLapTime;
            IsValid = isValid;
        }

    }

    //// Method to sort the table based on a number

    //void SelectLapList (int choice)
    //{
    //    // Temporary list to hold the sorted lists

    //    List<Lap> temp_laps;

    //    // Based on choice's value, sort the table accordingly

    //    switch (choice)
    //    {
    //        case 1: // Lap Number Ascending
    //            temp_laps = (from entry in m_laps orderby entry.LapNum ascending select entry).ToList();
    //            break;
    //        case 2: // Lap Number Descending
    //            temp_laps = (from entry in m_laps orderby entry.LapNum descending select entry).ToList();
    //            break;
    //        case 3: // Sector 1 time Ascending
    //            temp_laps = (from entry in m_laps orderby entry.Sector1 ascending select entry).ToList();
    //            break;
    //        case 4: // Sector 1 time Descending
    //            temp_laps = (from entry in m_laps orderby entry.Sector1 descending select entry).ToList();
    //            break;
    //        case 5: // Sector 2 time Ascending
    //            temp_laps = (from entry in m_laps orderby entry.Sector2 ascending select entry).ToList();
    //            break;
    //        case 6: // Sector 2 time Descending
    //            temp_laps = (from entry in m_laps orderby entry.Sector2 descending select entry).ToList();
    //            break;
    //        case 7: // Sector 3 time Ascending
    //            temp_laps = (from entry in m_laps orderby entry.Sector3 ascending select entry).ToList();
    //            break;
    //        case 8: // Sector 3 time Descending
    //            temp_laps = (from entry in m_laps orderby entry.Sector3 descending select entry).ToList();
    //            break;
    //        case 9: // Total time Ascending
    //            temp_laps = (from entry in m_laps orderby entry.TotalLapTime ascending select entry).ToList();
    //            break;
    //        case 10: // Total time Descending
    //            temp_laps = (from entry in m_laps orderby entry.TotalLapTime descending select entry).ToList();
    //            break;
    //        default:
    //            temp_laps = m_laps;
    //            break;
    //    }

    //    // Update table with this sorted list

    //    UpdateLapTable(temp_laps);
    //}

}



