//==================================================================================================
// (c) Angel Garcia "Edy" - Oviedo, Spain
// http://www.edy.es
//
// Cross-version display utilities
//==================================================================================================


using UnityEngine;


namespace VersionCompatibility
{

public static class UnityDisplay
	{
	public static int screenRefreshRateInt => Mathf.RoundToInt(screenRefreshRate);

	#if UNITY_6000_0_OR_NEWER
	public static float screenRefreshRate => (float)Screen.currentResolution.refreshRateRatio.value;
	#else
	public static float screenRefreshRate => Screen.currentResolution.refreshRate;
	#endif
	}

}