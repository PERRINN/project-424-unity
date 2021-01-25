using UnityEngine;

namespace Perrinn424.UI
{
    public class LapRow : MonoBehaviour
    {
        [SerializeField]
        private TimeCell [] timeCells;

        private void Reset()
        {
            timeCells = this.GetComponentsInChildren<TimeCell>();
        }

        public void Refresh(LapTime lap, FormatCell format)
        {
            for (int i = 0; i < lap.TimesCount; i++)
            {
                timeCells[i].SetTime(lap[i]);
                timeCells[i].ApplyFormat(format);
            }
        }

        public void ApplyFormat(int cellIndex, FormatCell format)
        {
            timeCells[cellIndex].ApplyFormat(format);
        }
    } 
}
