
// Integration with Motion Systems motion platform simulator


using UnityEngine;
using System;
using System.IO;


namespace Perrinn424
{

public class MotionSystemsSettings : MonoBehaviour
	{
	public MotionSystemsPlatform motionPlatform;
	public string fileName = "MotionSystemsSettings.json";


	void Awake ()
		{
		// Ensure the motion component is disabled beforehand

		if (motionPlatform != null) motionPlatform.enabled = false;
		}


	void OnEnable ()
		{
		// The motion platform component is required

		if (motionPlatform == null)
			{
			Debug.LogError("MotionSystemsSettings: motion platform component not configured. Component disabled.");
			enabled = false;
			return;
			}

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

		// Convert the json to settings and apply them

		if (!string.IsNullOrEmpty(json))
			{
			// Copy settings to preserve current references

			MotionSystemsPlatform.Settings settings = JsonUtility.FromJson<MotionSystemsPlatform.Settings>(json);
			EdyCommonTools.ObjectUtility.CopyObjectOverwrite<MotionSystemsPlatform.Settings>(settings, ref motionPlatform.settings);
			motionPlatform.enabled = true;
			}
		}


	[ContextMenu("Save current settings to file")]
	public void SaveCurrentSettings ()
		{
		MotionSystemsPlatform.Settings settings = new MotionSystemsPlatform.Settings();
		if (motionPlatform != null) settings = motionPlatform.settings;

		string json = JsonUtility.ToJson(settings, prettyPrint: true);
		string path = Path.Combine(Application.persistentDataPath, fileName);
		try
			{
			File.WriteAllText(path, json);
			Debug.Log($"Motion Systems settings saved to file.\nPath: [{path}]");
			}
		catch (Exception e)
			{
			Debug.LogWarning($"Error writing Motion Systems settings to file: {e.Message}\nPath: [{path}]");
			}
		}
	}

}