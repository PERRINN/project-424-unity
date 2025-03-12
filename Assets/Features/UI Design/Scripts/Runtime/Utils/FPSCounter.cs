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
            float t = Time.unscaledTime;
            if (t > timer + refreshRate)
            {
                Current = count / (t - timer);
                count = 0;
                timer = t;
            }
        }
    }
}
