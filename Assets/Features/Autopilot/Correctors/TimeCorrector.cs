using System;
using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    [Serializable]
    public class TimeCorrector : Corrector
    {
        private const float errorThreshold = 10f;
        public void Correct(float targetTime, float currentTime)
        {
            UpdatePIDSettings();

            Error =  targetTime - currentTime;
            if (Mathf.Abs(Error) > errorThreshold)
                return;

            PID.input = Error;
            PID.Compute();
            Vector3 localForce = Vector3.forward * PID.output;

            Force = rb.transform.TransformVector(localForce);

            rb.AddForce(Force);
        }
    } 
}
