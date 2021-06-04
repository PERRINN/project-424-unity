using UnityEngine;
using VehiclePhysics;


namespace Perrinn424
{
    public class WaitForDistance : VehicleYield
    {
        private readonly float distance;
        public float TraveledDistance { get; private set; }

        public WaitForDistance(VehicleBase vehicle, float distance) : base(vehicle)
        {
            this.distance = distance;
        }

        public override bool keepWaiting
        {
            get
            {
                TraveledDistance += vehicle.speed * Time.deltaTime;
                return TraveledDistance < distance;
            }
        }
    } 
}
