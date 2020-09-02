using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UpdateTextValues : MonoBehaviour
{
    public Slider Slider;
    public Text RPMS;
    public Text torque;
    public Slider torqueSlider;

    void Start()
    {
        RPMS = GetComponent<Text>();
        ShowSliderValue();
    }

    public void ShowSliderValue()
    {
        string sliderMessage = Slider.value.ToString();
        string tsliderMessage = torqueSlider.value.ToString();
        torque.text = tsliderMessage;
        RPMS.text = sliderMessage;
    }
}