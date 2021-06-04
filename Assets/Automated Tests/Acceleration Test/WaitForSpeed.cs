using VehiclePhysics;

namespace Perrinn424
{
    public class WaitForSpeed : VehicleYield
    {
        private readonly float speed;

        public WaitForSpeed(VehicleBase vehicle, float speed) : base(vehicle)
        {
            this.speed = speed;
        }

        public override bool keepWaiting
        {
            get
            {
                return vehicle.speed < speed;
            }
        }
    } 
}