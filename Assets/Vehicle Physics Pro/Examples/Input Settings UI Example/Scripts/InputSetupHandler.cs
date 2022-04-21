//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

// InputSetupDialogHandler: configures input, shows input setup dialog, saves/loads input setup to file


using UnityEngine;
using System;
using System.IO;
using System.Collections;
using VehiclePhysics.InputManagement;


namespace VehiclePhysics.UI
{

public class InputSetupHandler : MonoBehaviour
	{
	// Reference to the input dialog to be handled

	public InputSetupDialog inputSetupDialog;

	// Apply this input mapping file when no valid setup file is found

	public InputMappingAsset defaultMapping;

	// Filename to save/read the input settings to/from.
	// Saved in the folder given by Application.persistentDataPath.

	public string fileName = "InputSetup.json";

	// Optional hot key to show the input dialog

	[Space(5)]
	public bool enableHotKey = false;
	public KeyCode hotKey = KeyCode.I;


	[Serializable]
	class InputSetupData
		{
		public InputMapping mapping = new InputMapping();
		public InputSettings settings = new InputSettings();
		}


	void OnEnable ()
		{
		UITools.Disable(inputSetupDialog);

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
			InputSetupData data = JsonUtility.FromJson<InputSetupData>(json);

			// Copy settings to preserve current reference
			EdyCommonTools.ObjectUtility.CopyObjectOverwrite<InputSettings>(data.settings, ref InputManager.instance.settings);

			// Apply mapping
			InputManager.instance.customizableMapping = data.mapping;
			InputManager.instance.ResetAllMappings();
			}

		// Apply default mapping if no actual bindings have been set

		if (InputManager.instance.customizableMapping == null || InputManager.instance.customizableMapping.bindings.Count == 0)
			{
			if (defaultMapping != null)
				{
				InputManager.instance.customizableMapping = defaultMapping.mapping;
				InputManager.instance.ResetAllMappings();
				}
			}
		}


	void Update ()
		{
		// Monitor hotkey

		if (enableHotKey)
			{
			if (Input.GetKey(hotKey) && !UITools.IsEnabled(inputSetupDialog))
				StartCoroutine(ShowSetupDialog(inputSetupDialog));
			}
		}


	IEnumerator ShowSetupDialog (Behaviour dialog)
		{
		// Show dialog and wait for closing

		UITools.Enable(dialog);
		yield return new WaitWhile(() => UITools.IsEnabled(dialog));

		// Dialog has applied the settings to InputManager.instance.
		// Save setup file with them.

		InputSetupData data = new InputSetupData()
			{
			mapping = InputManager.instance.customizableMapping,
			settings = InputManager.instance.settings,
			};

		string json = JsonUtility.ToJson(data, prettyPrint: true);
		string path = Path.Combine(Application.persistentDataPath, fileName);
		try
			{
			File.WriteAllText(path, json);
			}
		catch (Exception e)
			{
			Debug.LogWarning($"Error writing input settings to file: {e.Message}\nPath: [{path}]");
			}
		}
	}

}
