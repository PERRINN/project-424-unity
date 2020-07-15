//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;


namespace VehiclePhysics.UI
{

public class SetableSlider : Slider
	{
	// Modify the value without calling the OnValueChanged callback

	public void SetValueWithoutCallback (float value)
		{
		Set(value, false);
		}

	// Configure a new range ensuring the OnValueChanged callback is not called.
	// For this, it ensures the current value to be within range before modifying it.

	public void SetRangeWithoutCallback (float min, float max)
		{
		if (value < min) Set(min, false);
		else if (value > max) Set(max, false);

		minValue = min;
		maxValue = max;
		}
	}

}