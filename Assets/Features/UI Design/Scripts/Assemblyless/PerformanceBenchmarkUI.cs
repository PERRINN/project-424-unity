using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Perrinn424.UI
{
    public class PerformanceBenchmarkUI : MonoBehaviour
    {

        [SerializeField]
        private Text electricRecord = default;
        [SerializeField]
        private Text overallRecord = default;

        [SerializeField]
        [FormerlySerializedAs("diff")]
        private PerformanceBenchmarkController performanceBenchmarkController;

        private string format = "+0.0 s;-0.0 s;0.0 s";

        private void Update()
        {
            electricRecord.text = performanceBenchmarkController.IDR.TimeDiff.ToString(format);
            overallRecord.text = performanceBenchmarkController.Porsche919.TimeDiff.ToString(format);
        }
    }
}
