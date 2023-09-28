
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;


class MyCustomBuildProcessor : IPostprocessBuildWithReport
	{
	public int callbackOrder => 0;

	public void OnPostprocessBuild(BuildReport report)
		{
		string sourcePath = "Assets/Hardware Integrations/Dynisma/Tools/Vioso/Plugins";
		string outputPath = Path.Combine(Path.GetDirectoryName(report.summary.outputPath), $"{Application.productName}_Data/Plugins/x86_64");

		if (report.summary.platform == BuildTarget.StandaloneWindows64)
			{
			CopyFile("_Startup_IG1.vwf", sourcePath, outputPath);
			CopyFile("_Startup_IG2.vwf", sourcePath, outputPath);
			CopyFile("_Startup_IG3.vwf", sourcePath, outputPath);
			CopyFile("_Startup_IG4.vwf", sourcePath, outputPath);
			CopyFile("_Startup_IG5.vwf", sourcePath, outputPath);
			CopyFile("VIOSOWarpBlend.ini", sourcePath, outputPath, true);
			}
		}

	void CopyFile (string fileName, string sourcePath, string targetPath, bool overwrite = false)
		{
		string sourceFile = Path.Combine(sourcePath, fileName);
		string targetFile = Path.Combine(targetPath, fileName);

		const int E_FILEALREADYEXISTS = -2147024816;

		try
			{
			Debug.Log($"Copying: [{sourceFile}] => [{targetFile}]");
			File.Copy(sourceFile, targetFile, overwrite);
			}
		catch (IOException err)
			{
			// Ignore "already exists" errors

			if (err.HResult != E_FILEALREADYEXISTS)
				Debug.Log($"[{err.HResult}] {err.Message}");
			}
		}
	}