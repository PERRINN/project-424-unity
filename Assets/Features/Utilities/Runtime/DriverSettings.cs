using System;

namespace Perrinn424.Utilities
{
    [Serializable]
    public class DriverSettings
    {
        public float height;
        public float fov;
        public bool damping = true;
        public int steeringWheelVisibility;
        public float miniDashboardPosition;
    }
}
