using System.Globalization;
using System.Threading;
using UnityEngine;


// This script corrects a bug in Unity .NET 4.x that uses local system's culture for numbers, dates,
// etc. instead of a neutral uniform culture. As result, numbers may have decimal commas instead of
// dots and such.
//
// The bug has been reported several times but no fix as for Unity 2018.3.
//
// https://forum.unity.com/threads/unity-ignores-culture-settings.587452/
// https://forum.unity.com/threads/editor-is-using-windows-locale-settings.442847/#post-3963928
// https://feedback.unity3d.com/suggestions/fix-localization-issues-with-cor
//
// Update 2019-12-09: Fixed in the editor in Unity 2018.4, but not in runtime, nor in ExecuteInEditorMode.


#if UNITY_EDITOR
[UnityEditor.InitializeOnLoad]
public static class FixCultureEditor
{
    static FixCultureEditor()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
    }
}
#endif


public static class FixCultureRuntime
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void FixCulture()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
    }
}

