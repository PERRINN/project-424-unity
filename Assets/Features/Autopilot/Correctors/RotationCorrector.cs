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

            float error = YawError(rb.transform.rotation, targetRotation);

            PID.input = error;
            PID.Compute();

            rb.AddRelativeTorque(0f, PID.output, 0f);
        }

        public static float YawError(Quaternion rotation, Quaternion targetRotation)
        {
            return YawError(rotation.eulerAngles.y, targetRotation.eulerAngles.y);
        }

        public static float YawError(float yaw, float targetYaw)
        {
            float error = yaw - targetYaw;
            error = MathUtility.ClampAngle(error);
            return error;
        }
    } 
}
