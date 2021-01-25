using System.Collections;
using UnityEngine;

namespace Perrinn424.UI
{
    public class LapDebug : MonoBehaviour
    {
        public LapTimeTable tapTimeTable;

        private IEnumerator Start()
        {
            IEnumerator getSectors = GetSector();

            while (getSectors.MoveNext())
            {
                yield return new WaitForSeconds(1f);
                float [] sectors = (float[])getSectors.Current;
                tapTimeTable.AddLap(sectors);
            }
        }

        IEnumerator GetSector()
        {
            yield return new[] {26.736f, 27.610f, 26.938f};
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
    } 
}
