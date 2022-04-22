//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

// CommonUiTools: utility methods


using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;


namespace VehiclePhysics.UI
{

public static class UITools
	{
	// Add/remove OnClick listener to Button

	public static void AddListener (Button button, UnityAction method)
		{
		if (button != null) button.onClick.AddListener(method);
		}

	public static void RemoveListener (Button button, UnityAction method)
		{
		if (button != null) button.onClick.RemoveListener(method);
		}

	// Set text in Text field

	public static void SetText (Text text, string textString)
		{
		if (text != null) text.text = textString;
		}

	// Set image fill in Image

	public static void SetImageFill (Image image, float fill)
		{
		if (image != null) image.fillAmount = fill;
		}

	// Show/hide any element

	public static void SetVisible (Behaviour element, bool enabled)
		{
		if (element != null) element.enabled = enabled;
		}

	// Enable/disable interactions

	public static void EnableRaycaster (GameObject gameObject)
		{
		GraphicRaycaster rayCaster = gameObject.GetComponent<GraphicRaycaster>();
		if (rayCaster != null) rayCaster.enabled = true;
		}

	public static void DisableRaycaster (GameObject gameObject)
		{
		GraphicRaycaster rayCaster = gameObject.GetComponent<GraphicRaycaster>();
		if (rayCaster != null) rayCaster.enabled = false;
		}

	// Enable/disable objects

	public static void Enable (GameObject gameObject)
		{
		if (gameObject != null) gameObject.SetActive(true);
		}

	public static void Enable (Behaviour element)
		{
		if (element != null) element.gameObject.SetActive(true);
		}

	public static void Disable (GameObject gameObject)
		{
		if (gameObject != null) gameObject.SetActive(false);
		}

	public static void Disable (Behaviour element)
		{
		if (element != null) element.gameObject.SetActive(false);
		}

	public static bool IsEnabled (GameObject gameObject)
		{
		return gameObject != null? gameObject.activeInHierarchy : false;
		}

	public static bool IsEnabled (Behaviour element)
		{
		return element != null? element.isActiveAndEnabled : false;
		}

	// Get/Set value in a slider

	public static float GetValue (Slider slider, float defaultValue = 0.0f)
		{
		return slider != null? slider.value : defaultValue;
		}

	public static void SetValue (Slider slider, float value)
		{
		if (slider != null) slider.value = value;
		}

	//----------------------------------------------------------------------------------------------
	// Groups and helpers


	// Add/remove OnClick listener to Button

	public static void AddListener (ButtonLabelGroup group, UnityAction method)
		{
		if (group != null && group.button != null)
			group.button.onClick.AddListener(method);
		}

	public static void RemoveListener (ButtonLabelGroup group, UnityAction method)
		{
		if (group != null && group.button != null)
			group.button.onClick.RemoveListener(method);
		}

	// Set text in Text field

	public static void SetText (ButtonLabelGroup group, string text)
		{
		if (group != null && group.label != null)
			group.label.text = text;
		}

	// Get/Set value in a slider

	public static float GetValue (SliderValueGroup group, float defaultValue = 0.0f)
		{
		if (group != null && group.slider != null)
			return group.slider.value;
		else
			return defaultValue;
		}

	public static void SetValue (SliderValueGroup group, float value)
		{
		if (group != null && group.slider != null)
			group.slider.value = value;
		}
	}

}
