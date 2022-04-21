//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

// SliderValueGroup: A slider with an associated label showing its value


using UnityEngine;
using UnityEngine.UI;


namespace VehiclePhysics.UI
{

public class SliderValueGroup : MonoBehaviour
	{
	public Slider slider;
	public Text text;
	public string format = "0.0";


	void OnEnable ()
		{
		if (slider != null)
			{
			slider.onValueChanged.AddListener(SliderChanged);
			SliderChanged(slider.value);
			}
		else
			{
			SliderChanged(0.0f);
			}
		}


	void OnDisable ()
		{
		if (slider != null)
			slider.onValueChanged.RemoveListener(SliderChanged);
		}


	void SliderChanged (float value)
		{
		if (text != null)
			text.text = value.ToString(format);
		}
	}
}