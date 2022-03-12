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

            //float angleError = Quaternion.Angle(rb.transform.rotation, targetRotation);
            pid.input = error;
            pid.Compute();
            //Vector3 localForce = pid.output *Vector3.right;
            //Force = rb.transform.TransformDirection(localForce);
            //rb.AddForceAtPosition(Force, rb.transform.position);

            rb.AddRelativeTorque(0f, pid.output, 0f);

            //DebugGraph.Log("Error", error);
            ////DebugGraph.Log("LocalForce", localForce);
            //DebugGraph.Log("PIDOutput", pid.output);

            //Quaternion rotationError = Quaternion.Inverse(targetRotation) * rb.rotation;
            //rb.AddTorque(rotationError.eulerAngles);
        }
    } 
}
