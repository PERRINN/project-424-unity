
// Integration with Dynisma motion platform simulator


using UnityEngine;
using System;
using System.IO;


namespace Perrinn424
{

public class DynismaSettings : MonoBehaviour
	{
	// When both MotionPlatform objects are specified, then the selector chooses one.
	// Otherwise, if theres only one available, it will be used.

	public enum MotionPlatformProtocol { DMG1, DMGS };
	public MotionPlatformProtocol motionPlatformProtocol = MotionPlatformProtocol.DMG1;

	[UnityEngine.Serialization.FormerlySerializedAs("motionPlatform")]
	public DynismaMotionPlatformDMG1 motionPlatformDMG1;
	public DynismaMotionPlatformDMGS motionPlatformDMGS;
	[Space(5)]
	public DynismaInputProvider inputProvider;
	public DynismaEyePointClient eyePointClient;
	public string fileName = "DynismaSettings.json";


	[Serializable]
	private class SavedSettings
		{
		public DynismaInputDevice.Settings inputSettings = new DynismaInputDevice.Settings();
		public DynismaMotionPlatformDMG1.Settings motionSettingsDMG1 = new DynismaMotionPlatformDMG1.Settings();
		public DynismaMotionPlatformDMGS.Settings motionSettingsDMGS = new DynismaMotionPlatformDMGS.Settings();
		public DynismaEyePointClient.Settings clientEyePointSettings = new DynismaEyePointClient.Settings();
		}


	void Awake ()
		{
		// Ensure dynisma components don't become enabled before we configure them.

		if (isActiveAndEnabled)
			{
			if (inputProvider != null) inputProvider.enabled = false;
			if (motionPlatformDMG1 != null) motionPlatformDMG1.enabled = false;
			if (motionPlatformDMGS != null) motionPlatformDMGS.enabled = false;
			if (eyePointClient != null) eyePointClient.enabled = false;
			}
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
			SavedSettings settings = JsonUtility.FromJson<SavedSettings>(json);

			// Copy settings to preserve current references

			if (inputProvider != null)
				{
				EdyCommonTools.ObjectUtility.CopyObjectOverwrite<DynismaInputDevice.Settings>(settings.inputSettings, ref inputProvider.settings);
				inputProvider.enabled = true;
				}

			if (motionPlatformDMG1 != null)
				{
				EdyCommonTools.ObjectUtility.CopyObjectOverwrite<DynismaMotionPlatformDMG1.Settings>(settings.motionSettingsDMG1, ref motionPlatformDMG1.settings);
				motionPlatformDMG1.enabled = motionPlatformDMGS == null || motionPlatformProtocol == MotionPlatformProtocol.DMG1;
				}

			if (motionPlatformDMGS != null)
				{
				EdyCommonTools.ObjectUtility.CopyObjectOverwrite<DynismaMotionPlatformDMGS.Settings>(settings.motionSettingsDMGS, ref motionPlatformDMGS.settings);
				motionPlatformDMGS.enabled = motionPlatformDMG1 == null || motionPlatformProtocol == MotionPlatformProtocol.DMGS;
				}

			if (eyePointClient != null)
				{
				EdyCommonTools.ObjectUtility.CopyObjectOverwrite<DynismaEyePointClient.Settings>(settings.clientEyePointSettings, ref eyePointClient.settings);
				eyePointClient.enabled = true;
				}
			}
		}


	[ContextMenu("Save current settings to file")]
	public void SaveCurrentSettings ()
		{
		SavedSettings settings = new SavedSettings()
			{
			inputSettings = inputProvider?.settings,
			motionSettingsDMG1 = motionPlatformDMG1?.settings,
			motionSettingsDMGS = motionPlatformDMGS?.settings,
			clientEyePointSettings = eyePointClient?.settings
			};

		string json = JsonUtility.ToJson(settings, prettyPrint: true);
		string path = Path.Combine(Application.persistentDataPath, fileName);
		try
			{
			File.WriteAllText(path, json);
			Debug.Log($"Dynisma settings saved to file.\nPath: [{path}]");
			}
		catch (Exception e)
			{
			Debug.LogWarning($"Error writing Dynisma settings to file: {e.Message}\nPath: [{path}]");
			}
		}
	}

}