using UnityEngine;
using UnityEngine.UI;
using VehiclePhysics;
using VehiclePhysics.Timing;

namespace Perrinn424.UI
{
    public class TimeLapDifference : MonoBehaviour
    {
        [SerializeField]
        private VehicleBase vehicle;

        [SerializeField]
        private Text electricRecord = default;
        [SerializeField]
        private Text overallRecord = default;

        private TimeDiff919 diff = new TimeDiff919();

        private string format = "+0.0 s;-0.0 s;0.0 s";

        private void Update()
        {
            Telemetry.DataRow latestTelemetry = vehicle.telemetry.latest;
            diff.Update((float)latestTelemetry.time, (float)latestTelemetry.distance);

            electricRecord.text = diff.VolkswagenDiff.ToString(format);
            overallRecord.text = diff.PorscheDiff.ToString(format);
        }
    }
}
