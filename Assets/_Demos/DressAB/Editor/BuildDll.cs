using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Player;
using UnityEngine;

public class BuildDll
{
    [MenuItem("Tools/BuildWin64")]
    public static void BuildWin64()
    {
        var buildPath = "./Build/Win64.exe";
        if (System.IO.File.Exists(buildPath))
            System.IO.File.Delete(buildPath);
        BuildPipeline.BuildPlayer(new EditorBuildSettingsScene[0], buildPath, BuildTarget.StandaloneWindows64, BuildOptions.None);
        Application.OpenURL(new System.Uri(System.IO.Path.GetFullPath(buildPath)).AbsoluteUri);
    }
}
