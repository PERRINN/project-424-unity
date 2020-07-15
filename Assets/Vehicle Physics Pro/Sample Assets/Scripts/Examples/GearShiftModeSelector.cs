//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;


namespace VehiclePhysics.Examples
{

public class GearShiftModeSelector : VehicleBehaviour
    {
	public enum GearShiftMode { Manual, Automatic }
	public GearShiftMode mode = GearShiftMode.Manual;

	Gearbox.Settings m_gearboxSettings;

	public override void OnEnableVehicle ()
		{
		// Retrieve a reference to the gearbox settings

		var gearbox = vehicle.GetInternalObject(typeof(Gearbox)) as Gearbox;
		if (gearbox == null)
			{
			// This vehicle doesn't have a gearbox. Self-disable this add-on.
			enabled = false;
			}

		m_gearboxSettings = gearbox.settings;
		}

    public override void FixedUpdateVehicle ()
        {
		if (m_gearboxSettings.type == Gearbox.Type.Automatic)
			{
			// Automatic Transmission. Select the proper mode.

			if (mode == GearShiftMode.Automatic)
				{
				// Force select mode D (4) unless current mode is R (2)

				if (vehicle.data.Get(Channel.Input, InputData.AutomaticGear) != 2)
					vehicle.data.Set(Channel.Input, InputData.AutomaticGear, 4);
				}
			else
				{
				// Force select mode M (0)

				vehicle.data.Set(Channel.Input, InputData.AutomaticGear, 0);
				}
			}
		else
			{
			// Manual gearbox. Just match the auto-shift mode.

			m_gearboxSettings.autoShift = mode == GearShiftMode.Automatic;
			}
        }
    }
}