using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Perrinn424
{
    public class LapTimeTable : IEnumerable<LapTime>
    {
        private List<LapTime> laps;
        private readonly int sectorCount;
        private readonly int timeColumsCount;
        private int[] bestTimes;
        private List<int> improvedTimes;

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

        public IEnumerator<LapTime> GetEnumerator()
        {
            return laps.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal void IntToLapSector(int index, out int lap, out int sector)
        {
            lap = index / timeColumsCount;
            sector = index % timeColumsCount;
        }
        internal void LapSectorToIndex(int lap, int sector, out int index)
        {
            index = lap * timeColumsCount + sector;
        }
    } 
}
