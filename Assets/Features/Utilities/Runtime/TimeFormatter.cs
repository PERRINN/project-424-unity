using System;

namespace Perrinn424.Utilities
{
    [Serializable]
    public class TimeFormatter
    {
        public enum Mode
        {
            TotalSeconds,
            MinutesAndSeconds,
        }

        public Mode mode;
        public string formatWithMinutes = @"m\:ss\:fff";
        public string formatWithoutMinutes = @"ss\:fff";

        public TimeFormatter(Mode mode, string formatWithMinutes, string formatWithoutMinutes)
        {
            this.mode = mode;
            this.formatWithMinutes = formatWithMinutes;
            this.formatWithoutMinutes = formatWithoutMinutes;
        }

        public string ToString(float seconds)
        {
            if (float.IsPositiveInfinity(seconds))
            {
                return String.Empty;
            }
            if (mode == Mode.TotalSeconds)
            {
                return seconds.ToString("N3");
            }

            //mode MinutesAndSeconds
            string format = seconds > 60f ? formatWithMinutes : formatWithoutMinutes;
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            return time.ToString(format);
        }
    } 
}
