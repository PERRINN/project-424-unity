using UnityEngine;

namespace Perrinn424
{
    [CreateAssetMenu(fileName = "PerformanceBenchmarkData", menuName = "Perrinn 424/PerformanceBenchmarkData")]
    public class PerformanceBenchmarkData : ScriptableObject
    {
        public float frequency = 1f;
        public float[] samples;
    } 
}
