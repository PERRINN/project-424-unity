//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2017 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

// THIS SCRIPT IS USED BY THE DEVS FOR UPDATING THE VPP SDK IN THE PROJECT.
// Warning: DON'T MESS WITH IT, OR IT MAY RENDER YOUR PROJECT UNUSABLE!

#if false


// When DEBUG_MODE is defined:
//	- No files are changed
//	- Supports non-existent DLL
//	- Parses a few files / matches only (allows to uncomment inner debug logs)

#define DEBUG_MODE


using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using VehiclePhysics.EditorCsvFileUtility;


namespace VehiclePhysics.EditorTools
{

public class DllProjectUtility
	{
	static string dllPath = @"Assets\Vehicle Physics Pro\Sdk\VehiclePhysics.dll";
	static string sdkPath = @"Assets\Vehicle Physics Pro\Sdk\VehiclePhysicsPro sdk-v10-alpha.csv";


	struct Class
		{
		public string className;
		public string classNamespace;
		public string GUID;
		public string fileID;
		}

	enum ReplacementMode { SourceCodeToDll, DllToSourceCode };


	static Dictionary<string, Class> m_guidReference;
	static string m_dllGUID;


	[MenuItem("Tools/Vehicle Physics/Install or update SDK DLL")]
	static void UseDllReferences ()
		{
		m_dllGUID = GetDllGUID();

		// Write the DLL GUID to file. The file name is the DLL name with extension txt.
		// This will be used later for removing the DLL references.

		File.WriteAllText(Path.ChangeExtension(dllPath, "txt"), m_dllGUID);

		// Load the class reference indexed by GUID.

		m_guidReference = LoadClassReference(sdkPath, IndexMode.GUID);
		Debug.Log("Class reference contains " + m_guidReference.Count + " entries indexed by GUID\nDLL GUID is " + m_dllGUID);

		ReplaceReferencesInProject(ReplacementMode.SourceCodeToDll);
		AssetDatabase.Refresh();
		}


	[MenuItem("Tools/Vehicle Physics/Uninstall SDK DLL")]
	static void UseSourceCodeReferences ()
		{
		// Read the DLL GUID. It has been saved previously when installing the DLL.
		//
		// Previously, it was read from the DLL itself (GetDllGUID) but right now the DLL
		// shouldn't be available, as it may be already conflicting with the source code.

		m_dllGUID = File.ReadAllText(Path.ChangeExtension(dllPath, "txt"));

		// Load the class reference indexed by fileID.

		m_guidReference = LoadClassReference(sdkPath, IndexMode.FileID);
		Debug.Log("Class reference contains " + m_guidReference.Count + " entries indexed by fileID\nDLL GUID is " + m_dllGUID);

		// Replace the refences in the project

		ReplaceReferencesInProject(ReplacementMode.DllToSourceCode);
		AssetDatabase.Refresh();
		}


	//----------------------------------------------------------------------------------------------


	static void ReplaceReferencesInProject (ReplacementMode mode)
		{
		System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
		timer.Start();

		// Find all .unity, .prefab and .asset files in the project

		string[] files = GetProjectFiles();

		// Replace references in the files

		string result = "";
		int totalReplaced = 0;
		int filesChanged = 0;

		foreach (string file in files)
			{
			int replaced = ReplaceReferencesInFile(file, mode);

			if (replaced > 0)
				{
				result += Path.GetFileName(file) + ": " + replaced + " references\n";
				filesChanged++;
				}

			totalReplaced += replaced;
			#if DEBUG_MODE
			// if (totalReplaced > 10) break;
			#endif
			}

		timer.Stop();

		Debug.Log("Replaced " + totalReplaced + " references in " + filesChanged + " files (from " + files.Length +  ") in " + timer.Elapsed.TotalSeconds.ToString("0.0") + " seconds:\n\n" + result);
		}


	// References:
	//	Regex replacement groups		https://stackoverflow.com/questions/5899794/match-and-replace
	//	Replacing contents in strings	https://stackoverflow.com/questions/6945255/c-sharp-replace-string-in-string#6945262


	static int ReplaceReferencesInFile (string file, ReplacementMode mode)
		{
		string[] fileLines = File.ReadAllLines(file);

		int matches = 0;

		for (int i = 0; i < fileLines.Length; i++)
			{
			string line = fileLines[i];

			// Process the lines that contain a guid

			if (line.IndexOf("guid:") >= 0)
				{
				// Look for a line with this format and extract the parameters:
				// "  m_Script: {fileID: 11500000, guid: bc1fb48e788e43c4f825c1da67319778, type: 3}"
				//
				// Regex: https://regex101.com/r/sUx5Yd/3

				string pattern = @"fileID:\s*([\d-]+),\s*guid:\s*([A-Fa-f0-9]+),\s*type:\s*([\d]+)";
				Match m = Regex.Match(line, pattern);

				// Send to the proper replacement method.

				bool replaced;
				if (mode == ReplacementMode.SourceCodeToDll)
					replaced = ReplaceReferencesToDll(m, ref fileLines[i]);
				else
					replaced = ReplaceReferencesToSourceCode(m, ref fileLines[i]);

				if (replaced) matches++;
				}
			}

		if (matches > 0)
			{
			#if !DEBUG_MODE
			// Using CR as line ending and CR at the end of file (Unity's default)
			File.WriteAllText(file, string.Join("\n", fileLines) + "\n");
			#else
			Debug.Log("Found: " + file + " (" + matches + ")\nDEBUG_MODE: References not changed");
			#endif
			}

		return matches;
		}


