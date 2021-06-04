using UnityEngine;
using VehiclePhysics;


namespace Perrinn424
{
    public abstract class VehicleYield : CustomYieldInstruction
    {
        protected VehicleBase vehicle;
        protected VehicleYield(VehicleBase vehicle)
        {
            this.vehicle = vehicle;
        }
    }
} 

