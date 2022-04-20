using System;
using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    [Serializable]
    public class PositionCorrector : Corrector
    {
        public Vector3 correctionAxis = Vector3.right;
        public float maxForceP;
        public float maxForceD;



        public Vector3 localApplicationPosition;
        public Vector3 ApplicationPosition { get; private set; }

        public override float MaxForceP => maxForceP;

        public override float MaxForceD => maxForceD;

        private float previousUnsignedError;
        public void Correct(Vector3 targetPosition)
        {

            ApplicationPosition = rb.transform.TransformPoint(localApplicationPosition);

            Vector3 dir = targetPosition - ApplicationPosition;
            dir = Vector3.ProjectOnPlane(dir, rb.transform.up);
            Vector3 localDir = rb.transform.InverseTransformDirection(dir);
            Vector3 errorVector = Vector3.Project(localDir, correctionAxis);
            float sign = Mathf.Sign(Vector3.Dot(errorVector, correctionAxis));
            float unsignedError = errorVector.magnitude;
            Error = sign*unsignedError;

            UpdatePID(unsignedError);

            PIDController.input = Error;
            PIDController.Compute();
            Vector3 localForce = -correctionAxis * PIDController.output;

            Force = rb.transform.TransformVector(localForce);

            rb.AddForceAtPosition(Force, ApplicationPosition);

            previousUnsignedError = unsignedError;
        }

        private void UpdatePID(float unsignedError)
        {
            float kpTemp = Error == 0 ? kp : Mathf.Min(kp, maxForceP / unsignedError);
            float kdTemp = Mathf.Min(kd, maxForceD * Time.deltaTime / Mathf.Abs(unsignedError - previousUnsignedError));

            UpdatePIDSettings(kpTemp, ki, kdTemp);
        }
    } 
}
