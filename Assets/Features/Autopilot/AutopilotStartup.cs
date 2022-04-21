using System;
using UnityEngine;
using VehiclePhysics;

namespace Perrinn424.AutopilotSystem
{
    [Serializable]
    public class AutopilotStartup
    {
        public RotationCorrector rotationCorrector;

        [Range(0.1f, 1f)]
        public float speedPercentage;
        [Range(0f, 1f)]
        public float throttle;

        private VehicleBase vehicle;

        public bool isStartUp = true;
        public void Init(VehicleBase vehicle)
        {
            this.vehicle = vehicle;
            rotationCorrector.Init(vehicle.cachedRigidbody);
        }

        public bool IsStartup(float expectedSpeed)
        {
            float threshold = 0.20f;
            float speed = vehicle.speed;
            if (isStartUp && speed > expectedSpeed * (speedPercentage + threshold))
            {
                isStartUp = false;
            }
            else if (!isStartUp && speed < expectedSpeed * (speedPercentage))
            {
                isStartUp = true;
            }

            return isStartUp;
        }
        public Sample Correct(Sample sample)
        {

            rotationCorrector.Correct(sample.rotation);

            Sample startupSample = sample;
            startupSample.rawBrake = 0;
            startupSample.rawThrottle = (int)(10000*throttle);
            
            startupSample.brakePressure = 0;
            startupSample.throttle = (int)(100 * throttle);

            return startupSample;
        }
    } 
}
