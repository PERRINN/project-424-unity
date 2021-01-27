using System;
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

        public void SetTime(float seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);

            string format = time.Minutes > 0 ? @"m\:ss\:fff" : @"ss\:fff";
            string str = time.ToString(format);
            SetText(str);
        }

        public void SetTime(float seconds, string format)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            string str = time.ToString(format);
            SetText(str);
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
    } 
}
