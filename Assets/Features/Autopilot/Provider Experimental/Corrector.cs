using EdyCommonTools;
using System;
using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    [Serializable]
    public abstract class Corrector
    {
        public float kp;
        public float ki;
        public float kd;
        public float max;
        public PidController.ProportionalMode mode;

        protected Rigidbody rb;
        protected PidController pid;

        public Vector3 Force { get; protected set; }

        public void Init(Rigidbody rb)
        {
            this.rb = rb;
            pid = new PidController();
        }

        protected void UpdatePIDSettings()
        {
            pid.SetParameters(kp, ki, kd);
            pid.limitedOutput = true;
            pid.maxOutput = max;
            pid.minOutput = -max;
            pid.proportionalMode = mode;
        }
    } 
}