	static bool ReplaceReferencesToDll (Match m, ref string line)
		{
		if (!m.Success) return false;

		string fileID = m.Groups[1].Value;
		string GUID = m.Groups[2].Value;
		string type = m.Groups[3].Value;

		if (fileID == "11500000" && type == "3")
			{
			Class c;
			if (m_guidReference.TryGetValue(GUID, out c))
				{
				// Reference found! Replacing 11500000 and class GUID with fileID and dllGUID
				// Two-steps replacement to avoid the unlikely (but not impossible) case of a file id being contained in a guid.

				string replaced = line.Replace("11500000", "{1}").Replace(GUID, "{2}");
				replaced = replaced.Replace("{1}", c.fileID).Replace("{2}", m_dllGUID);

				#if DEBUG_MODE
				// Debug.Log(c.className + " [" + line + "] => ["  + replaced + "]");
				#endif

				line = replaced;
				return true;
				}
			}

		return false;
		}


	static bool ReplaceReferencesToSourceCode (Match m, ref string line)
		{
		if (!m.Success) return false;

		string fileID = m.Groups[1].Value;
		string GUID = m.Groups[2].Value;
		string type = m.Groups[3].Value;

		if (GUID == m_dllGUID && type == "3")
			{
			Class c;
			if (m_guidReference.TryGetValue(fileID, out c))
				{
				// Reference found! Replacing file id and dll GUID with 11500000 and class GUID.
				// Two-steps replacement to avoid the unlikely (but not impossible) case of a file id being contained in a guid.

				string replaced = line.Replace(fileID, "{1}").Replace(m_dllGUID, "{2}");
				replaced = replaced.Replace("{1}", "11500000").Replace("{2}", c.GUID);

				#if DEBUG_MODE
				// Debug.Log(c.className + " [" + line + "] => ["  + replaced + "]");
				#endif

				line = replaced;
				return true;
				}
			}

		return false;
		}


	//----------------------------------------------------------------------------------------------


	enum IndexMode { GUID, FileID }


	static Dictionary<string, Class> LoadClassReference (string file, IndexMode indexMode)
		{
		Dictionary<string, Class> collection = new Dictionary<string, Class>();

		using (CsvFileReader reader = new CsvFileReader(file))
			{
			CsvRow row = new CsvRow();
			while (reader.ReadRow(row))
				{
				Class classData;

				if (row.Count == 4)
					{
					classData.className = row[0];
					classData.classNamespace = row[1];
					classData.GUID = row[2];
					classData.fileID = row[3];

					if (indexMode == IndexMode.GUID)
						collection.Add(classData.GUID, classData);
					else
						collection.Add(classData.fileID, classData);
					}
				}
			}

		#if DEBUG_MODE
		string results = "DEBUG_MODE: Read " + collection.Count + " entries from file " + Path.GetFileName(file) + "\n\n";

		int i = 0;
		foreach (var item in collection)
			{
			Class c = item.Value;

			results += "#" + i.ToString("000") + "  " + c.className + (string.IsNullOrEmpty(c.classNamespace)? "" : " (" + c.classNamespace + ")");
			results += "   [" + c.fileID + ", " + c.GUID + "]";
			results += "\n";
			i++;
			}
		Debug.Log(results);
		#endif

		return collection;
		}


	static string[] GetProjectFiles ()
		{
		// Find all .unity, .prefab and .asset files in the project

		string[] files = Directory.GetFiles("Assets\\", "*.*", SearchOption.AllDirectories)
            .Where(s => s.EndsWith(".unity", StringComparison.OrdinalIgnoreCase)
			|| s.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)
			|| s.EndsWith(".asset", StringComparison.OrdinalIgnoreCase)).ToArray();

		#if DEBUG_MODE
		string result = "DEBUG_MODE: Total " + files.Length + " files to verify\n\n";
		for (int i=0; i<files.Length; i++)
			result += "#" + i.ToString("000") + "  " + files[i] + "\n";
		Debug.Log(result);
		#endif

		return files;
		}


	static string GetDllGUID ()
		{
		string GUID = AssetDatabase.AssetPathToGUID(dllPath);

		if (string.IsNullOrEmpty(GUID))
			{
			#if DEBUG_MODE
			GUID = "XXINVALIDGUIDXX";
			#else
			// GUID = "c22152d75ab87b646a3cce92fe8e232b";
			throw new Exception("Dll file not found (" + dllPath + "). Aborted.");
			#endif
			}

		return GUID;
		}
	}

}
#endif