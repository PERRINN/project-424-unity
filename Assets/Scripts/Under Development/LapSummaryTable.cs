using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VehiclePhysics.Timing;

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

            /// TEST DATA /////
            //m_laps.Add(new Lap(1, 82.111f, false, 27.750f, 30.610f, 29.938f, true, true, true));
            //m_laps.Add(new Lap(2, 83.298f, true, 27.740f, 29.610f, 28.938f, true, true, true));
            //m_laps.Add(new Lap(3, 82.298f, true, 28.450f, 29.610f, 27.938f, true, true, true));
            //m_laps.Add(new Lap(4, 84.298f, true, 28.950f, 27.500f, 26.938f, false, true, true));
            //m_laps.Add(new Lap(5, 82.598f, true, 27.750f, 27.600f, 27.938f, true, true, false));
            //m_laps.Add(new Lap(6, 83.998f, true, 28.750f, 27.610f, 28.938f, true, true, true));
            //m_laps.Add(new Lap(7, 82.998f, true, 27.450f, 29.610f, 29.938f, true, true, true));
            //m_laps.Add(new Lap(8, 85.498f, false, 27.350f, 28.610f, 26.838f, false, false, true));
            //m_laps.Add(new Lap(9, 82.198f, true, 28.550f, 27.610f, 26.938f, true, true, true));
            //m_laps.Add(new Lap(10, 82.398f, true, 29.750f, 27.499f, 26.938f, true, true, true));
            //m_lapCount += m_laps.Count();

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

            m_laps.Add(new Lap(m_lapCount, lapTime, validBool, sectors[0], sectors[1], sectors[2], validSectors[0], validSectors[1], validSectors[2]));
            m_lapCount++;

            // Checks if the laptime is a new best
            // Filters-out invalid laps or laps where any sector is 0 seconds

            if (m_bestLap == 0.0f && validBool == true && sectors.Contains(0) == false
                || lapTime < m_bestLap && validBool == true && sectors.Contains(0) == false)
            {
                // Reassign best lap to new values
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

            // Pointers to the laps which have the best times

            Lap bestSec1Time = null;
            Lap bestSec2Time = null;
            Lap bestSec3Time = null;
            Lap bestTotalTime = null;

            // Finds the best sector/lap times and assigns them before we print them

            foreach (Lap lap in m_laps)
            {
                // If there is no best for this time
                if (bestTotalTime == null)
                {
                    // If this time is valid
                    if (lap.LapIsValid == true)
                        // This time is now the best time
                        bestTotalTime = lap;
                }
                else
                {
                    // If this time is less than the best time AND its valid (or if there is no best and this lap is valid)
                    if (lap.TotalLapTime < bestTotalTime.TotalLapTime && lap.LapIsValid == true || bestTotalTime == null && lap.LapIsValid == true)
                    {
                        // If pointer is not null
                        if (bestTotalTime != null)
                            bestTotalTime.TotalIsGreen = true;
                        // Set the best lap to green and then set this lap as the best lap
                        bestTotalTime = lap;
                    }
                }

                // Same logic as above, but for the sector times
                if (bestSec1Time == null)
                {
                    if (lap.Sector1IsValid == true)
                        bestSec1Time = lap;
                }
                else
                {
                    if (lap.Sector1 < bestSec1Time.Sector1 && lap.Sector1IsValid == true || bestSec1Time == null && lap.Sector1IsValid == true)
                    {
                        if (bestSec1Time != null)
                            bestSec1Time.Sec1IsGreen = true;
                        bestSec1Time = lap;
                    }
                }

                // Sector 2
                if (bestSec2Time == null)
                {
                    if (lap.Sector2IsValid == true)
                        bestSec2Time = lap;
                }
                else
                {
                    if (lap.Sector2 < bestSec2Time.Sector2 && lap.Sector2IsValid == true || bestSec2Time == null && lap.Sector2IsValid == true)
                    {
                        if (bestSec2Time != null)
                            bestSec2Time.Sec2IsGreen = true;
                        bestSec2Time = lap;
                    }
                }

                // Sector 3
                if (bestSec3Time == null)
                {
                    if (lap.Sector3IsValid == true)
                        bestSec3Time = lap;
                }
                else
                {
                    if (lap.Sector3 < bestSec3Time.Sector3 && lap.Sector3IsValid == true || bestSec3Time == null && lap.Sector3IsValid == true)
                    {
                        if (bestSec3Time != null)
                            bestSec3Time.Sec3IsGreen = true;
                        bestSec3Time = lap;
                    }
                }
            }

            // String to store table's text values

            m_text = " Lap Number | Sector 1 | Sector 2 | Sector 3 | Total Time\n";

            // Prints the last ten laps in the m_laps dictionary 

            foreach (Lap lap in lastTenLaps)
            {
                m_text += ("   Lap " + lap.LapNum).PadRight(11, ' ');

                // Sector 1
                m_text += printSector(lap, lap.Sector1, lap.Sector1IsValid, lap.Sec1IsGreen, bestSec1Time, m_laps).PadRight(11, ' ');

                // Sector 2
                m_text += printSector(lap, lap.Sector2, lap.Sector2IsValid, lap.Sec2IsGreen, bestSec2Time, m_laps).PadRight(11, ' ');

                // Sector 3
                m_text += printSector(lap, lap.Sector3, lap.Sector3IsValid, lap.Sec3IsGreen, bestSec3Time, m_laps).PadRight(11, ' ');

                // LapTime
                m_text += printLap(lap, lap.TotalLapTime, lap.LapIsValid, lap.TotalIsGreen, bestTotalTime, m_laps);
            }

            // Prints the best lap as the last table entry

            m_text += "\n Best Lap   | " +
                FormatSectorTime(m_bestLapSector1) + "  | " +
                FormatSectorTime(m_bestLapSector2) + "  | " +
                FormatSectorTime(m_bestLapSector3) + "  |" +
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

        // Method to handle how the sectors are printed

        string printSector(Lap thisSector, float thisSectorTime, bool sectorValid, bool sectorGreen, Lap bestSector, List<Lap> lapList)
        {
            string text = " | ";

            if (lapList.Count == 1) // If there's only one lap, color it white
                text += FormatSectorTime(thisSectorTime);
            else if (thisSector == bestSector) // Purple
                text += "<color=#ff00ffff>" + FormatSectorTime(thisSectorTime) + " </color>";
            else if (sectorGreen == true) // Green
                text += "<color=#008000ff>" + FormatSectorTime(thisSectorTime) + " </color>";
            else // White
                text += FormatSectorTime(thisSectorTime);
            if (sectorValid == false)
                text += "*";
            return text;
        }

        // Method to handle how the Total Time is printed

        string printLap(Lap thisSector, float thisSectorTime, bool sectorValid, bool sectorGreen, Lap bestSector, List<Lap> lapList)
        {
            string text = " |";

            if (lapList.Count == 1) // If there's only one lap, color it white
                text += FormatLapTime(thisSectorTime);
            else if (thisSector == bestSector) // Purple
                text += "<color=#ff00ffff>" + FormatLapTime(thisSectorTime) + " </color>";
            else if (sectorGreen == true) // Green
                text += "<color=#008000ff>" + FormatLapTime(thisSectorTime) + " </color>";
            else // White
                text += FormatLapTime(thisSectorTime);
            if (sectorValid == false)
                text += "*";
            text += "\n";
            return text;
        }

    }

    // Lap class stores all laps as objects that can be put into lists

    public class Lap
    {
        public int LapNum { get; set; }
        public float Sector1 { get; set; }
        public bool Sector1IsValid { get; set; }
        public float Sector2 { get; set; }
        public bool Sector2IsValid { get; set; }
        public float Sector3 { get; set; }
        public bool Sector3IsValid { get; set; }
        public float TotalLapTime { get; set; }
        public bool LapIsValid { get; set; }
        public bool Sec1IsGreen { get; set; }
        public bool Sec2IsGreen { get; set; }
        public bool Sec3IsGreen { get; set; }
        public bool TotalIsGreen { get; set; }

        public Lap(int lapNum, float totalLapTime, bool lapIsValid, float sector1, float sector2,
                   float sector3, bool sector1IsValid, bool sector2IsValid, bool sector3IsValid)
        {
            LapNum = lapNum;
            Sector1 = sector1;
            Sector2 = sector2;
            Sector3 = sector3;
            TotalLapTime = totalLapTime;
            Sector1IsValid = sector1IsValid;
            Sector2IsValid = sector2IsValid;
            Sector3IsValid = sector3IsValid;
            LapIsValid = lapIsValid;

            // These attributes are set to false by default
            // They should only change to true if they were previously a best (purple) record
            Sec1IsGreen = false;
            Sec2IsGreen = false;
            Sec3IsGreen = false;
            TotalIsGreen = false;
        }

    }
}