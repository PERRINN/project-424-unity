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
        public LapTimeTable(int sectorCount)
        {
            this.sectorCount = sectorCount;
            laps = new List<LapTime>();
            bestSectors = new int[sectorCount];
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

        private void CalculateBest()
        {
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


    } 
}
