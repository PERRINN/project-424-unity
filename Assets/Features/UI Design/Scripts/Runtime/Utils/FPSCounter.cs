using UnityEngine;

namespace Perrinn424
{
    public class FPSCounter
    {
        public float Current { get; private set; }
        public float refreshRate = 1.0f;


        private int count;
        private float timer;


        public FPSCounter ()
        {
            count = 0;
            timer = Time.unscaledTime;
        }


        public void Update()
        {
            count++;
            if (Time.unscaledTime > timer + refreshRate)
            {
                Current = count / refreshRate;
                count = 0;
                timer = Time.unscaledTime;
            }
        }
    }
}
