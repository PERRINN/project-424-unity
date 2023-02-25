using EdyCommonTools;
using System;
using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    [Serializable]
    public abstract class Corrector : IPIDInfo
    {
        public bool enabled = true;
        public float kp;
        public float ki;
        public float kd;
        public float max;
        public PidController.ProportionalMode mode;

        protected Rigidbody rb;
        protected PidController PIDController { get; private set; }

        public Vector3 Force { get; protected set; }
        public float Error { get; protected set; }

        public float P => PIDController.proportional;

        public float I => PIDController.integral;

        public float D => PIDController.derivative;

        public virtual float MaxForceP => throw new NotImplementedException();

        public virtual float MaxForceD => throw new NotImplementedException();

        public float PID => PIDController.output;

        public void Init(Rigidbody rb)
        {
            this.rb = rb;
            PIDController = new PidController();
        }
        protected void UpdatePIDSettings()
        {
            UpdatePIDSettings(kp, ki, kd);
        }

        protected void UpdatePIDSettings(float kp, float ki, float kd)
        {
            PIDController.SetParameters(kp, ki, kd);
            PIDController.limitedOutput = true;
            PIDController.maxOutput = enabled ? max : 0;
            PIDController.minOutput = enabled ? -max : 0;
            PIDController.proportionalMode = mode;
        }
    } 
}
