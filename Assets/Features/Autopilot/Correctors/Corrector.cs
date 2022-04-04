using EdyCommonTools;
using System;
using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    [Serializable]
    public abstract class Corrector
    {
        public bool enabled = true;
        public float kp;
        public float ki;
        public float kd;
        public float max;
        public PidController.ProportionalMode mode;

        protected Rigidbody rb;
        public PidController PID { get; private set; }

        public Vector3 Force { get; protected set; }
        public float Error { get; protected set; }

        public void Init(Rigidbody rb)
        {
            this.rb = rb;
            PID = new PidController();
        }
        protected void UpdatePIDSettings()
        {
            UpdatePIDSettings(kp, ki, kd);
        }

        protected void UpdatePIDSettings(float kp, float ki, float kd)
        {
            PID.SetParameters(kp, ki, kd);
            PID.limitedOutput = true;
            PID.maxOutput = enabled ? max : 0;
            PID.minOutput = enabled ? -max : 0;
            PID.proportionalMode = mode;
        }
    } 
}
