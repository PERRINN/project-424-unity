
// Integration with Dynisma motion platform simulator


using UnityEngine;
using System;
using System.IO;


namespace Perrinn424
{

public class DynismaSettings : MonoBehaviour
	{
	public DynismaInputProvider inputProvider;
	public DynismaMotionPlatform motionPlatform;
	public string fileName = "DynismaSettings.json";


	[Serializable]
	private class SavedSettings
		{
		public DynismaInputDevice.Settings inputSettings = new DynismaInputDevice.Settings();
		public DynismaMotionPlatform.Settings motionSettings = new DynismaMotionPlatform.Settings();
		}


	void Awake ()
		{
		// Ensure dynisma components are disabled beforehand

		if (inputProvider != null) inputProvider.enabled = false;
		if (motionPlatform != null) motionPlatform.enabled = false;
		}


	void OnEnable ()
		{
		// Look for existing input file and read the json text

		string json;
		try
		 	{
			json = File.ReadAllText(Path.Combine(Application.persistentDataPath, fileName));
			}
		catch (Exception)
			{
			json = "";
			}

		// Convert the json to saved settings and apply them

		if (!string.IsNullOrEmpty(json))
			{
			SavedSettings savedSettings = JsonUtility.FromJson<SavedSettings>(json);

			// Copy settings to preserve current references

			if (inputProvider != null)
				{
				EdyCommonTools.ObjectUtility.CopyObjectOverwrite<DynismaInputDevice.Settings>(savedSettings.inputSettings, ref inputProvider.settings);

				inputProvider.enabled = true;
				}

			if (motionPlatform != null)
				{
				EdyCommonTools.ObjectUtility.CopyObjectOverwrite<DynismaMotionPlatform.Settings>(savedSettings.motionSettings, ref motionPlatform.settings);

				motionPlatform.enabled = true;
				}
			}
		}
	}

}