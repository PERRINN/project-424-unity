namespace Perrinn424.SpeedEstimatorSystem
{
    public struct MLSpeedEstimatorInput
    {
        public float throttle;
        public float brake;
        public float accelerationLateral;
        public float accelerationLongitudinal;
        public float accelerationVertical;
        public float nWheelFL;
        public float nWheelFR;
        public float nWheelRL;
        public float nWheelRR;
        public float steeringAngle;

        public static int count = 10;
    } 
}
