using Perrinn424.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Perrinn424.UI
{
    public class TimeCell : MonoBehaviour
    {
        [SerializeField]
        private Text text = default;
        [SerializeField]
        private Image background = default;

        [SerializeField]
        internal TimeFormatter timeFormatter;

        public void SetTime(float seconds)
        {
            SetText(timeFormatter.ToString(seconds));
        }

        public void SetText(string newText)
        {
            text.text = newText;
        }

        public void ApplyFormat(FormatCell format)
        {
            text.color = format.textColor;
            background.color = format.backgroundColor;
        }

        private void Reset()
        {
            timeFormatter = new TimeFormatter(TimeFormatter.Mode.TotalSeconds, @"m\:ss\.fff", @"ss\.fff");
        }
    }
}
