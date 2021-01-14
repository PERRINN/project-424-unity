using UnityEngine;
using EdyCommonTools;
using VehiclePhysics.Timing;

namespace Perrinn424
{
    public class TimeDiff919IDRUI : MonoBehaviour
    {
        public GUITextBox.Settings overlay = new GUITextBox.Settings();

        // Trick to apply a default font to the telemetry box. Configure it at the script settings.

        [HideInInspector]
        public Font defaultFont;

        GUITextBox textBox = new GUITextBox();

        private static LapTimer lapTime;

        private TimeReference porsche = TimeReferenceHelper.CreatePorsche();
        private TimeReference volkswagen = TimeReferenceHelper.CreateVolkswagen();

        void OnEnable()
        {
            InitTextBox();
            GetLapTime();
        }

        private void InitTextBox()
        {
            textBox.settings = overlay;
            textBox.header = "Lap time comparison";

            if (overlay.font == null)
                overlay.font = defaultFont;
        }

        private void GetLapTime()
        {
            lapTime = (LapTimer) FindObjectOfType(typeof(LapTimer));
            if (lapTime == null)
            {
                enabled = false;
                return;
            }
        }


        // Update is called once per frame
        void Update()
        {
            float currentLapTime = lapTime.currentLapTime;
            float currentLapDistance = Project424.Telemetry424.m_lapDistance;

            float porscheDiff = porsche.LapDiff(currentLapTime, currentLapDistance);
            float vwDiff = volkswagen.LapDiff(currentLapTime, currentLapDistance);

            string text = $"Time difference /919    {porscheDiff,6:0.00}\n";
            text += $"Time difference /IDR    {vwDiff,6:0.00}";

            textBox.UpdateText(text);
        }

        void OnGUI()
        {
            textBox.OnGUI();
        }

    }
}
