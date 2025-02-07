using Perrinn424.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Perrinn424.UI
{
    public class LapTimeTable : MonoBehaviour
    {
        [SerializeField]
        private FormatCell normalFormat = default;
        [SerializeField]
        private FormatCell improvementFormat = default;
        [SerializeField]
        private FormatCell bestFormat = default;
        [SerializeField]
        private FormatCell idealFormat = default;

        [SerializeField]
        private LapRow lapUIPrefab = default;
        [SerializeField]
        private Transform rowParent = default;

        [SerializeField]
        private ScrollRect scrollRect = default;

        [SerializeField]
        private int sectorCount = 3;

        [SerializeField]
        private LapRow idealLapRow;

        private List<LapRow> m_uiRowList;
        private Utilities.LapTimeTable m_timeTable;

        public Utilities.LapTimeTable timeTable => m_timeTable;

        private void OnEnable()
        {
            CleanTable();

            m_timeTable = new Utilities.LapTimeTable(sectorCount);
            m_uiRowList = new List<LapRow>();
            if (lapUIPrefab != null)
                lapUIPrefab.gameObject.SetActive(false);
            RefreshIdealLap();
        }

        private void CleanTable()
        {
            if (m_uiRowList != null && m_uiRowList.Count > 0)
            {
                foreach (LapRow row in m_uiRowList)
                {
                    GameObject.Destroy(row.gameObject);
                }
            }
        }

        public void AddLap(float[] sectors)
        {
            LapTime newLap = new LapTime(sectors);
            m_timeTable.AddLap(newLap);

            AddRow();

            Refresh();
        }

        public void AddSector(float sector)
        {
            m_timeTable.AddSector(sector);
            Refresh();
        }

        public void UpdateRollingTime(float sectorRollingTime, float lapRollingTime)
        {
            if (lapUIPrefab == null) return;

            bool timeTableIsFull = m_timeTable.IsEmpty || m_timeTable[m_timeTable.LapCount - 1].IsCompleted;
            bool needUIRow = m_timeTable.LapCount == m_uiRowList.Count && timeTableIsFull;
            if (needUIRow)
            {
                AddRow();
            }

            int currentLapIndex = m_uiRowList.Count - 1;
            int currentSector = timeTableIsFull ? 0 : m_timeTable[currentLapIndex].SectorsCompletedIndex;

            LapRow currentRow = m_uiRowList[currentLapIndex];
            currentRow.Refresh(currentSector, sectorRollingTime, normalFormat);
            currentRow.Refresh(sectorCount, lapRollingTime, normalFormat);
        }

        private void AddRow()
        {
            LapRow newLapUI = Instantiate(lapUIPrefab, rowParent);
            m_uiRowList.Add(newLapUI);
            newLapUI.Refresh($"Lap {m_uiRowList.Count}", normalFormat, new LapTime(sectorCount), normalFormat);
            newLapUI.gameObject.SetActive(true);
            newLapUI.gameObject.name = $"Lap {m_uiRowList.Count}";
        }

        private void Refresh()
        {
            RefreshTimes();
            RefreshIdealLap();

            if (m_timeTable.LapCount < 2)
                return;

            RefreshImprovements();

            RefreshBestSectors();

            StartCoroutine(RefreshScroll());
        }


        private void RefreshTimes()
        {
            for (int i = 0; i < m_uiRowList.Count; i++)
            {
                LapTime lap = m_timeTable[i];
                LapRow rowUI = m_uiRowList[i];
                rowUI.Refresh($"Lap {i + 1}", normalFormat, lap, normalFormat);
            }
        }

        private void RefreshImprovements()
        {
            int[] improvedTimes = m_timeTable.GetImprovedTimes();

            foreach (int improvedTimeIndex in improvedTimes)
            {
                m_timeTable.IndexToLapSector(improvedTimeIndex, out int lapIndex, out int sectorIndex);
                if (lapIndex < m_uiRowList.Count)
                    m_uiRowList[lapIndex].ApplyFormat(sectorIndex, improvementFormat);
            }
        }

        private void RefreshBestSectors()
        {
            int[] bestSectors = m_timeTable.GetBestLapForEachSector();

            //Special case.
            // Best sectors should appears only when there are others sectors to be compared
            //In lap 1, there are never two sectors to compare
            //From 3 on, there are always two sectors to compare
            //Lap 2 is a special case, so we need to track the current sector and draw until there
            int GetColumnMax()
            {
                int lapCount = m_timeTable.LapCount;
                LapTime lastLap = m_timeTable[lapCount - 1];
                if (lapCount == 2 && !lastLap.IsCompleted)
                {
                    return lastLap.SectorsCompletedIndex;
                }

                return bestSectors.Length;
            }

            int columnMax = GetColumnMax();

            for (int i = 0; i < columnMax; i++)
            {
                int lapIndex = bestSectors[i];
                if (lapIndex < m_uiRowList.Count)
                    m_uiRowList[lapIndex].ApplyFormat(i, bestFormat);
            }
        }

        private void RefreshIdealLap()
        {
            var idealLap = m_timeTable.GetIdealLap();
            idealLapRow.Refresh("Ideal Lap", normalFormat, idealLap, idealFormat);
        }

        private IEnumerator RefreshScroll()
        {
            if (scrollRect != null)
            {
                // Give enough frames for the UI to refresh its layout
                for (int i = 0; i < 5; i++)
                    yield return null;

                scrollRect.verticalNormalizedPosition = 0f;
            }
        }
    }
}
