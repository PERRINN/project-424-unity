using System;
using UnityEngine;

namespace Perrinn424.LapFileSystem
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
                Debug.Log("YES");
                remainingTime = 1f / frequency;
                return true;
            }

            Debug.Log("NO");
            return false;
        }

        public void Reset()
        {
            remainingTime = -1f;
        }

        public static implicit operator int(Frequency f) => f.frequency;
    } 
}
