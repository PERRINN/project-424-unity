using Perrinn424.AutopilotSystem;
using Perrinn424.Utilities;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Perrinn424.UI
{
    public class ReferenceInformation : MonoBehaviour
    {
        [SerializeField]
        [FormerlySerializedAs("text")]
        private Text time = default;

        [SerializeField]
        private Text date;

        [SerializeField]
        private BaseAutopilot autopilot;

        private void OnEnable()
        {
            TimeFormatter timeFormatter = new TimeFormatter(TimeFormatter.Mode.MinutesAndSeconds, @"m\:ss\.fff", @"m\:ss\.fff");
            string duration = timeFormatter.ToString(autopilot.Duration);
            time.text = $"Ref {duration}";

            date.text = "PlaceHolder";
        }
    } 
}
