using Perrinn424.Utilities;
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
        public TimeFormatter timeFormatter;

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
            background.sprite = format.backgroundSprite;
        }

        private void Reset()
        {
            timeFormatter = new TimeFormatter(TimeFormatter.Mode.TotalSeconds, @"m\:ss\.fff", @"ss\.fff");
        }
    }
}
