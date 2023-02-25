using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Perrinn424.Utilities
{
    [Serializable]
    public class RefreshHelper
    {
        public float refreshRate = 1;

        private float timeAccumulated;

        public bool Update(float dt)
        {
            timeAccumulated += dt;
            if (timeAccumulated > (1f / refreshRate))
            {
                timeAccumulated = 0f;
                return true;
            }

            return false;
        }
    } 
}
