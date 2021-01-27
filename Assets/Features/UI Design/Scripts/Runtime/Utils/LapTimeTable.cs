using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Perrinn424
{
    public class LapTimeTable : IEnumerable<LapTime>
    {
        private readonly List<LapTime> laps;
        private readonly int sectorCount;
        private readonly int timeColumsCount;

        public int LapCount => laps.Count;

        /// <summary>
        /// Contains the best lap of each sector
        /// For example bestTimes[2] => 1 means that in sector 2, the best lap is 1
        /// </summary>
        private readonly int[] bestTimes;

        /// <summary>
        /// Improved times contains a list of index. These indices are in colunm-row format. This mean that, with one int, we could get column and row
        /// </summary>
        /// <example>
        /// If column width = 4 and the index = 11, it reference to a row = 2, column = 3
        /// 11/4 = 2
        /// 11%4 = 3
        /// 0 1 2 3
        /// 4 5 6 7
        /// 8 9 10 [11] 
        /// </example>
        private readonly List<int> improvedTimes;

        private bool isDirty;

        public LapTimeTable(int sectorCount)
        {
            this.sectorCount = sectorCount;
            this.timeColumsCount = sectorCount + 1;
            laps = new List<LapTime>();

            bestTimes = new int[timeColumsCount];
            improvedTimes = new List<int>();
        }

        public void AddLap(IEnumerable<float> sectors)
        {
            float[] sectorsTime = sectors.ToArray();

            AddLap(new LapTime(sectorsTime));
        }

        public void AddLap(LapTime newLap)
        {
            if (newLap.sectorCount != sectorCount)
                throw new ArgumentException($"Lap must have exactly {sectorCount} sectors");

            laps.Add(newLap);

            isDirty = true;
        }

        public int [] GetBestTimes()
        {
            CalculateBest();
            return bestTimes;
        }

        public int[] GetImprovedTimes()
        {
            CalculateBest();
            return improvedTimes.ToArray();
        }

        public int GetBestLap()
        {
            CalculateBest();
            return bestTimes[timeColumsCount-1];
        }

        private void CalculateBest()
        {
            if(!isDirty)
                return;

            improvedTimes.Clear();

            //Assume that the best sector is in lap 0
            for (int sectorIndex = 0; sectorIndex < timeColumsCount; sectorIndex++)
            {
                bestTimes[sectorIndex] = 0;
            }

            for (int lapIndex = 1; lapIndex < laps.Count; lapIndex++)
            {
                for (int sectorIndex = 0; sectorIndex < timeColumsCount; sectorIndex++)
                {
                    int minLapIndex = bestTimes[sectorIndex];
                    float minTime = laps[minLapIndex][sectorIndex];

                    float currentTime = laps[lapIndex][sectorIndex];
                    if (currentTime < minTime)
                    {
                        bestTimes[sectorIndex] = lapIndex;
                        LapSectorToIndex(lapIndex, sectorIndex, out int globalIndex);
                        improvedTimes.Add(globalIndex);
                    }
                }
            }

            isDirty = false;
        }

        public LapTime this[int i] => laps[i];


        public IEnumerator<LapTime> GetEnumerator()
        {
            return laps.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void IntToLapSector(int index, out int lap, out int sector)
        {
            lap = index / timeColumsCount;
            sector = index % timeColumsCount;
        }
        public void LapSectorToIndex(int lap, int sector, out int index)
        {
            index = lap * timeColumsCount + sector;
        }
    } 
}
