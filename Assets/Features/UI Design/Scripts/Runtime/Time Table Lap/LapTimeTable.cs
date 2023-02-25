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

        private List<LapRow> uiRowList;
        private Utilities.LapTimeTable timeTable;

        private void OnEnable()
        {
            CleanTable();

            timeTable = new Utilities.LapTimeTable(sectorCount);
            uiRowList = new List<LapRow>();
            lapUIPrefab.gameObject.SetActive(false);
            RefreshIdealLap();
        }

        private void CleanTable()
        {
            if (uiRowList != null && uiRowList.Count > 0)
            {
                foreach (LapRow row in uiRowList)
                {
                    GameObject.Destroy(row.gameObject);
                }
            }
        }

        public void AddLap(float[] sectors)
        {
            LapTime newLap = new LapTime(sectors);
            timeTable.AddLap(newLap);

            AddRow();

            Refresh();
        }

        public void AddSector(float sector)
        {
            timeTable.AddSector(sector);
            Refresh();
        }

        public void UpdateRollingTime(float sectorRollingTime, float lapRollingTime)
        {
            bool timeTableIsFull = timeTable.IsEmpty || timeTable[timeTable.LapCount - 1].IsCompleted;
            bool needUIRow = timeTable.LapCount == uiRowList.Count && timeTableIsFull;
            if (needUIRow)
            {
                AddRow();
            }

            int currentLapIndex = uiRowList.Count - 1;
            int currentSector = timeTableIsFull ? 0 : timeTable[currentLapIndex].SectorsCompletedIndex;

            LapRow currentRow = uiRowList[currentLapIndex];
            currentRow.Refresh(currentSector, sectorRollingTime, normalFormat);
            currentRow.Refresh(sectorCount, lapRollingTime, normalFormat);
        }

        private void AddRow()
        {
            LapRow newLapUI = Instantiate(lapUIPrefab, rowParent);
            uiRowList.Add(newLapUI);
            newLapUI.Refresh($"Lap {uiRowList.Count}", normalFormat, new LapTime(sectorCount), normalFormat);
            newLapUI.gameObject.SetActive(true);
            newLapUI.gameObject.name = $"Lap {uiRowList.Count}";
        }

        private void Refresh()
        {
            RefreshTimes();
            RefreshIdealLap();

            if (timeTable.LapCount < 2)
                return;

            RefreshImprovements();

            RefreshBestSectors();

            StartCoroutine(RefreshScroll());
        }


        private void RefreshTimes()
        {
            for (int i = 0; i < timeTable.LapCount; i++)
            {
                LapTime lap = timeTable[i];
                LapRow rowUI = uiRowList[i];
                rowUI.Refresh($"Lap {i + 1}", normalFormat, lap, normalFormat);
            }
        }

        private void RefreshImprovements()
        {
            int[] improvedTimes = timeTable.GetImprovedTimes();

            foreach (int improvedTimeIndex in improvedTimes)
            {
                timeTable.IndexToLapSector(improvedTimeIndex, out int lapIndex, out int sectorIndex);
                uiRowList[lapIndex].ApplyFormat(sectorIndex, improvementFormat);
            }
        }

        private void RefreshBestSectors()
        {
            int[] bestSectors = timeTable.GetBestLapForEachSector();

            //Special case.
            // Best sectors should appears only when there are others sectors to be compared
            //In lap 1, there are never two sectors to compare
            //From 3 on, there are always two sectors to compare
            //Lap 2 is a special case, so we need to track the current sector and draw until there
            int GetColumnMax()
            {
                int lapCount = timeTable.LapCount;
                LapTime lastLap = timeTable[lapCount - 1];
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
                uiRowList[lapIndex].ApplyFormat(i, bestFormat);
            }
        }

        private void RefreshIdealLap()
        {
            var idealLap = timeTable.GetIdealLap();
            idealLapRow.Refresh("Ideal Lap", normalFormat, idealLap, idealFormat);
        }

        private IEnumerator RefreshScroll()
        {
            yield return null;
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
