using System;
using UnityEngine;

namespace Perrinn424.TelemetryLapSystem
{
    [Serializable]
    public class Frequency
    {
        [SerializeField]
        private int frequency;
        private float remainingTime;

        public bool Update(float dt)
        {
            remainingTime -= dt;

            if (remainingTime <= 0f)
            {
                remainingTime = 1f / frequency;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            remainingTime = -1f;
        }

        public static implicit operator int(Frequency f) => f.frequency;
    } 
}
