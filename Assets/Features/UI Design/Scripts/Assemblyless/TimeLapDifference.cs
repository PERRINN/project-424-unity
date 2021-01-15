using UnityEngine;
using UnityEngine.UI;
using VehiclePhysics.Timing;

namespace Perrinn424.UI
{
    public class TimeLapDifference : MonoBehaviour
    {
        [SerializeField]
        private LapTimer lapTime;

        [SerializeField]
        private Text electricRecord;
        [SerializeField]
        private Text overallRecord;

        private TimeDiff919 diff = new TimeDiff919();

        private void Update()
        {
            float currentLapTime = lapTime.currentLapTime;
            float currentLapDistance = Project424.Telemetry424.m_lapDistance;

            diff.Update(currentLapTime, currentLapDistance);

            electricRecord.text = $"electric record {diff.VolkswagenDiff:+0.000;-0.000;0.000}";
            overallRecord.text = $"overall record {diff.PorscheDiff::+0.000;-0.000;0.000}";
        }
    } 
}
