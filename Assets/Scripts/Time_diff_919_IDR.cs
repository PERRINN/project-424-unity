using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VehiclePhysics;
using EdyCommonTools;

namespace VehiclePhysics.Timing{
public class Time_diff_919_IDR : MonoBehaviour
{
  public GUITextBox.Settings overlay = new GUITextBox.Settings();

	// Trick to apply a default font to the telemetry box. Configure it at the script settings.
	[HideInInspector] public Font defaultFont;

	GUITextBox m_textBox = new GUITextBox();

  static LapTimer lapTime = new LapTimer();

    // Start is called before the first frame update
    public void Start()
    {
      m_textBox.settings = overlay;
      m_textBox.header = "Lap time comparison";

      if (overlay.font == null)
        overlay.font = defaultFont;
    }

    void OnEnable()
    {
        lapTime = (LapTimer)FindObjectOfType(typeof(LapTimer));
    }


    // Update is called once per frame
    void Update()
    {
      //Gather 919 and IDR data around Nuerburgring
      float[] PorscheTime = new float[321] ;
      int[] PorscheDistance = new int[321] {0,41,94,148,187,218,245,280,327,385,451,521,590,658,729,803,880,943,995,1039,1085,1140,1200,1251,1297,1338,1385,1439,1483,1523,1563,1614,1674,1745,1822,1903,1987,2072,2158,2238,2316,2392,2470,2550,2632,2716,2803,2892,2984,3078,3173,3268,3359,3447,3532,3613,3696,3779,3837,3881,3922,3963,4014,4077,4149,4228,4311,4395,4482,4570,4660,4751,4843,4926,4996,5067,5124,5179,5221,5252,5282,5316,5351,5397,5454,5523,5598,5677,5758,5843,5926,5997,6067,6141,6197,6241,6286,6339,6398,6459,6501,6537,6576,6626,6684,6748,6813,6880,6950,7013,7074,7138,7206,7278,7343,7393,7431,7458,7482,7515,7561,7618,7681,7749,7821,7890,7938,7979,8028,8085,8136,8179,8224,8279,8344,8416,8496,8578,8663,8749,8834,8898,8946,8986,9029,9082,9144,9215,9291,9372,9454,9537,9621,9706,9791,9877,9965,10054,10145,10240,10338,10435,10531,10625,10718,10809,10884,10956,11030,11106,11184,11263,11340,11417,11495,11562,11611,11646,11677,11712,11757,11809,11868,11931,11990,12032,12061,12087,12114,12140,12169,12205,12254,12314,12380,12451,12524,12599,12674,12752,12817,12877,12935,12993,13048,13097,13144,13200,13262,13329,13400,13470,13527,13584,13641,13700,13765,13818,13864,13915,13971,14027,14076,14127,14182,14233,14279,14327,14384,14443,14486,14528,14574,14629,14687,14734,14773,14814,14866,14925,14990,15060,15133,15209,15278,15341,15403,15466,15530,15595,15665,15739,15814,15892,15973,16055,16140,16227,16314,16402,16490,16560,16628,16694,16744,16791,16845,16904,16957,16994,17027,17062,17107,17163,17225,17292,17350,17407,17468,17533,17596,17664,17738,17818,17904,17994,18089,18187,18287,18390,18493,18595,18697,18799,18902,19005,19107,19209,19311,19412,19514,19615,19714,19813,19911,20008,20105,20199,20288,20374,20457,20522,50573,20611,20642,20677,20715,20756,20784,20814,20831};
      float[] VWTime = new float[367];
      int[] VWDistance = new int[367] {0,39,87,140,186,220,249,279,317,364,418,477,541,605,668,733,802,875,937,988,1032,1071,1113,1162,1216,1264,1306,1343,1379,1423,1474,1518,1555,1589,1632,1681,1736,1798,1864,1934,2007,2081,2155,2229,2302,2371,2435,2505,2576,2649,2724,2797,2872,2947,3022,3098,3174,3249,3324,3397,3471,3545,3615,3684,3757,3825,3875,3915,3949,3986,4030,4081,4110,4174,4243,4316,4391,4467,4543,4619,4696,4771,4842,4911,4975,5036,5090,5134,5179,5217,52551,5280,5309,5338,5372,5417,5470,5530,5594,5663,5734,5809,5883,5954,6017,6078,6144,6198,6239,6275,6317,6366,6421,6473,6511,6544,6578,6623,6671,6731,6795,6857,6924,6993,7051,7106,7163,7226,7295,7353,7400,7435,7463,7486,7516,7554,7601,7654,7715,7779,7848,7907,7952,7991,8031,8078,8127,8163,8200,8245,8297,8356,8422,8490,8561,8634,8708,8782,8850,8903,8945,8980,9080,9067,9116,9171,9232,9296,9363,9432,9502,9572,9643,9713,9785,9857,9929,9999,10069,10140,10210,10280,10349,10418,10487,10556,10625,10694,10763,10831,10895,10960,11025,11091,11157,11224,11292,11360,11428,11495,11555,11598,11630,11657,11688,11727,11772,11825,11882,11943,11992,12026,12053,12077,12102,12125,12151,12185,12228,12280,12338,12400,12466,12533,12599,12666,12733,12795,12851,12903,12952,13004,13052,13096,13138,13189,13248,13313,13380,13443,13501,13555,13611,13663,13718,13777,13825,13870,13921,13980,14032,14080,14129,14185,14234,14277,14324,14379,14434,14474,14510,14549,14597,14652,14704,14744,14781,14822,14849,14923,14986,15053,15122,15192,15262,15321,15372,15427,15481,15539,15604,15670,15737,15808,15879,15946,16013,16082,16152,16220,16291,16362,16433,16502,16565,16624,16682,16728,16773,16820,16874,16932,16975,17007,17039,17080,17128,17184,17244,17306,17361,17415,17473,17535,17600,17664,17727,17791,17856,17922,17988,18057,18126,18196,18267,18337,18407,18476,18546,18616,18686,18756,18825,18895,18964,19034,19102,16171,19238,19307,19375,19443,19512,19580,19648,19716,19785,19854,19923,19993,20063,20133,20203,20270,20337,20403,20468,20527,20575,20610,20639,20671,20709,20751,20784,20813,20822};

      int i;
      for (i=0; i<321; i++)
        {
          PorscheTime[i]=i;
        }
      for (i=0; i<367; i++)
        {
          VWTime[i]=i;
        }

      //Get P424 Lap Time and Lap distance
      float currentLapTime = lapTime.currentLapTime;
      float getLapDistance;
      getLapDistance=Project424.Telemetry424.m_lapDistance;

      //Variables to get time difference between P424 and 919 on one side, P424 and IDR on the other one
      string lapTimeDiffPo="--";
      string lapTimeDiffVW="--";

      //Calculation of lap time difference between 919/IDR and P424
      for (i=0; i<320; i++)
        {
          if (PorscheDistance[i]<getLapDistance & getLapDistance<PorscheDistance[i+1])
            {
            float PorscheTimeExtrapolation=PorscheTime[i]+((PorscheTime[i+1]-PorscheTime[i])/(PorscheDistance[i+1]-PorscheDistance[i]))*(getLapDistance-PorscheDistance[i]);
            lapTimeDiffPo = LapTimeComparison(PorscheTimeExtrapolation,currentLapTime).ToString("0.000");
            }
        }

      for (i=0; i<366; i++)
        {
          if (VWDistance[i]<getLapDistance & getLapDistance<VWDistance[i+1])
            {
            float VWTimeExtrapolation=VWTime[i]+((VWTime[i+1]-VWTime[i])/(VWDistance[i+1]-VWDistance[i]))*(getLapDistance-VWDistance[i]);
            lapTimeDiffVW = LapTimeComparison(VWTimeExtrapolation,currentLapTime).ToString("0.000");
            }
        }

      string text = "Time difference /919 : ";
      text += $"{lapTimeDiffPo}\n";
      text += "Time difference /IDR : ";
      text += $"{lapTimeDiffVW}";

        m_textBox.UpdateText(text);
      }

        public float LapTimeComparison(float reference, float proto)
          {
            float result = proto-reference;
            return result;
          }

    void OnGUI ()
    {
    m_textBox.OnGUI();
    }

    /*void UpdateTextProperties ()
      {
      m_style.font = font;
      m_style.fontSize = 16;
      m_style.normal.textColor = Color.white;

      m_bigStyle.font = font;
      m_bigStyle.fontSize = 20;
      m_bigStyle.normal.textColor = Color.white;
      }*/
}
}
