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
        private int[] bestSectors;
        private List<int> improvementSectors;
        public LapTimeTable(int sectorCount)
        {
            this.sectorCount = sectorCount;
            laps = new List<LapTime>();

            bestSectors = new int[sectorCount];
            improvementSectors = new List<int>();
        }

        public void AddLap(IEnumerable<float> sectors)
        {
            float[] sectorsTime = sectors.ToArray();

            if(sectorsTime.Length != sectorCount)
                throw new ArgumentException($"Lap must have exactly {sectorCount} sectors");

            laps.Add(new LapTime(sectorsTime));
        }

        public void AddLap(LapTime newLap)
        {
            if (newLap.sectorCount != sectorCount)
                throw new ArgumentException($"Lap must have exactly {sectorCount} sectors");

            laps.Add(newLap);
        }

        

        public int [] GetBest()
        {
            CalculateBest();
            return bestSectors;
        }

        public int[] GetImprovements()
        {
            CalculateBest();
            return improvementSectors.ToArray();
        }

        private void CalculateBest()
        {
            improvementSectors.Clear();

            //Assume that the best sector is in lap 0
            for (int sectorIndex = 0; sectorIndex < sectorCount; sectorIndex++)
            {
                bestSectors[sectorIndex] = 0;
            }

            for (int lapIndex = 1; lapIndex < laps.Count; lapIndex++)
            {
                for (int sectorIndex = 0; sectorIndex < sectorCount; sectorIndex++)
                {
                    int minLapIndex = bestSectors[sectorIndex];
                    float minTime = laps[minLapIndex][sectorIndex];

                    float currentTime = laps[lapIndex][sectorIndex];
                    if (currentTime < minTime)
                    {
                        bestSectors[sectorIndex] = lapIndex;
                        LapSectorToIndex(lapIndex, sectorIndex, out int globalIndex);
                        improvementSectors.Add(globalIndex);
                    }
                }
            }
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
            lap = index / sectorCount;
            sector = index % sectorCount;
        }
        internal void LapSectorToIndex(int lap, int sector, out int index)
        {
            index = lap * sectorCount + sector;
        }
    } 
}
