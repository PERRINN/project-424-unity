using System;
using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    [Serializable]
    public class PositionCorrector : Corrector
    {
        public Vector3 correctionAxis = Vector3.right;
        public Vector3 localApplicationPosition;

        public Vector3 ApplicationPosition { get; private set; }
        public void Correct(Vector3 targetPosition)
        {
            UpdatePIDSettings();

            ApplicationPosition = rb.transform.TransformPoint(localApplicationPosition);

            Vector3 dir = targetPosition - ApplicationPosition;
            dir = Vector3.ProjectOnPlane(dir, rb.transform.up);
            Vector3 localDir = rb.transform.InverseTransformDirection(dir);
            Vector3 errorVector = Vector3.Project(localDir, correctionAxis);
            float sign = Mathf.Sign(Vector3.Dot(errorVector, correctionAxis));
            Error = sign*errorVector.magnitude;

            PID.input = Error;
            PID.Compute();
            Vector3 localForce = -correctionAxis * PID.output;

            Force = rb.transform.TransformVector(localForce);

            rb.AddForceAtPosition(Force, ApplicationPosition);
        }
    } 
}
