
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

	// Motion platform components should be disabled in the vehicle prefab. We will enable them from here.

	[UnityEngine.Serialization.FormerlySerializedAs("motionPlatform")]
	public DynismaMotionPlatformDMG1 motionPlatformDMG1;
	public DynismaMotionPlatformDMGS motionPlatformDMGS;

	[Space(5)]
	public DynismaInputProvider inputProvider;
	public DynismaEyePointClient eyePointClient;
	[Space(5)]
	public string fileName = "DynismaSettings.json";


	[Serializable]
	private class SavedSettings
		{
		public DynismaInputDevice.Settings inputSettings = new DynismaInputDevice.Settings();
		public DynismaMotionPlatformDMG1.Settings motionSettingsDMG1 = new DynismaMotionPlatformDMG1.Settings();
		public DynismaMotionPlatformDMGS.Settings motionSettingsDMGS = new DynismaMotionPlatformDMGS.Settings();
		public DynismaEyePointClient.Settings clientEyePointSettings = new DynismaEyePointClient.Settings();
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

			// Copy settings to preserve current references.
			// Enable the corresponding motion platform script. They should be disabled in the GameObject.

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

			// Apply settings to these components and restart them if they're enabled.

			if (inputProvider != null && inputProvider.isActiveAndEnabled)
				{
				EdyCommonTools.ObjectUtility.CopyObjectOverwrite<DynismaInputDevice.Settings>(settings.inputSettings, ref inputProvider.settings);
				inputProvider.enabled = false;
				inputProvider.enabled = true;
				}

			if (eyePointClient != null && eyePointClient.isActiveAndEnabled)
				{
				// Apply settings and restart the component

				EdyCommonTools.ObjectUtility.CopyObjectOverwrite<DynismaEyePointClient.Settings>(settings.clientEyePointSettings, ref eyePointClient.settings);
				eyePointClient.enabled = false;
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