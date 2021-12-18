using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Perrinn424.Utilities
{
    public class LapTimeTable : IEnumerable<LapTime>
    {
        private readonly List<LapTime> laps;
        private readonly int sectorCount;
        private readonly int timeColumsCount;

        private LapTime idealLap;

        public int LapCount => laps.Count;
        public bool IsEmpty => laps.Count == 0;

        /// <summary>
        /// Contains the best lap of each sector
        /// For example bestLapForEachSector[2] => 1 means that in sector 2, the best lap is 1
        /// </summary>
        private readonly int[] bestLapForEachSector;

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
        /// <see cref="IndexToLapSector"/>
        private readonly List<int> improvedTimes;

        private bool isDirty;

        public LapTimeTable(int sectorCount)
        {
            this.sectorCount = sectorCount;
            this.timeColumsCount = sectorCount + 1; //colum count = sector count (n) + total time (1)
            laps = new List<LapTime>();

            bestLapForEachSector = new int[timeColumsCount];
            improvedTimes = new List<int>();
            idealLap = new LapTime(sectorCount);
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

        public bool AddSector(float sector)
        {
            LapTime lastLap = laps.LastOrDefault();
            bool newLapNeeded = lastLap == null || lastLap.IsCompleted; //laps empty or last lap is completed
            if (newLapNeeded)
            {
                LapTime newLap = new LapTime(sectorCount, new []{sector});
                AddLap(newLap);
            }
            else
            {
                lastLap.AddSector(sector);
            }

            isDirty = true;
            return newLapNeeded;
        }

        public int [] GetBestLapForEachSector()
        {
            CalculateBest();
            return bestLapForEachSector;
        }

        public int[] GetImprovedTimes()
        {
            CalculateBest();
            return improvedTimes.ToArray();
        }

        public int GetBestLap()
        {
            CalculateBest();
            return bestLapForEachSector[timeColumsCount-1];
        }

        public LapTime GetIdealLap()
        {
            CalculateBest();
            return idealLap;
        }

        private void CalculateBest()
        {
            if(!isDirty)
                return;

            improvedTimes.Clear();

            //Assume that the best sector is in lap 0
            for (int sectorIndex = 0; sectorIndex < timeColumsCount; sectorIndex++)
            {
                bestLapForEachSector[sectorIndex] = 0;
            }

            //Calculate best sectors
            for (int lapIndex = 1; lapIndex < laps.Count; lapIndex++)
            {
                for (int sectorIndex = 0; sectorIndex < timeColumsCount; sectorIndex++)
                {
                    int indexOfLapWithMinimumTimeInThisSector = bestLapForEachSector[sectorIndex];
                    float minimumTimeInThisSector = laps[indexOfLapWithMinimumTimeInThisSector][sectorIndex];

                    float timeInThisLapInThisSector = laps[lapIndex][sectorIndex];
                    if (timeInThisLapInThisSector < minimumTimeInThisSector)
                    {
                        bestLapForEachSector[sectorIndex] = lapIndex;
                        LapSectorToIndex(lapIndex, sectorIndex, out int globalIndex);
                        improvedTimes.Add(globalIndex);
                    }
                }
            }


            //Calculate the ideal laps
            for (int sectorIndex = 0; sectorIndex < sectorCount; sectorIndex++)
            {
                int lapIndex = bestLapForEachSector[sectorIndex];
                float sectorTime = laps[lapIndex][sectorIndex];
                idealLap.UpdateSector(sectorIndex, sectorTime);
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

        public void IndexToLapSector(int index, out int lap, out int sector)
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
