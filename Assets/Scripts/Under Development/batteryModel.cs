using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VehiclePhysics;
using VehiclePhysics.UI;

public class batteryModel : MonoBehaviour
{
    public VehicleBase vehicle;

    // public Vector2 position = new Vector2(8, 8);
    // public Font font;
    // [Range(6, 100)]
    // public int fontSize = 17;
    // public Color fontColor = Color.white;
    //
    string m_text = "";
    GUIStyle m_textStyle = new GUIStyle();
    float m_boxWidth;
    float m_boxHeight;

    public static float batteryCapacity;
    float batteryTemprature;
    public static float batterySOC;
    float batteryDOD;
    float batteryVoltage;

    public static float frontPower;
    public static float rearPower;


    public static float powerTotal;
    //float elapsed = 0f;


    private void Start()
    {
        batteryCapacity = 55;
        batterySOC = batteryCapacity;
        batteryDOD = 0;

        // m_textStyle.font = font;
        // m_textStyle.fontSize = fontSize;
        // m_textStyle.normal.textColor = fontColor;
    }

    private void FixedUpdate()
    {

        int[] custom = vehicle.data.Get(Channel.Custom);

        frontPower = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.ElectricalPower] / 1000.0f;
        rearPower = custom[Perrinn424Data.RearMguBase + Perrinn424Data.ElectricalPower] / 1000.0f;

        powerTotal = frontPower + rearPower;




        //elapsed += Time.deltaTime;
        //if (elapsed >= 1f)
        //{
        //    elapsed = elapsed % 1f;
        //    batteryCharge();
        //}

        batteryCharge();


        batterySOC = (batteryCapacity / 55) * 100;
        batteryDOD = 100 - batterySOC;

        //print("State of charge: " + batterySOC + "%");
        //print("Depth of discharge: " + batteryDOD + "%");

        //print(batteryCapacity);
        //print(powerTotal);



        SteeringScreen.batSOC = batterySOC;
        SteeringScreen.batCapacity = batteryCapacity;
    }

    void batteryCharge()
    {
        if (powerTotal > 0)
        {
            batteryCapacity -= ((powerTotal / 60) / 60) /500;
        }
        else if (powerTotal < 0)
        {
            batteryCapacity -= ((powerTotal / 60) / 60) / 500;
        }

        if (batteryCapacity < 0)
        {
            batteryCapacity = 0;
        }
    }

    void OnGUI()
    {
        // Compute box size

        //m_text = "Battery SOC (%): " + System.Math.Round(batterySOC) + "\n";
        //m_text += "Power used (kW): " + System.Math.Round(powerTotal) + "\n";
        //m_text += "Energy left (kWh): " + System.Math.Round(batteryCapacity) + "\n";

        //Vector2 contentSize = m_textStyle.CalcSize(new GUIContent(m_text));
        //float margin = m_textStyle.lineHeight * 1.2f;
        //float headerHeight = GUI.skin.box.lineHeight;

        //m_boxWidth = contentSize.x + margin;
        //m_boxHeight = contentSize.y + headerHeight + margin / 2;

        //// Compute box position

        //float xPos = position.x < 0 ? Screen.width + position.x - m_boxWidth : position.x;
        //float yPos = position.y < 0 ? Screen.height + position.y - m_boxHeight : position.y;

        //// Draw telemetry box

        //GUI.Box(new Rect(xPos, yPos, m_boxWidth, m_boxHeight), "424 Battery Model");
        //GUI.Label(new Rect(xPos + margin / 2, yPos + margin / 2 + headerHeight, Screen.width, Screen.height), m_text, m_textStyle);
    }

}
