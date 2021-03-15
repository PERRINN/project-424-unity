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
            //if (float.IsPositiveInfinity(seconds))
            //{
            //    SetText(string.Empty);
            //    return;
            //}

            //if (minuteFormat)
            //{
            //    string format = seconds > 60f ? @"m\:ss\:fff" : @"ss\:fff";
            //    SetTime(seconds, format);
            //}
            //else
            //{
            //    SetText(seconds.ToString("N3"));
            //}
        }

        //public void SetTime(float seconds, string format)
        //{
        //    //if (float.IsPositiveInfinity(seconds))
        //    //{
        //    //    SetText(string.Empty);
        //    //    return;
        //    //}
        //    //TimeSpan time = TimeSpan.FromSeconds(seconds);
        //    //string str = time.ToString(format);
        //    //SetText(str);
        //}

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
