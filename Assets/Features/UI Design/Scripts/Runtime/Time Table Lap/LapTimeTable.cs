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
        private LapRow lapUIPrefab = default;
        [SerializeField]
        private Transform rowParent = default;

        [SerializeField]
        private ScrollRect scrollRect = default;

        [SerializeField]
        private int sectorCount = 3;

        private List<LapRow> rowList;
        private Perrinn424.LapTimeTable table;

        private void Awake()
        {
            table = new Perrinn424.LapTimeTable(sectorCount);
            rowList = new List<LapRow>();
            lapUIPrefab.gameObject.SetActive(false);
        }

        public void AddLap(float[] sectors)
        {
            LapTime newLap = new LapTime(sectors);
            table.AddLap(newLap);

            AddRow();

            Refresh();
            StartCoroutine(RefreshScroll());
        }

        public void AddSector(float sector)
        {
            bool newLapAdded = table.AddSector(sector);

            if (newLapAdded)
            {
                AddRow();
            }

            Refresh();
            StartCoroutine(RefreshScroll());
        }

        private IEnumerator RefreshScroll()
        {
            yield return null;
            scrollRect.verticalNormalizedPosition = 0f;
        }

        private void AddRow()
        {
            LapRow newLapUI = Instantiate(lapUIPrefab, rowParent);
            rowList.Add(newLapUI);
            newLapUI.gameObject.SetActive(true);
        }

        private void Refresh()
        {
            RefreshTimes();

            if (table.LapCount < 2)
                return;

            RefreshImprovements();

            RefreshBestSectors();
        }

        private void RefreshTimes()
        {
            for (int i = 0; i < table.LapCount; i++)
            {
                LapTime lap = table[i];
                LapRow rowUI = rowList[i];
                rowUI.Refresh($"Lap {i + 1}", lap, normalFormat);
            }
        }

        private void RefreshImprovements()
        {
            int[] improvedTimes = table.GetImprovedTimes();

            foreach (int improvedTimeIndex in improvedTimes)
            {
                table.IndexToLapSector(improvedTimeIndex, out int lapIndex, out int sectorIndex);
                rowList[lapIndex].ApplyFormat(sectorIndex, improvementFormat);
            }
        }

        private void RefreshBestSectors()
        {
            int[] bestSectors = table.GetBestLapForEachSector();

            for (int i = 0; i < bestSectors.Length; i++)
            {
                int lapIndex = bestSectors[i];
                rowList[lapIndex].ApplyFormat(i, bestFormat);
            }
        }
    } 
}
