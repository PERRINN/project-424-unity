using System;
using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    [Serializable]
    public class TimeCorrector : Corrector
    {
        public Vector3 axis;
        public void Correct(float currentTime, float targetTime)
        {
            UpdatePIDSettings();

            Error =  currentTime - targetTime;
            if (Mathf.Abs(Error) > 10f)
                return;

            PID.input = Error;
            PID.Compute();
            Vector3 localForce = axis * PID.output;

            Force = rb.transform.TransformVector(localForce);

            rb.AddForce(Force);
        }
    } 
}
