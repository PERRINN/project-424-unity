using Perrinn424.AutopilotSystem;
using Perrinn424.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Perrinn424.UI
{
    public class ReferenceTime : MonoBehaviour
    {
        [SerializeField]
        private Text text = default;

        [SerializeField]
        private AutopilotProvider autopilotProvider;

        private void OnEnable()
        {
            TimeFormatter timeFormatter = new TimeFormatter(TimeFormatter.Mode.MinutesAndSeconds, @"m\:ss\.fff", @"m\:ss\.fff");
            string duration = timeFormatter.ToString(autopilotProvider.CalculateDuration());
            text.text = $"Ref {duration}";

        }
    } 
}
