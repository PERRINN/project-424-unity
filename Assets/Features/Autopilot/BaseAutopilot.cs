using System;
using VehiclePhysics;

namespace Perrinn424.AutopilotSystem
{
    public abstract class BaseAutopilot : VehicleBehaviour, IPIDInfo
    {
        public bool IsOn { get; private set; }
        public event Action<bool> OnStatusChanged;

        public abstract bool IsStartup { get; }

        public abstract float Error { get; }

        public abstract float P { get; }
        public abstract float I { get; }

        public abstract float D { get; }

        public abstract float PID { get; }

        public abstract float MaxForceP { get; }

        public abstract float MaxForceD { get; }

        public void ToggleStatus()
        {
            SetStatus(!IsOn);
        }

        protected virtual void SetStatus(bool isOn)
        {
            IsOn = isOn;
            OnStatusChanged?.Invoke(IsOn);
        }

        public abstract float CalculatePlayingTime();

        public abstract float CalculateDuration();
    } 
}
