using Perrinn424.AutopilotSystem;
using Perrinn424.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Perrinn424.UI
{
    public class ReferenceTime : MonoBehaviour
    {
        [SerializeField]
        private Text text = default;

        [SerializeField]
        private BaseAutopilot autopilot;

        private void OnEnable()
        {


            TimeFormatter timeFormatter = new TimeFormatter(TimeFormatter.Mode.MinutesAndSeconds, @"m\:ss\.fff", @"m\:ss\.fff");
            string duration = timeFormatter.ToString(autopilot.CalculateDuration());
            text.text = $"Ref {duration}";

        }
    } 
}
