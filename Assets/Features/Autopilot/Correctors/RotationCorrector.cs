using EdyCommonTools;
using System;
using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    [Serializable]
    public class RotationCorrector : Corrector
    {
        public void Correct(Quaternion targetRotation)
        {
            UpdatePIDSettings();

            float yaw = rb.transform.rotation.eulerAngles.y;
            float targetYaw = targetRotation.eulerAngles.y;
            float error = yaw - targetYaw;
            error = MathUtility.ClampAngle(error);

            PID.input = error;
            PID.Compute();

            rb.AddRelativeTorque(0f, PID.output, 0f);
        }
    } 
}
