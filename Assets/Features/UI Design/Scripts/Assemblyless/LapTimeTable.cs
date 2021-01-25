using System;
using System.Collections.Generic;
using Project424;
using UnityEngine;
using VehiclePhysics.Timing;

namespace Perrinn424.UI
{
    public class LapTimeTable : MonoBehaviour
    {
        [SerializeField]
        private FormatCell normalFormat;
        [SerializeField]
        private FormatCell improvementFormat;
        [SerializeField]
        private FormatCell bestFormat;

        private LapTimer lapTimer;
        private Perrinn424.LapTimeTable table;

        [SerializeField]
        private LapRow lapUIPrefab;
        [SerializeField]
        private Transform rowParent;

        private List<LapRow> rowList;

        private void Awake()
        {
            rowList = new List<LapRow>();
            lapTimer = FindObjectOfType<LapTimer>();
            lapUIPrefab.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            //lapTimer.onLap += OnLap;
        }

        private void OnDisable()
        {
            //lapTimer.onLap -= OnLap;
        }

        private void OnLap(float lapTime, bool validBool, float[] sectors, bool[] validSectors)
        {
            AddLap(sectors);
        }

        internal void AddLap(float[] sectors)
        {
            if (table == null)
                table = new Perrinn424.LapTimeTable(sectors.Length);

            LapTime newLap = new LapTime(sectors);
            table.AddLap(newLap);

            LapRow newLapUI = GameObject.Instantiate(lapUIPrefab, rowParent);
            rowList.Add(newLapUI);
            newLapUI.gameObject.SetActive(true);

            Refresh();
        }

        private void Refresh()
        {
            for (int i = 0; i < rowList.Count; i++)
            {
                LapTime lap = table[i];
                LapRow row = rowList[i];
                row.Refresh(lap, normalFormat);
            }

            int[] improvedTimes = table.GetImprovedTimes();

            foreach (int improvedTimeIndex in improvedTimes)
            {
                table.IntToLapSector(improvedTimeIndex, out int lapIndex, out int sectorIndex);
                rowList[lapIndex].ApplyFormat(sectorIndex, improvementFormat);
            }

            int[] best = table.GetBestTimes();

            for (int i = 0; i < best.Length; i++)
            {
                int lapIndex = best[i];
                rowList[lapIndex].ApplyFormat(i, bestFormat);
            }
        }
    } 
}
