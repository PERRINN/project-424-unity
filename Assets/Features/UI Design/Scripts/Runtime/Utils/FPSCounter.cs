using UnityEngine;

namespace Perrinn424
{
    public class FPSCounter
    {
        public float Current { get; private set; }

        public void Update()
        {
            Current = 1f / Time.unscaledDeltaTime;
        }
    } 
}
