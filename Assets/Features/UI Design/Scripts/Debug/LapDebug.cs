using System.Collections;
using UnityEngine;

namespace Perrinn424.UI.Debug
{
    public class LapDebug : MonoBehaviour
    {
        public LapTimeTable tapTimeTable;
        public float waitTime = 2f;

        private IEnumerator Start()
        {
            //yield break;
            yield return new WaitForSeconds(waitTime);

            foreach (float sector in GetSectorsFromLaps())
            {
                tapTimeTable.AddSector(sector);
                yield return new WaitForSeconds(waitTime);
            }
        }

        private void Update()
        {
            tapTimeTable.UpdateRollingTime(Time.time, Time.time * 2f);
        }

        IEnumerable GetLaps()
        {
            yield return new[] { 116.736f, 27.610f, 26.938f}; //lap 1
            yield return new[] { 26.726f, 27.610f, 26.938f };//lap 2
            yield return new[] { 26.730f, 27.610f, 26.936f };//lap 3
            yield return new[] { 26.728f, 27.606f, 26.934f };//lap 4
            yield return new[] { 26.730f, 27.610f, 26.938f };//lap 5
            yield return new[] { 26.734f, 27.614f, 26.936f };//lap 6
            yield return new[] { 26.732f, 27.604f, 26.940f };//lap 7
            yield return new[] { 26.738f, 27.608f, 26.940f };//lap 8
            yield return new[] { 26.726f, 27.608f, 26.934f };//lap 9
            yield return new[] { 26.736f, 27.602f, 26.934f };//lap 10
        }

        IEnumerable GetSectors()
        {
            yield return 20f; //lap 11, sector 1
            yield return 22f; //lap 11, sector 2
            yield return 24f; //lap 11, sector 3

            yield return 10f; //lap 12, sector 1
            yield return 32f; //lap 12, sector 2
            yield return 50f; //lap 12, sector 3
        }

        IEnumerable GetSectorsFromLaps()
        {
            foreach (float [] lap in GetLaps())
            {
                foreach (float sector in lap)
                {
                    yield return sector;
                }
            }
        }
    } 
}
