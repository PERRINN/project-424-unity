using System;
using UnityEngine;
using UnityEngine.UI;

namespace Perrinn424.UI
{
    public class TimeCell : MonoBehaviour
    {

        [SerializeField]
        private Text text;
        [SerializeField]
        private Image background;

        public void SetTime(float seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);

            //here backslash is must to tell that colon is
            //not the part of format, it just a character that we want in output
            string format = time.Minutes > 0 ? @"m\:ss\:fff" : @"ss\:fff";
            string str = time.ToString(format);
            text.text = str;
        }

        public void ApplyFormat(FormatCell format)
        {
            text.color = format.textColor;
            background.color = format.backgroundColor;
        }
    } 
}
