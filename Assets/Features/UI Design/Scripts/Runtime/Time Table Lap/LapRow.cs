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

        public void Refresh(string title, LapTime lap, FormatCell format)
        {
            timeCells[0].SetText(title);
            timeCells[0].ApplyFormat(format);
            for (int i = 0; i < lap.TimesCount; i++)
            {
                timeCells[i+1].SetTime(lap[i]);
                timeCells[i+1].ApplyFormat(format);
            }
        }

        public void ApplyFormat(int cellIndex, FormatCell format)
        {
            timeCells[cellIndex+1].ApplyFormat(format);
        }
    } 
}
