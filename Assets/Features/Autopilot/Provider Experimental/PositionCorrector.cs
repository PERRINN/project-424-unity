using System;
using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    [Serializable]
    public class PositionCorrector : Corrector
    {
        public Vector3 correctionAxis = Vector3.right;
        public void Correct(Vector3 targetPosition)
        {
            UpdatePIDSettings();

            Vector3 dir = targetPosition - rb.position;
            Vector3 localDir = rb.transform.InverseTransformDirection(dir);
            Vector3 errorVector = Vector3.Project(localDir, correctionAxis);
            float sign = Mathf.Sign(Vector3.Dot(errorVector, correctionAxis));
            Error = sign*errorVector.magnitude;

            PID.input = Error;
            PID.Compute();
            Vector3 localForce = -correctionAxis * PID.output;


            Force = rb.transform.TransformVector(localForce);
            //Force = distanceErrorDir * pid.output;
            //rb.AddForceAtPosition(Force, rb.transform.position);
            //rb.AddForce(Force);
            rb.AddForce(Force);

            //DebugGraph.Log("LocalPosition", localPosition);
            DebugGraph.Log("Error", errorVector);
            DebugGraph.Log("LocalForce", localForce);
            DebugGraph.Log("PIDOutput", PID.output);
        }

        //public void Correct(Vector3 targetPosition)
        //{

        //    Vector3 dir = targetPosition - rb.position;
        //    Vector3 localExperimental = rb.transform.InverseTransformDirection(Vector3.Project(dir, rb.transform.right));

            
            
        //    UpdatePIDSettings();

        //    Vector3 localPosition = rb.transform.InverseTransformPoint(targetPosition);
        //    float error = localPosition.x;
        //    //Vector3 distanceError = rb.transform.position - targetPosition;
        //    //distanceError = Vector3.ProjectOnPlane(distanceError, rb.transform.up);

        //    //float distanceErrorMagnitude = distanceError.magnitude;
        //    //Vector3 distanceErrorDir = distanceError.normalized;
        //    pid.input = error;
        //    pid.Compute();
        //    Vector3 localForce = Vector3.left * pid.output;


        //    Force = rb.transform.TransformVector(localForce);
        //    //Force = distanceErrorDir * pid.output;
        //    //rb.AddForceAtPosition(Force, rb.transform.position);
        //    //rb.AddForce(Force);
        //    rb.AddForce(Force);

        //    //DebugGraph.Log("LocalPosition", localPosition);
        //    DebugGraph.Log("Error", error);
        //    DebugGraph.Log("LocalForce", localForce);
        //    DebugGraph.Log("PIDOutput", pid.output);
        //}
    } 
}
